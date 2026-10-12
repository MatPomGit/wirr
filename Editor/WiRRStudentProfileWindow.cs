using System;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace KIA.WiRR.Editor
{
    // Local identity. Remote group membership requires instructor-controlled verification.
    internal static class WiRRStudentProfile
    {
        private static string Key(string suffix) =>
            "KIA.WiRR.Student." + Hash128.Compute(Application.dataPath) + "." + suffix;
        public static bool IsRegistered => !string.IsNullOrEmpty(EditorPrefs.GetString(Key("Index"), ""));
        public static string Index => EditorPrefs.GetString(Key("Index"), "");
        public static int Group => EditorPrefs.GetInt(Key("Group"), 0);
        public static string GroupLabel => Group == 1 ? "G1" : Group == 2 ? "G2" : "Nieprzypisana";
        public static string Identifier
        {
            get
            {
                var bytes = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(Index));
                return BitConverter.ToString(bytes).Replace("-", "").Substring(0, 16).ToLowerInvariant();
            }
        }

        public static bool TryRegister(string index, int group, out string error)
        {
            index = (index ?? "").Trim();
            if (index.Length < 5 || index.Length > 12 || !System.Linq.Enumerable.All(index, char.IsDigit))
            {
                error = "Numer indeksu musi mieć 5–12 cyfr.";
                return false;
            }
            if (group != 1 && group != 2)
            {
                error = "Wybierz grupę G1 albo G2.";
                return false;
            }
            if (IsRegistered)
            {
                error = "Profil jest już przypisany. Zmiana grupy wymaga prowadzącego.";
                return false;
            }
            EditorPrefs.SetString(Key("Index"), index);
            EditorPrefs.SetInt(Key("Group"), group);
            EditorPrefs.SetInt("KIA.WiRR.MissionGroup", group);
            error = null;
            return true;
        }
    }

    public sealed class WiRRStudentProfileWindow : EditorWindow
    {
        private string index = "";
        private int group;
        [MenuItem("WiRR/Student/Profil i grupa", priority = 11)]
        public static void Open()
        {
            var window = GetWindow<WiRRStudentProfileWindow>("Profil WiRR");
            window.minSize = new Vector2(400, 230);
            window.Show();
        }
        private void OnGUI()
        {
            EditorGUILayout.LabelField("Profil laboratoryjny", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Wpisz własny numer indeksu i grupę z planu zajęć. Grupa zostanie zablokowana w tym projekcie. Profil pozostaje lokalny.", MessageType.Info);
            if (WiRRStudentProfile.IsRegistered)
            {
                EditorGUILayout.LabelField("Numer indeksu", WiRRStudentProfile.Index);
                EditorGUILayout.LabelField("Grupa", WiRRStudentProfile.GroupLabel);
                EditorGUILayout.HelpBox("Przydział grupy jest stały. Korekta wymaga weryfikacji przez prowadzącego w systemie uczelnianym.", MessageType.None);
                return;
            }
            index = EditorGUILayout.TextField("Numer indeksu", index);
            group = EditorGUILayout.Popup("Grupa", group, new[] { "Wybierz", "G1", "G2" });
            if (GUILayout.Button("Zapisz profil i przypisz grupę"))
            {
                if (WiRRStudentProfile.TryRegister(index, group, out var error))
                {
                    Close();
                    WiRRMissionBoardWindow.Open();
                }
                else EditorUtility.DisplayDialog("Profil WiRR", error, "OK");
            }
        }
    }
}
