namespace Multicast.Unity {
    using Sirenix.OdinInspector;
    using UnityEngine;
    using UnityEngine.UI;

    [RequireComponent(typeof(Image))]
    public sealed class ImageAlphaHitThreshold : MonoBehaviour {
        [ValidateInput(nameof(ValidateTarget))]
        [SerializeField, Required] private Image target = default;

        [SerializeField] private float threshold = 0.5f;

        private void Start() {
            this.target.alphaHitTestMinimumThreshold = this.threshold;
        }

        private void Reset() {
            this.target = this.GetComponent<Image>();
        }

        private bool ValidateTarget(Image it, ref string message) {
            if (it == null) {
                return true;
            }

            if (it.sprite == null) {
                message = "Sprite is null";
                return false;
            }

            if (!it.sprite.texture.isReadable) {
                message = "Sprite texture must be readable";
                return false;
            }

            return true;
        }
    }
}