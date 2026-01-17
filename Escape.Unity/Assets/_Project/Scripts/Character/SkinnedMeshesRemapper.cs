namespace _Project.Scripts.Character {
    using System;
    using System.Collections.Generic;
    using Sirenix.OdinInspector;
    using UnityEngine;
    using UnityEngine.Pool;

    [TypeInfoBox(
        "EDITOR компонент для выставления костей у SkinnedMesh.\n" +
        "Проблема: если переместить SkinnedMeshRenderer из одной иерархии в другую (например, с одного персонажа на другого)," +
        "то это не работает (меш остается привязан к старым костям)\n" +
        "Решение: выставить кодом bones у SkinnedMeshRenderer\n\n" +
        "ЭТО РАБОТАЕТ ТОЛЬКО ЕСЛИ НАЗВАНИЯ КОСТЕЙ СТРОГО СОВПАДАЮТ! Не совпадающие кости отображаются в MissingBones")]
    public class SkinnedMeshesRemapper : MonoBehaviour {
        [SerializeField, Required] private Animator  myAnimator;
        [SerializeField, Required] private Transform rootBone;

        private static readonly ObjectPool<HashSet<Transform>> TransformSetPool = new(() => new HashSet<Transform>(), actionOnRelease: it => it.Clear());

        [ShowInInspector]
        private SkinnedMeshRenderer[] MeshesToRemap {
            get {
                using (ListPool<SkinnedMeshRenderer>.Get(out var meshes)) {
                    this.GetComponentsInChildren(includeInactive: true, meshes);

                    meshes.RemoveAll(it => {
                        // SM наш потомок, но у него другой аниматор в родителях.
                        // Возможно это оружие, так что пропускаем
                        if (it.GetComponentInParent<Animator>() != this.myAnimator) {
                            return true;
                        }

                        // если rootBone корректная, то вероятно и все кости корректные и ремаппинг для этого объекта не нужен. Пропускаем
                        if (it.rootBone && it.rootBone.GetComponentInParent<Animator>() == this.myAnimator) {
                            return true;
                        }

                        return false;
                    });

                    return meshes.ToArray();
                }
            }
        }

        [ShowInInspector]
        private List<string> MissingBones {
            get {
                if (this.rootBone == null) {
                    return new List<string>();
                }

                using var _  = TransformSetPool.Get(out var hashSet);
                using var __ = ListPool<Transform>.Get(out var bones);

                foreach (var sm in this.MeshesToRemap) {
                    hashSet.Add(sm.rootBone);

                    foreach (var smBone in sm.bones) {
                        hashSet.Add(smBone);
                    }
                }

                this.SearchValidBones(bones);

                var result = new List<string>();

                foreach (var src in hashSet) {
                    // src != null - иногда в bones почему-то попадаются null
                    if (src && bones.Find(dst => dst.name == src.name) == null) {
                        result.Add(src.name);
                    }
                }

                return result;
            }
        }

        [Button]
        private void ReMap() {
            using var _ = ListPool<Transform>.Get(out var bones);

            this.SearchValidBones(bones);

            foreach (var sm in this.MeshesToRemap) {
                sm.rootBone = bones.Find(dst => sm.rootBone && dst.name == sm.rootBone.name);
                sm.bones    = Array.ConvertAll(sm.bones, src => bones.Find(dst => src && dst.name == src.name));
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(sm);
#endif
            }
        }

        private void SearchValidBones(List<Transform> bones) {
            this.rootBone.GetComponentsInChildren(includeInactive: true, bones);
            bones.RemoveAll(it => it.GetComponentInParent<Animator>() != this.myAnimator);
        }

        private void Reset() {
            this.myAnimator = this.GetComponent<Animator>();
        }
    }
}