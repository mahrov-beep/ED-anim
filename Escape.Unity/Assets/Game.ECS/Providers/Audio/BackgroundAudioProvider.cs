namespace Game.ECS.Providers.Audio {
    using Components.Audio;
    using DG.Tweening;
    using Scellecs.Morpeh.Providers;

    public class BackgroundAudioProvider : MonoProvider<BackgroundAudioComponent> {
        protected override void Initialize() {
            base.Initialize();

            ref var data = ref this.GetData();

            data.defaultVolume = data.audioSource.volume;

            data.audioSource.volume = 0f;
            data.audioSource.enabled = false;
        }

        protected override void Deinitialize() {
            ref var data = ref this.GetData();

            data.audioSource.DOKill();
            data.audioSource.volume = data.defaultVolume;

            base.Deinitialize();
        }
    }
}