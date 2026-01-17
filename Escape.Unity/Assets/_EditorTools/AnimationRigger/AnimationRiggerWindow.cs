#if UNITY_EDITOR

namespace _EditorTools.AnimationRigger {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor;
    using UnityEditor;
    using UnityEngine;

    public class AnimationRiggerWindow : OdinEditorWindow {
        [ReadOnly] public ModelImporterAvatarSetup   ModelAvatarSetup   = ModelImporterAvatarSetup.CreateFromThisModel;
        [ReadOnly] public ModelImporterAnimationType ModelAnimationType = ModelImporterAnimationType.Human;

        [ReadOnly] public ClipAnimationMaskType AnimMaskType = ClipAnimationMaskType.CopyFromOther;
        [Required] public AvatarMask            AnimAvatarMask;

        [Required] public List<GameObject> Objects = new List<GameObject>();

        [MenuItem("Tools/Animation Rigger")]
        public static void Open() {
            var window = GetWindow<AnimationRiggerWindow>();
            window.titleContent = new GUIContent("Anim Rigger");
            window.Show();
        }

        protected override void OnEnable() {
            base.OnEnable();

            Selection.selectionChanged += this.SelectionChanged;
        }

        protected override void OnDisable() {
            base.OnDisable();

            Selection.selectionChanged -= this.SelectionChanged;
        }

        private void SelectionChanged() {
            var selected = Selection.objects;

            var assets = selected
                .Select(AssetDatabase.GetAssetPath)
                .SelectMany(path => AssetDatabase.IsValidFolder(path)
                    ? AssetDatabase.FindAssets("t:GameObject", new[] { path }).Select(AssetDatabase.GUIDToAssetPath)
                    : AssetDatabase.AssetPathExists(path)
                        ? new[] { path }
                        : Array.Empty<string>())
                .Select(AssetDatabase.LoadAssetAtPath<GameObject>)
                .Where(asset => asset != null);

            this.Objects.Clear();
            this.Objects.AddRange(assets);
        }

        [Button(ButtonSizes.Large)]
        public void AssignRigTypeAndAvatar() {
            try {
                foreach (var model in this.Objects) {
                    var path     = AssetDatabase.GetAssetPath(model);
                    var importer = AssetImporter.GetAtPath(path);

                    if (importer is not ModelImporter modelImporter) {
                        Debug.LogError($"Asset {model} is not model");
                        continue;
                    }

                    modelImporter.animationType = this.ModelAnimationType;
                    modelImporter.avatarSetup   = this.ModelAvatarSetup;
                    
                    modelImporter.SaveAndReimport();

                    var humanDesc = modelImporter.humanDescription;
                    humanDesc.hasTranslationDoF    = true;
                    modelImporter.humanDescription = humanDesc;

                    modelImporter.resampleCurves = true;

                    modelImporter.SaveAndReimport();

                    var clips = modelImporter.clipAnimations;

                    foreach (var clipImporter in clips) {
                        clipImporter.maskType   = this.AnimMaskType;
                        clipImporter.maskSource = this.AnimAvatarMask;

                        Debug.Log("Set mask for " + clipImporter.name);
                    }

                    modelImporter.clipAnimations = clips;

                    EditorUtility.SetDirty(modelImporter);

                    modelImporter.SaveAndReimport();

                    AssetDatabase.WriteImportSettingsIfDirty(path);
                    AssetDatabase.SaveAssetIfDirty(model);
                }
            }
            finally {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
    }
}

#endif