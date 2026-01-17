namespace Multicast.Localization {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor;
    using UnityEditor;
    using UnityEngine;
    using Utilities;

    internal class LocalizationAnalyzeWindow : EditorWindow {
        [MenuItem("Localization/Analyze/Analyze Text", priority = 500)]
        private static void OpenAnalyzeWindow() {
            var window = GetWindow<LocalizationAnalyzeWindow>();
            window.titleContent = new GUIContent("Localization Analyze");
            window.Show();
        }

        private List<(string, string)> infos = new List<(string, string)>();

        private void OnEnable() {
            AnalyzeText();
        }

        private void OnGUI() {
            foreach (var (key, value) in this.infos) {
                if (string.IsNullOrEmpty(value)) {
                    continue;
                }

                GUILayout.BeginHorizontal();
                GUILayout.Label(key, GUILayout.Width(100));
                GUILayout.TextArea(value, GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();
            }
        }

        public void AnalyzeText() {
            this.infos.Clear();

            var cache = EditorAddressablesCache<LocalizationTable>.Instance;
            var paths = EditorAddressablesUtils
                .EnumeratePathsByLabel(AppConstants.AddressableLabels.LOCALIZATION)
                .ToArray();

            var tables = Array.ConvertAll(paths, path => cache.Get(path));

            var langCharsMap = new Dictionary<string, HashSet<char>>();

            foreach (var table in tables) {
                if (!langCharsMap.ContainsKey(table.Lang)) {
                    langCharsMap.Add(table.Lang, new HashSet<char>());
                }

                foreach (var text in table.Values) {
                    foreach (var c in text) {
                        langCharsMap[table.Lang].Add(c);
                    }
                }
            }

            var sharedChars = new HashSet<char>();
            for (var i = 32; i <= 126; i++) {
                sharedChars.Add((char) i);
            }

            foreach (var kvp in langCharsMap) {
                kvp.Value.RemoveWhere(it => sharedChars.Contains(it));
            }

            var sb = new StringBuilder();
            sb.AppendLine("Localization text stats:");

            foreach (var kvp in langCharsMap) {
                var list = kvp.Value.ToList();
                foreach (var c in list) {
                    kvp.Value.Add(char.ToLower(c));
                    kvp.Value.Add(char.ToUpper(c));
                }
            }

            this.infos.Add(("shared", HashSetToString(sharedChars)));

            foreach (var (key, value) in langCharsMap) {
                this.infos.Add((key, HashSetToString(value)));
            }
        }

        private static string HashSetToString(HashSet<char> hashSet) {
            return new string(hashSet.OrderBy(it => (int) it).ToArray());
        }
    }
}