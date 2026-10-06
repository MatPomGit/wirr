using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace KIA.WiRR.Editor
{
    internal enum WiRREnvironmentState
    {
        Ready,
        Warning,
        Missing
    }

    internal sealed class WiRREnvironmentItem
    {
        public string Label { get; }
        public WiRREnvironmentState State { get; }
        public string Detail { get; }
        public bool AutoRepairable { get; }

        public WiRREnvironmentItem(string label, WiRREnvironmentState state, string detail, bool autoRepairable = false)
        {
            Label = label;
            State = state;
            Detail = detail;
            AutoRepairable = autoRepairable;
        }
    }

    internal static class WiRREnvironmentPreflight
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(5);
        private static DateTime lastRefreshUtc = DateTime.MinValue;
        private static int cachedLab;
        private static List<WiRREnvironmentItem> cachedItems = new List<WiRREnvironmentItem>();

        public static IReadOnlyList<WiRREnvironmentItem> Get(int labNumber, bool force = false)
        {
            if (force || cachedLab != labNumber || DateTime.UtcNow - lastRefreshUtc > CacheDuration)
                Refresh(labNumber);
            return cachedItems;
        }

        public static bool HasAutoRepairableProblems(int labNumber) =>
            Get(labNumber).Any(item => item.AutoRepairable && item.State != WiRREnvironmentState.Ready);

        public static bool HasBlockingProblems(int labNumber) =>
            Get(labNumber).Any(item => item.State == WiRREnvironmentState.Missing);

        public static void AutoRepair(int labNumber)
        {
            var missingPackages = WiRRPackageInstaller.GetMissingPackagesForLab(labNumber);
            WiRRActivityLogger.Record(
                "environment_auto_repair_requested",
                labNumber,
                $"missing_upm={missingPackages.Count}");

            if (missingPackages.Count > 0)
                WiRRPackageInstaller.InstallForLab(labNumber);

            Refresh(labNumber);
        }

        public static void Refresh(int labNumber)
        {
            cachedLab = Math.Max(1, Math.Min(7, labNumber));
            lastRefreshUtc = DateTime.UtcNow;
            cachedItems = Build(cachedLab);
        }

        private static List<WiRREnvironmentItem> Build(int labNumber)
        {
            var items = new List<WiRREnvironmentItem>();

            var windows = Application.platform == RuntimePlatform.WindowsEditor;
            items.Add(new WiRREnvironmentItem(
                "System operacyjny",
                windows ? WiRREnvironmentState.Ready : WiRREnvironmentState.Warning,
                windows
                    ? $"Windows · {SystemInfo.operatingSystem}"
                    : $"{SystemInfo.operatingSystem}. Środowisko referencyjne kursu: Windows 11 64-bit."));

            var unityReference = Application.unityVersion.StartsWith("6000.6.", StringComparison.Ordinal);
            items.Add(new WiRREnvironmentItem(
                "Unity",
                unityReference ? WiRREnvironmentState.Ready : WiRREnvironmentState.Warning,
                unityReference
                    ? $"{Application.unityVersion} · zgodne z wersją referencyjną 6000.6.x"
                    : $"{Application.unityVersion} · kurs referencyjnie używa 6000.6.x"));

            var missingPackages = WiRRPackageInstaller.GetMissingPackagesForLab(labNumber);
            items.Add(new WiRREnvironmentItem(
                $"Pakiety Unity dla Lab {labNumber:00}",
                missingPackages.Count == 0 ? WiRREnvironmentState.Ready : WiRREnvironmentState.Missing,
                missingPackages.Count == 0
                    ? "Wszystkie wymagane zależności UPM są zainstalowane."
                    : "Brak: " + string.Join(", ", missingPackages),
                missingPackages.Count > 0));

            var git = CommandAvailable("git", "--version");
            items.Add(new WiRREnvironmentItem(
                "Git",
                git ? WiRREnvironmentState.Ready : WiRREnvironmentState.Missing,
                git ? "Dostępny w PATH." : "Nie znaleziono polecenia git. Git jest wymagany do wysyłania raportów."));

            var gh = CommandAvailable("gh", "--version");
            items.Add(new WiRREnvironmentItem(
                "GitHub CLI",
                gh ? WiRREnvironmentState.Ready : WiRREnvironmentState.Warning,
                gh
                    ? "Zainstalowany. Automatyczne tworzenie Pull Request może być dostępne po zalogowaniu."
                    : "Opcjonalny. Bez gh raport nadal może zostać wypchnięty, ale PR może wymagać utworzenia ręcznie."));

            AddAndroidStatus(items, labNumber);

            if (labNumber == 6)
            {
                var docker = CommandAvailable("docker", "--version") && CommandAvailable("docker", "compose version");
                items.Add(new WiRREnvironmentItem(
                    "Docker / WebSim",
                    docker ? WiRREnvironmentState.Ready : WiRREnvironmentState.Warning,
                    docker
                        ? "Docker i Docker Compose są dostępne."
                        : "Docker jest potrzebny dla wariantu WebSim. Klasyczny ROS 2/Gazebo pozostaje osobną ścieżką."));
            }

            return items;
        }

        private static void AddAndroidStatus(List<WiRREnvironmentItem> items, int labNumber)
        {
            var androidRelevant = labNumber == 1 || labNumber == 2 || labNumber == 3 ||
                                  labNumber == 4 || labNumber == 7;

            var androidSupport = BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android);
            var moduleState = androidSupport
                ? WiRREnvironmentState.Ready
                : androidRelevant ? WiRREnvironmentState.Missing : WiRREnvironmentState.Warning;
            items.Add(new WiRREnvironmentItem(
                "Android Build Support",
                moduleState,
                androidSupport
                    ? "Moduł platformy Android jest dostępny."
                    : "Brak modułu. Unity Hub → Installs → 6000.6.x → Add modules → Android Build Support."));

            var contents = EditorApplication.applicationContentsPath;
            var androidPlayer = Path.Combine(contents, "PlaybackEngines", "AndroidPlayer");
            var sdk = Directory.Exists(Path.Combine(androidPlayer, "SDK"));
            var ndk = Directory.Exists(Path.Combine(androidPlayer, "NDK"));
            var jdk = Directory.Exists(Path.Combine(androidPlayer, "OpenJDK")) ||
                      Directory.Exists(Path.Combine(androidPlayer, "JDK"));

            AddHubTool(items, "Android SDK", sdk, androidRelevant,
                "Unity Hub → Installs → 6000.6.x → Add modules → Android SDK & NDK Tools.");
            AddHubTool(items, "Android NDK", ndk, androidRelevant,
                "Unity Hub → Installs → 6000.6.x → Add modules → Android SDK & NDK Tools.");
            AddHubTool(items, "OpenJDK", jdk, androidRelevant,
                "Unity Hub → Installs → 6000.6.x → Add modules → OpenJDK.");
        }

        private static void AddHubTool(
            ICollection<WiRREnvironmentItem> items,
            string label,
            bool available,
            bool relevant,
            string missingInstruction)
        {
            items.Add(new WiRREnvironmentItem(
                label,
                available ? WiRREnvironmentState.Ready :
                    relevant ? WiRREnvironmentState.Missing : WiRREnvironmentState.Warning,
                available ? "Zainstalowany wraz z używaną wersją Unity." : missingInstruction));
        }

        private static bool CommandAvailable(string fileName, string arguments)
        {
            try
            {
                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = fileName,
                        Arguments = arguments,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                };

                process.Start();
                if (!process.WaitForExit(2500))
                {
                    try { process.Kill(); } catch { }
                    return false;
                }
                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
