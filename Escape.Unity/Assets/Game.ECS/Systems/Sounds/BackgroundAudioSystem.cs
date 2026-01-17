namespace Game.ECS.Systems.Sounds {
    using Components.Audio;
    using DG.Tweening;
    using Scellecs.Morpeh;
    using UnityEngine;
    using UnityEngine.Audio;
    using UnityEngine.SceneManagement;
    using SystemBase = Scellecs.Morpeh.SystemBase;

    public class BackgroundAudioSystem : SystemBase {
        private SingletonFilter<BackgroundAudioComponent> backgroundAudioFilter;

        public override void OnAwake() {
            this.backgroundAudioFilter = this.World.FilterSingleton<BackgroundAudioComponent>();
        }

        public override void OnUpdate(float deltaTime) { }

        public AudioMixerGroup GetOutputAudioMixerGroup() {
            if (this.backgroundAudioFilter == null || this.backgroundAudioFilter.IsValid == false) {
                return null;
            }

            return this.backgroundAudioFilter.Instance.audioSource.outputAudioMixerGroup;
        }

        public Tweener SetBackgroundAudioState(bool state) {
            if (this.backgroundAudioFilter == null) {
                Debug.LogError("backgroundAudioSystem not initialized in scene " + SceneManager.GetActiveScene().name);
                return null;
            }

            if (!this.backgroundAudioFilter.IsValid) {
                Debug.LogError("No background sound found in scene " + SceneManager.GetActiveScene().name);
                return null;
            }

            if (state) {
                this.backgroundAudioFilter.Instance.audioSource.enabled = true;
            }
            
            return this.backgroundAudioFilter.Instance.audioSource.DOFade(state ? this.backgroundAudioFilter.Instance.defaultVolume : 0, state ? 1f : 0.3f)
                .OnComplete(() => {
                    this.backgroundAudioFilter.Instance.audioSource.enabled = state;
                });
        }
    }
}