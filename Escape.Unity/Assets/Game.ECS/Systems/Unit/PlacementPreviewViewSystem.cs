namespace Game.ECS.Systems.Unit {
    using System.Collections.Generic;
    using Player;
    using Photon.Deterministic;
    using Quantum;
    using Services.Photon;
    using UnityEngine;
    using LastPreUpdateSystem = Game.ECS.Systems.Input.LastPreUpdateSystem;
    using Multicast;

    /// <summary>
    /// Показывает локальному игроку превью установки объекта, пока зажата кнопка способности.
    /// </summary>
    public sealed class PlacementPreviewViewSystem : LastPreUpdateSystem {
        [Inject] private PhotonService photonService;
        [Inject] private LocalPlayerSystem localPlayerSystem;
        private sealed class PreviewInstanceData {
            public GameObject GameObject;
            public Transform Transform;
            public MeshFilter MeshFilter;
            public MeshRenderer MeshRenderer;
        }

        private readonly Dictionary<PlacementPreviewAbilityItem, PreviewInstanceData> previewPool = new Dictionary<PlacementPreviewAbilityItem, PreviewInstanceData>();
        private PreviewInstanceData currentPreview;

        public override void Awake() { }

        public override void OnDispose() {
            DestroyAllPreviews();
        }
        public override unsafe void OnLastPreUpdate(float deltaTime) {
            if (!photonService.TryGetPredicted(out var frame)) {
                DeactivateCurrentPreview();
                return;
            }

            if (localPlayerSystem.HasNotLocalEntityRef(out var localRef)) {
                DeactivateCurrentPreview();
                return;
            }

            var unit = frame.GetPointer<Unit>(localRef);
            if (!frame.Exists(unit->AbilityRef)) {
                DeactivateCurrentPreview();
                return;
            }

            var ability = frame.GetPointer<Ability>(unit->AbilityRef);
            if (ability->IsOnCooldown || ability->IsDelayedOrActive || unit->IsWeaponChanging || unit->IsActiveWeaponReloading(frame)) {
                DeactivateCurrentPreview();
                return;
            }

            if (!frame.TryGetPointer(localRef, out InputContainer* input) || !input->ButtonAbilityIsDown) {
                DeactivateCurrentPreview();
                return;
            }

            var abilityAsset = frame.FindAsset(ability->Config) as PlacementPreviewAbilityItem;
            if (abilityAsset == null || abilityAsset.previewMesh == null || abilityAsset.previewMaterial == null) {
                DeactivateCurrentPreview();
                return;
            }

            if (!abilityAsset.TryCalculatePlacementTransform(frame, localRef, ability, out FPVector3 position, out FPQuaternion rotation)) {
                DeactivateCurrentPreview();
                return;
            }

            var preview = EnsurePreviewInstance(abilityAsset);

            rotation = abilityAsset.GetPreviewRotation(rotation);
            Quaternion unityRotation = rotation.ToUnityQuaternion();
            Vector3 unityPosition = position.ToUnityVector3();

            preview.Transform.SetPositionAndRotation(unityPosition, unityRotation);

            var previewScale = abilityAsset.previewScaleFP.ToUnityVector3();
            if (preview.Transform.localScale != previewScale) {
                preview.Transform.localScale = previewScale;
            }
        }

        PreviewInstanceData EnsurePreviewInstance(PlacementPreviewAbilityItem ability) {
            if (currentPreview != null) {
                if (previewPool.TryGetValue(ability, out var existingCurrent) && existingCurrent == currentPreview) {
                    ApplyPreviewAssets(ability, currentPreview);
                    if (!currentPreview.GameObject.activeSelf) {
                        currentPreview.GameObject.SetActive(true);
                    }
                    return currentPreview;
                }

                DeactivateCurrentPreview();
            }

            if (!previewPool.TryGetValue(ability, out var preview)) {
                preview = CreatePreviewInstance(ability);
                previewPool.Add(ability, preview);
            }

            if (!preview.GameObject.activeSelf) {
                preview.GameObject.SetActive(true);
            }

            ApplyPreviewAssets(ability, preview);
            currentPreview = preview;
            return preview;
        }

        PreviewInstanceData CreatePreviewInstance(PlacementPreviewAbilityItem ability) {
            var instance = new GameObject("PlacementPreviewView");
            instance.hideFlags = HideFlags.DontSave;
            instance.SetActive(true);

            var data = new PreviewInstanceData {
                GameObject = instance,
                Transform = instance.transform,
                MeshFilter = instance.AddComponent<MeshFilter>(),
                MeshRenderer = instance.AddComponent<MeshRenderer>()
            };

            data.MeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            data.MeshRenderer.receiveShadows = false;

            return data;
        }

        void DeactivateCurrentPreview() {
            if (currentPreview != null) {
                if (currentPreview.GameObject != null) {
                    currentPreview.GameObject.SetActive(false);
                }
                currentPreview = null;
            }
        }

        void DestroyAllPreviews() {
            foreach (var kv in previewPool) {
                if (kv.Value?.GameObject != null) {
                    GameObject.Destroy(kv.Value.GameObject);
                }
            }
            previewPool.Clear();
            currentPreview = null;
        }

        static void ApplyPreviewAssets(PlacementPreviewAbilityItem ability, PreviewInstanceData preview) {
            if (preview.MeshFilter != null && preview.MeshFilter.sharedMesh != ability.previewMesh) {
                preview.MeshFilter.sharedMesh = ability.previewMesh;
            }

            if (preview.MeshRenderer != null && preview.MeshRenderer.sharedMaterial != ability.previewMaterial) {
                preview.MeshRenderer.sharedMaterial = ability.previewMaterial;
            }
        }
    }
}
