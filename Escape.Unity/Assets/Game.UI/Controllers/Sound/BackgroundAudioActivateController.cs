namespace Game.UI.Controllers.Sound {
    using System;
    using Cysharp.Threading.Tasks;
    using ECS.Systems.Sounds;
    using Multicast;

    [Serializable, RequireFieldsInit]
    public struct BackgroundAudioActivateControllerArgs : IDisposableControllerArgs {
    }

    public class BackgroundAudioActivateController : DisposableController<BackgroundAudioActivateControllerArgs> {
        [Inject] private BackgroundAudioSystem backgroundAudioSystem;

        protected override async UniTask Activate(Context context) {
            this.backgroundAudioSystem.SetBackgroundAudioState(true);
        }

        protected override async UniTask OnDisposeAsync(Context context) {
            var tween = this.backgroundAudioSystem.SetBackgroundAudioState(false);
            if (tween != null) {
                await tween;
            }
        }
    }
}