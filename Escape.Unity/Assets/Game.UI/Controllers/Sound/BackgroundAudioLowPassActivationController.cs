namespace Game.UI.Controllers.Sound {
    using System;
    using Cysharp.Threading.Tasks;
    using ECS.Systems.Sounds;
    using JetBrains.Annotations;
    using Multicast;
    using UnityEngine;
    using UnityEngine.Audio;

    [Serializable, RequireFieldsInit]
    public struct BackgroundAudioLowPassActivationControllerArgs : IFlowControllerArgs {
    }

    public class BackgroundAudioLowPassActivationController : FlowController<BackgroundAudioLowPassActivationControllerArgs> {
        private const string BACKGROUND_LOWPASS_CUTOFF = "BackgroundAudio LowPass Cutoff";

        [Inject] private BackgroundAudioSystem backgroundAudioSystem;

        [CanBeNull] private AudioMixer audioMixer;

        protected override async UniTask Activate(Context context) {
            var group = this.backgroundAudioSystem.GetOutputAudioMixerGroup();

            if (group && group.audioMixer.GetFloat(BACKGROUND_LOWPASS_CUTOFF, out var prevLowPassCutoff)) {
                // revert to prev value of controller disposing
                this.Lifetime.Register(() => group.audioMixer.SetFloat(BACKGROUND_LOWPASS_CUTOFF, prevLowPassCutoff));

                this.audioMixer = group.audioMixer;
            }
        }

        protected override void OnUpdate() {
            base.OnUpdate();

            if (this.audioMixer) {
                this.audioMixer.GetFloat(BACKGROUND_LOWPASS_CUTOFF, out var current);
                this.audioMixer.SetFloat(BACKGROUND_LOWPASS_CUTOFF, Mathf.Lerp(current, 750, 3 * Time.unscaledDeltaTime));
            }
        }
    }
}