namespace Multicast.Boot {
    using Cysharp.Threading.Tasks;
    using Sirenix.OdinInspector;
    using UnityEngine;
    using UnityEngine.UI;

    public class PreloaderUI : MonoBehaviour {
        [SerializeField, Required] private CanvasGroup canvasGroup;
        [SerializeField, Required] private Slider      progressSlider;

        private float targetProgress;

        private void Awake() {
            ShowCanvasGroup(this.canvasGroup);
        }

        private void Update() {
            this.UpdateProgressSlider();
        }

        public void UpdateProgress(float progress) {
            this.targetProgress = progress;
        }

        public async UniTask AnimateHide() {
            await AnimateHideCanvasGroup(this.canvasGroup, 0.2f);
        }

        private static async UniTask AnimateHideCanvasGroup(CanvasGroup canvasGroup, float duration) {
            var v = 0f;
            while ((v += Time.deltaTime / duration) < 1f) {
                canvasGroup.alpha = 1f - v;
                await UniTask.Yield();
            }

            canvasGroup.alpha = 0f;
        }

        private static void ShowCanvasGroup(CanvasGroup canvasGroup) {
            canvasGroup.alpha = 1f;
        }

        private void UpdateProgressSlider() {
            this.progressSlider.value += Time.deltaTime * 0.1f;

            if (this.progressSlider.value < this.targetProgress) {
                this.progressSlider.value = Mathf.MoveTowards(
                    this.progressSlider.value, this.targetProgress, Time.deltaTime * 5f);
            }
        }
    }
}