namespace Multicast.Unity {
    using Sirenix.OdinInspector;
    using UnityEngine;

    [ExecuteInEditMode]
    public class ScaleCameraToMatchWidth : MonoBehaviour {
        [SerializeField, Required] private Camera cam;

        [SerializeField] private float baseSize = 10;
        [SerializeField] private float minSize  = 0;
        [SerializeField] private float maxSize  = 6;

        private void Update() {
            if (!this.enabled) {
                return;
            }

            if (this.cam == null) {
                return;
            }

            var size = Mathf.Clamp(this.baseSize * Screen.height / Screen.width, this.minSize, this.maxSize);

            if (Mathf.Approximately(this.cam.orthographicSize, size)) {
                return;
            }

            this.cam.orthographicSize = size;
        }
    }
}