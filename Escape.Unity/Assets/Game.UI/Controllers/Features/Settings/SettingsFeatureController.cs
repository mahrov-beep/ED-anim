namespace Game.UI.Controllers.Features.Settings {
    using System;
    using Cysharp.Threading.Tasks;
    using Multicast;
    using Sirenix.OdinInspector;
    using UniMob.UI;
    using Widgets.Settings;   
    using Game.UI.Widgets.Common;
    using UnityEngine;

    [Serializable, RequireFieldsInit]
    public struct SettingsFeatureControllerArgs : IFlowControllerArgs {
    }

    public class SettingsFeatureController : FlowController<SettingsFeatureControllerArgs> {
        private IUniTaskAsyncDisposable settingsScreen;

        protected override async UniTask Activate(Context context) {
            SettingsFeatureEvents.Open.Listen(this.Lifetime, () => this.RequestFlow(this.OpenSettings));
            SettingsFeatureEvents.Close.Listen(this.Lifetime, () => this.RequestFlow(this.CloseSettings));
        }

        private async UniTask OpenSettings(Context context) {
            if (this.settingsScreen != null) {
                return;
            }

            this.settingsScreen = await context.RunDisposable(new UiScreenControllerArgs {
                Route = UiConstants.Routes.Settings,
                Page = BuildSettingsScreen,
                OnBackPerformed = () => SettingsFeatureEvents.Close.Raise(),
            });
        }

        private async UniTask CloseSettings(Context context) {
            if (this.settingsScreen == null) {
                return;
            }

            await this.settingsScreen.DisposeAsync();
            this.settingsScreen = null;
        }

        private static Widget BuildSettingsScreen() {
            return new GraphicsSettingsWidget {
                OnClose = () => SettingsFeatureEvents.Close.Raise(),
            };
        }
    }
}
