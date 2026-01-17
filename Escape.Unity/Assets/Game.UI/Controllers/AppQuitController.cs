namespace Game.UI.Controllers {
    using System;
    using Cysharp.Threading.Tasks;
    using Multicast;
    using UnityEngine;
    using Widgets.Common;

    public class AppQuitController : ResultController<AppQuitControllerArgs> {
        protected override async UniTask Execute(Context context) {
            if (context.RootNavigator.TopmostRoute.Key == "gameplay") {
                return;
            }

            var quit = await context.RootNavigator.Alert(AlertDialogWidget.YesNo("QUIT_GAME"));

            if (!quit) {
                return;
            }

            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}