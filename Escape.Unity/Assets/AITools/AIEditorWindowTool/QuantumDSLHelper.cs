using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;

namespace AITools.AIEditorWindowTool
{
    public class QuantumDSLHelper : AIEditorWindow
    {
        [MenuItem("GPTGenerated/" + nameof(QuantumDSLHelper))]
        public static void ShowWindow()
        {
            GetWindow<QuantumDSLHelper>("PathTest");
        }

        private DefaultAsset folder;
        private Vector2 scrollPosition;
        private List<ComponentData> components = new List<ComponentData>();

        private class ComponentData
        {
            public string Name;
            public string Code;
            public bool Expanded;
        }

        protected virtual void OnEnable()
        {
            base.OnEnable();
        }

        protected virtual void OnDisable()
        {
            base.OnDisable();
        }

        public override void OnGUI()
        {
            base.OnGUI();

            var newFolder = (DefaultAsset)EditorGUILayout.ObjectField("Root Directory", folder, typeof(DefaultAsset), false);
            if (newFolder != folder)
            {
                folder = newFolder;
                UpdateComponents();
                Repaint();
            }

            if (folder == null) return;

            if (GUILayout.Button("Copy to JSON"))
            {
                CopyToJson();
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            foreach (var c in components)
            {
                c.Expanded = EditorGUILayout.Foldout(c.Expanded, c.Name);
                if (c.Expanded)
                {
                    EditorGUILayout.TextArea(c.Code);
                }
            }
            EditorGUILayout.EndScrollView();
        }

        private void UpdateComponents()
        {
            components.Clear();
            if (folder == null) return;
            var path = AssetDatabase.GetAssetPath(folder);
            if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
            {
                var files = Directory.GetFiles(path, "*.qtn", SearchOption.AllDirectories);
                var pattern = @"component\s+(\w+)\s*\{([\s\S]*?)\}";
                foreach (var file in files)
                {
                    var text = File.ReadAllText(file);
                    foreach (Match match in Regex.Matches(text, pattern))
                    {
                        var name = match.Groups[1].Value.Trim();
                        var code = "component " + name + "\n{\n" + match.Groups[2].Value + "\n}";
                        components.Add(new ComponentData
                        {
                            Name = name,
                            Code = code,
                            Expanded = false
                        });
                    }
                }
            }
        }

        private void CopyToJson()
        {
            var sb = new StringBuilder();
            sb.Append("{");
            for (int i = 0; i < components.Count; i++)
            {
                sb.AppendFormat("\"{0}\": \"{1}\"", components[i].Name, EscapeToJson(components[i].Code));
                if (i < components.Count - 1) sb.Append(",");
            }
            sb.Append("}");
            EditorGUIUtility.systemCopyBuffer = sb.ToString();
        }

        private string EscapeToJson(string s)
        {
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }
    }
}