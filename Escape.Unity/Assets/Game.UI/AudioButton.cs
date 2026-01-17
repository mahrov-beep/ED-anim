namespace Game.UI.Scripts.Audio {
    using Domain;
    using Multicast;
    using SoundEffects;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    [RequireComponent(typeof(Button))]
    public class AudioButton : MonoBehaviour, IPointerClickHandler {
        [SerializeField] private string soundEffectKey = CoreConstants.SoundEffectKeys.Button;

        [SerializeField] private Button button;

        private ISoundEffectService soundEffectService = null;

        private void Reset() {
            this.button = this.GetComponent<Button>();
        }

        private void Start() {
            this.soundEffectService = App.Get<ISoundEffectService>();
        }


        public void OnPointerClick(PointerEventData eventData) {
            if (this.button != null && this.button.IsInteractable()) {
                this.soundEffectService?.PlayOneShot(this.soundEffectKey);
            }
        }
    }
}