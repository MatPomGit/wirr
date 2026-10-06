using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using KIA.WiRR;
using UnityEditor;
using UnityEngine;

namespace KIA.WiRR.Editor
{
    [InitializeOnLoad]
    internal static class WiRRActivityLogger
    {
        private const string LabPrefKey = "KIA.WiRR.SelectedLab";
        private const int MaxStoredEvents = 2000;
        private static readonly string Root = Path.GetFullPath(Path.Combine(Application.dataPath, "../Library/WiRRReports"));
        private static readonly string LogPath = Path.Combine(Root, "activity-log.json");
        private static DateTime? playModeStartedUtc;

        [Serializable]
        private sealed class LogFile
        {
            public List<WiRRActivityEvent> events = new List<WiRRActivityEvent>();
        }

        static WiRRActivityLogger()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.quitting += OnEditorQuitting;
            Record("editor_session_started", CurrentLab(), "ok");
        }

        public static void Record(string eventName, int labNumber, string result = "")
        {
            if (string.IsNullOrWhiteSpace(eventName))
                return;

            var log = Load();
            log.events.Add(new WiRRActivityEvent
            {
                eventName = eventName.Trim(),
                labNumber = Mathf.Clamp(labNumber, 1, 7),
                timestampUtc = DateTime.UtcNow.ToString("O"),
                result = result ?? string.Empty,
                durationSeconds = 0d
            });

            Trim(log);
            Save(log);
        }

        public static void RecordDuration(string eventName, int labNumber, DateTime startedUtc, string result = "")
        {
            var seconds = Math.Max(0d, (DateTime.UtcNow - startedUtc).TotalSeconds);
            var log = Load();
            log.events.Add(new WiRRActivityEvent
            {
                eventName = eventName,
                labNumber = Mathf.Clamp(labNumber, 1, 7),
                timestampUtc = DateTime.UtcNow.ToString("O"),
                result = result ?? string.Empty,
                durationSeconds = Math.Round(seconds, 3)
            });

            Trim(log);
            Save(log);
        }

        public static void AttachTo(WiRRReportDocument document)
        {
            if (document == null)
                return;

            var log = Load();
            var events = log.events
                .Where(e => e != null && e.labNumber == document.labNumber)
                .OrderBy(e => ParseUtc(e.timestampUtc))
                .ToList();

            document.activityTimeline = events;
            document.activitySummary = BuildSummary(events);
        }

        private static WiRRActivitySummary BuildSummary(IReadOnlyList<WiRRActivityEvent> events)
        {
            var summary = new WiRRActivitySummary
            {
                eventCount = events.Count,
                playModeSessions = events.Count(e => e.eventName == "play_mode_exited"),
                playModeSeconds = Math.Round(events.Where(e => e.eventName == "play_mode_exited").Sum(e => Math.Max(0d, e.durationSeconds)), 3)
            };

            if (events.Count == 0)
                return summary;

            var first = ParseUtc(events[0].timestampUtc);
            var last = ParseUtc(events[events.Count - 1].timestampUtc);
            summary.firstEventAtUtc = events[0].timestampUtc;
            summary.lastEventAtUtc = events[events.Count - 1].timestampUtc;
            if (first != DateTime.MinValue && last != DateTime.MinValue)
                summary.elapsedSeconds = Math.Round(Math.Max(0d, (last - first).TotalSeconds), 3);

            return summary;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            var lab = CurrentLab();
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                playModeStartedUtc = DateTime.UtcNow;
                Record("play_mode_entered", lab, "ok");
            }
            else if (state == PlayModeStateChange.ExitingPlayMode)
            {
                if (playModeStartedUtc.HasValue)
                    RecordDuration("play_mode_exited", lab, playModeStartedUtc.Value, "ok");
                else
                    Record("play_mode_exited", lab, "ok");
                playModeStartedUtc = null;
            }
        }

        private static void OnEditorQuitting()
        {
            Record("editor_session_ended", CurrentLab(), "ok");
        }

        private static int CurrentLab() => Mathf.Clamp(EditorPrefs.GetInt(LabPrefKey, 1), 1, 7);

        private static LogFile Load()
        {
            Directory.CreateDirectory(Root);
            if (!File.Exists(LogPath))
                return new LogFile();

            try
            {
                var parsed = JsonUtility.FromJson<LogFile>(File.ReadAllText(LogPath));
                if (parsed != null)
                {
                    parsed.events = parsed.events ?? new List<WiRRActivityEvent>();
                    return parsed;
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[WiRR] Nie udało się odczytać lokalnego logu aktywności: " + exception.Message);
            }

            return new LogFile();
        }

        private static void Save(LogFile log)
        {
            Directory.CreateDirectory(Root);
            File.WriteAllText(LogPath, JsonUtility.ToJson(log, true));
        }

        private static void Trim(LogFile log)
        {
            if (log.events.Count <= MaxStoredEvents)
                return;
            log.events.RemoveRange(0, log.events.Count - MaxStoredEvents);
        }

        private static DateTime ParseUtc(string value)
        {
            return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsed)
                ? parsed.ToUniversalTime()
                : DateTime.MinValue;
        }
    }
}
