namespace Game.UI.Controllers {
    using System;
    using Cysharp.Threading.Tasks;
    using JetBrains.Annotations;
    using Multicast;
    using Photon;
    using Scenes;
    using UniMob.UI.Widgets;
    using UnityEngine;

    [Serializable, RequireFieldsInit]
    public struct ScenesControllerArgs : IFlowControllerArgs {
    }

    public interface IScenesController {
        UniTask GoToEmpty(ControllerBase.Context callerContext);
        UniTask GoToMainMenu(ControllerBase.Context callerContext);
        UniTask GoToGameplay(ControllerBase.Context callerContext);
    }

    public class ScenesController : FlowController<ScenesControllerArgs>, IScenesController {
        [CanBeNull] private IUniTaskAsyncDisposable currentState;

        protected override async UniTask Activate(Context context) {
            var reconnectedToGame = await context.RunForResult(new ReconnectOnStartupControllerArgs {
                ScenesController = this,
            }, default(bool));

            if (!reconnectedToGame) {
                await this.GoToState(context, new MainMenuSceneControllerArgs {
                    ScenesController = this,
                });
            }
        }

        public async UniTask GoToEmpty(Context callerContext) {
            using (this.Experimental_GrabActivationFrom(callerContext, out var context)) {
                await this.ExitCurrentState(context);
            }
        }

        public async UniTask GoToMainMenu(Context callerContext) {
            using (this.Experimental_GrabActivationFrom(callerContext, out var context)) {
                await this.ExitCurrentState(context);
                //
                await this.GoToState(context, new MainMenuSceneControllerArgs {
                    ScenesController = this,
                });
            }
        }

        public async UniTask GoToGameplay(Context callerContext) {
            using (this.Experimental_GrabActivationFrom(callerContext, out var context)) {
                await this.ExitCurrentState(context);
                //
                await this.GoToState(context, new GameplaySceneControllerArgs {
                    ScenesController = this,
                });
            }
        }

        private async UniTask GoToState<TStateArgs>(Context context, TStateArgs args)
            where TStateArgs : struct, IDisposableControllerArgs {
            await this.ExitCurrentState(context);
            this.currentState = await context.RunDisposable(args);
        }

        private async UniTask ExitCurrentState(Context context) {
            if (this.currentState != null) {
                try {
                    await this.currentState.DisposeAsync();
                }
                catch (Exception ex) {
                    Debug.LogError(ex);
                }
                finally {
                    this.currentState = null;
                }
            }
        }
    }
}