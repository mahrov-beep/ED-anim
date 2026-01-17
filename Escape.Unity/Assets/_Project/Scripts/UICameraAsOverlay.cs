namespace Unity.Rendering {
    using Sirenix.OdinInspector;
    using UnityEngine;
    using UnityEngine.Rendering.Universal;

    [RequireComponent(typeof(Camera))]
    public class UICameraAsOverlay : MonoBehaviour {
        [SerializeField, Required] private Camera uiCamera;

        private Camera activeMainCamera;

        private void Reset() {
            this.uiCamera = this.GetComponent<Camera>();
        }

        private void Update() {
            var mainCamera = Camera.main;

            if (this.activeMainCamera == mainCamera) {
                return;
            }

            if (this.activeMainCamera) {
                var mainCameraData = this.activeMainCamera.GetUniversalAdditionalCameraData();
                mainCameraData.cameraStack.Remove(this.uiCamera);
            }

            this.activeMainCamera = mainCamera;

            if (this.activeMainCamera) {
                var mainCameraData = this.activeMainCamera.GetUniversalAdditionalCameraData();
                mainCameraData.cameraStack.Add(this.uiCamera);
            }

            var uiData = this.uiCamera.GetUniversalAdditionalCameraData();
            uiData.renderType = this.activeMainCamera ? CameraRenderType.Overlay : CameraRenderType.Base;
        }
    }
}