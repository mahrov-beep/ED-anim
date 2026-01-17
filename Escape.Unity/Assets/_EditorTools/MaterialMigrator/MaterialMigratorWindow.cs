#if UNITY_EDITOR

namespace _EditorTools {
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor;
    using UnityEditor;
    using UnityEngine;

    public class MaterialMigratorWindow : OdinEditorWindow {
        //[MenuItem("Tools/Material Migrator")]
        public static void Open() {
            var window = GetWindow<MaterialMigratorWindow>();
            window.titleContent = new GUIContent("Material Migrator");
            window.Show();
        }

        public Shader FromShader;
        public Shader ToShader;

        public List<Material> MaterialsToMigrate = new List<Material>();

        [Button("Scan in Selection")]
        public void ScanInSelection() {
            var selectedObject = Selection.activeObject;
            var selectedPath   = AssetDatabase.GetAssetPath(selectedObject);

            var materials = AssetDatabase.FindAssets("t: Material", new[] { selectedPath })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<Material>)
                .ToList();

            materials.RemoveAll(mat => mat.shader != this.FromShader);

            this.MaterialsToMigrate = materials;
        }

        [Button("Migrate!")]
        public void Migrate() {
            // Autodesk Interactive -> URP Lit conversion
            
            foreach (var material in this.MaterialsToMigrate) {
                // запоминаем старые текстуры
                var mainTex       = material.mainTexture;
                var metallicTex   = material.GetTexture("_SpecGlossMap");
                var color         = material.color;
                var emissionColor = material.GetColor("_EmissionColor");

                material.shader = this.ToShader;

                // применяем текстуры обратно
                material.mainTexture = mainTex;
                material.color       = color;
                material.SetTexture("_MetallicGlossMap", metallicTex);
                material.SetColor("_EmissionColor", emissionColor);

                EditorUtility.SetDirty(material);
                AssetDatabase.SaveAssetIfDirty(material);
            }
        }
    }
}

#endif