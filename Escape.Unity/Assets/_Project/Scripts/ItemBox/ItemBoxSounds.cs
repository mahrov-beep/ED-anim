using UnityEngine;

namespace Escape.Audio {
    using System;
    using Photon.Deterministic;
    using Quantum;
    using Sirenix.OdinInspector;

    public class ItemBoxSounds : QuantumEntityViewComponent {
        [SerializeField, Required] private ItemBoxAudioPlayer audioPlayer;
        [SerializeField, Required] private ItemBoxConfig      config;

        private float itemBoxTimer;

        private void Start() {
            QuantumEvent.Subscribe<EventOpenItemBox>(this, this.OpenLootBoxSound, onlyIfActiveAndEnabled: true, onlyIfEntityViewBound: true);
            QuantumEvent.Subscribe<EventItemBoxItemsChanged>(this, this.OnItemBoxChanged, onlyIfActiveAndEnabled: true, onlyIfEntityViewBound: true);
        }

        public override void OnUpdateView() {
            var f = PredictedFrame;

            if (!TryGetPredictedQuantumComponent<TimerItemBoxMarker>(out var timerItemBoxMarker)) {
                return;
            }

            if (this.itemBoxTimer > 0) {
                this.itemBoxTimer -= Time.deltaTime;

                return;
            }

            this.itemBoxTimer = this.config.OpeningAudioIntervalSeconds;

            this.TimerOpening();
        }

        public void OpenLootBoxSound(EventOpenItemBox eventData) {
            if (this.EntityRef == eventData.itemBoxRef) {
                this.audioPlayer.PlayOneShot(ItemBoxAudioLayers.Interact, this.config.AudioClipsOpen);
            }
        }

        private void OnItemBoxChanged(EventItemBoxItemsChanged eventData) {
            if (this.EntityRef == eventData.itemBoxRef) {
                this.audioPlayer.PlayOneShot(ItemBoxAudioLayers.Interact, this.config.AudioClipsItemPickUp);
            }
        }

        public void TimerOpening() {
            this.audioPlayer.PlayOneShot(ItemBoxAudioLayers.Interact, this.config.AudioClipsOpening);
        }
    }
}