namespace Multicast.Boot.Steps {
    using System;
    using Cysharp.Threading.Tasks;
    using Multicast;

    [Serializable, RequireFieldsInit]
    public struct BackButtonControllerArgs : IFlowControllerArgs {
    }

    public class BackButtonController : FlowController<BackButtonControllerArgs> {
        protected override void OnUpdate() {
            if (App.Current != null && IsEscapePressed()) {
                this.RequestFlow(this.HandleBackButtonClick, FlowOptions.NowOrNever);
            }
        }

        private async UniTask HandleBackButtonClick(Context context) {
            var handled = App.Current.GetNavigator(AppNavigatorType.Root).HandleBack();

            if (handled) {
                return;
            }

            if (ControllersShared.IsControllerRegisteredForArgs<AppQuitControllerArgs>()) {
                await context.RunForResult(new AppQuitControllerArgs());
            }
        }

        private static bool IsEscapePressed() {
#if UNITY_INPUT_SYSTEM
            return UnityEngine.InputSystem.Keyboard.current[UnityEngine.InputSystem.Key.Escape].wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.Escape);
#endif
        }
    }
}