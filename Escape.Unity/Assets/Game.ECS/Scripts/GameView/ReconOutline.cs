namespace Game.ECS.Scripts.GameView {
    using System.Collections.Generic;  
    using Quantum;
    using Sirenix.OdinInspector;
    using UnityEngine;
    using UnityEngine.Pool;

    /// <summary>
    /// Компонент для управления outline эффектом разведывательной гранаты на врагах
    /// </summary>
    public class ReconOutline : MonoBehaviour {
        [Required, SerializeField] private Material outlineMaterial;  
        private bool isOutlineActive;
        private List<Renderer> cachedRenderers = new List<Renderer>();

        private void Awake() {
            CacheRenderers();
        }

        private void CacheRenderers() {
            cachedRenderers.Clear();
            
            var skinnedRenderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (var renderer in skinnedRenderers) {
                cachedRenderers.Add(renderer);
            }

            var meshRenderers = GetComponentsInChildren<MeshRenderer>(true);
            foreach (var renderer in meshRenderers) {
                cachedRenderers.Add(renderer);
            }
        }

        public void SetOutline(bool isActive) {
            if (isOutlineActive == isActive) {
                return;
            }

            isOutlineActive = isActive;
            ApplyOutline(isActive);
        }

        private void ApplyOutline(bool shouldAdd) {
            if (outlineMaterial == null) {
                return;
            }

            using (ListPool<Material>.Get(out var buffer)) {
                foreach (var renderer in cachedRenderers) {
                    if (renderer == null) continue;

                    renderer.GetSharedMaterials(buffer);
                    bool isDirty = false;
                    bool hasOutlineMaterial = false;

                    for (int i = 0; i < buffer.Count; i++) {
                        if (buffer[i] == outlineMaterial) {
                            hasOutlineMaterial = true;
                            break;
                        }
                    }

                    if (shouldAdd) {
                        if (!hasOutlineMaterial) {
                            buffer.Add(outlineMaterial);
                            isDirty = true;
                        }
                    } else {
                        if (hasOutlineMaterial) {
                            buffer.Remove(outlineMaterial);
                            isDirty = true;
                        }
                    }

                    if (isDirty) {
                        renderer.SetSharedMaterials(buffer);
                    }

                    buffer.Clear();
                }
            }
        }

        private void OnDestroy() {
            if (isOutlineActive) {
                SetOutline(false);
            }
        }
    }
}

