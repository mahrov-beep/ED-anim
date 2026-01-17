namespace InfimaGames.LowPolyShooterPack {
    using Sirenix.OdinInspector;
    using UnityEngine;
    using UnityEngine.Rendering.Universal;

    public class CharacterDepthCamera : MonoBehaviour {
        [SerializeField, Required] private CharacterBehaviour characterBehaviour;

        private void OnEnable() {
            if (this.characterBehaviour.GetCharacterType() != CharacterTypes.LocalView) {
                return;
            }

            if (Camera.main is var mainCamera && mainCamera && mainCamera.GetUniversalAdditionalCameraData() is var cameraData) {
                cameraData.cameraStack.Add(this.characterBehaviour.GetCameraDepth());
            }
        }

        private void OnDisable() {
            if (this.characterBehaviour.GetCharacterType() != CharacterTypes.LocalView) {
                return;
            }

            if (Camera.main is var mainCamera && mainCamera && mainCamera.TryGetComponent(out UniversalAdditionalCameraData cameraData)) {
                cameraData.cameraStack.Remove(this.characterBehaviour.GetCameraDepth());
            }
        }
    }
}