namespace Multicast.Boot.Steps {
    using System;
    using CodeWriter.ViewBinding;
    using Cysharp.Threading.Tasks;
    using Multicast;
    using UnityEngine;

    [Serializable, RequireFieldsInit]
    public struct UnhandledErrorControllerArgs : IFlowControllerArgs {
    }

    public class UnhandledErrorController : FlowController<UnhandledErrorControllerArgs> {
        [RuntimeInitializeOnLoadMethod]
        private static void Setup() {
            ControllersShared.RegisterController<UnhandledErrorControllerArgs, UnhandledErrorController>();
        }

        // not implemented yet

        private static async UniTaskVoid ShowUnhandledErrorDialog() {
            var msg = BindingsLocalization.Localize("CORE_UNHANDLED_ERROR",
                "An error has occurred. The developers have already been notified of the issue and are working on a fix. Restart the game to continue playing");
            var ok = BindingsLocalization.Localize("CORE_UNHANDLED_ERROR_QUIT", "Quit");

            await NativeDialog.Confirm(Application.productName, msg, ok);

            Application.Quit();
        }
    }
}