#if UNITY_EDITOR

namespace _EditorTools {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Pool;

    public class AnimatorOverrideMigratorWindow : OdinEditorWindow {
        //[MenuItem("Tools/Animator Override Migrator")]
        public static void Open() {
            var window = GetWindow<AnimatorOverrideMigratorWindow>();
            window.titleContent = new GUIContent("OC Migrator");
            window.Show();
        }

        [Serializable]
        private struct Mapping {
            [ReadOnly] public AnimationClip From;

            public AnimationClip To;
        }

        [SerializeField, ReadOnly]
        [ListDrawerSettings(IsReadOnly = true)]
        private List<AnimatorOverrideController> controllers;

        [SerializeField]
        [TableList(IsReadOnly = true)]
        private List<Mapping> mapping;

        [SerializeField, Required]
        private RuntimeAnimatorController newController;

        [Button, EnableIf("@this.newController != null")]
        private void Migrate() {
            foreach (var oc in this.controllers) {
                using (ListPool<KeyValuePair<AnimationClip, AnimationClip>>.Get(out var list)) {
                    oc.GetOverrides(list);

                    for (int i = 0; i < list.Count; i++) {
                        if (list[i].Value != null) {
                            list[i] = new KeyValuePair<AnimationClip, AnimationClip>(this.GetMappedKeyClip(list[i].Key), list[i].Value);
                        }
                    }

                    oc.runtimeAnimatorController = this.newController;
                    oc.ApplyOverrides(list);
                    EditorUtility.SetDirty(oc);
                    AssetDatabase.SaveAssetIfDirty(oc);
                }
            }
        }

        [Button]
        private void CleanupOverrides() {
            var method = typeof(AnimatorOverrideController).GetMethod("PerformOverrideClipListCleanup", BindingFlags.Instance | BindingFlags.NonPublic);

            foreach (var oc in this.controllers) {
                method.Invoke(oc, null);
                EditorUtility.SetDirty(oc);
                AssetDatabase.SaveAssetIfDirty(oc);
                
            }
        }

        private AnimationClip GetMappedKeyClip(AnimationClip src) {
            foreach (var it in this.mapping) {
                if (it.From == src) {
                    return it.To;
                }
            }

            return null;
        }

        [Button]
        private void RefreshControllersFromSelection() {
            this.controllers = Selection.objects.OfType<AnimatorOverrideController>().ToList();

            this.mapping = this.controllers.SelectMany(it => {
                    using (ListPool<KeyValuePair<AnimationClip, AnimationClip>>.Get(out var list)) {
                        it.GetOverrides(list);
                        return list.ToList();
                    }
                })
                .Where(it => it.Value != null)
                .Select(it => it.Key)
                .Distinct()
                .Select(it => new Mapping { From = it, To = null })
                .ToList();
        }
    }
}

#endif