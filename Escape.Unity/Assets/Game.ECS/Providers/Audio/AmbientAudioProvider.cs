namespace Game.ECS.Providers.Audio {
    using Components.Audio;
    using Scellecs.Morpeh.Providers;
    using UnityEngine;

    public class AmbientAudioProvider : MonoProvider<AmbientAudioComponent> {
        private void Reset() {
            ref var data = ref this.GetData();

            data.audioSource = GetComponent<AudioSource>();
            data.transform   = transform;
        }
    }
}