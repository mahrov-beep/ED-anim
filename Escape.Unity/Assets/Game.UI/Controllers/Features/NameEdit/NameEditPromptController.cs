namespace Game.UI.Controllers.Features.NameEdit {
    using System;
    using Cysharp.Threading.Tasks;
    using JetBrains.Annotations;
    using Multicast;
    using Multicast.Routes;
    using Game.Shared.DTO;
    using UI;
    using UniMob.UI.Widgets;
    using Widgets.EditName;

    [Serializable, RequireFieldsInit]
    public struct NameEditPromptControllerArgs : IFlowControllerArgs {
    }

    public class NameEditPromptController : FlowController<NameEditPromptControllerArgs> {
        private IUniTaskAsyncDisposable promptScreen;
        private IUniTaskAsyncDisposable fadeScreen;
        private string lastRejectedNickName;

        protected override async UniTask Activate(Context context) {
            await this.Open(context);
        }

        private async UniTask Open(Context context) {
            this.fadeScreen = await context.RunFadeScreenDisposable();
            this.promptScreen = await context.RunDisposable(new UiScreenControllerArgs {
                Route = UiConstants.Routes.EditName,
                Page = () => new EditNameDialogWidget {
                    RequestChangeNickName = newNickName => this.RequestChangeNickName(newNickName),
                    OnConfirm = result => {
                        if (result.confirmed) {
                            this.RequestFlow(this.ConfirmAndClose, result.newNickName);
                        }
                        else {
                            this.RequestFlow(this.Close);
                        }
                    },
                },
            });
        }

        private async UniTask<bool> RequestChangeNickName(string newNickName) {
            var tcs = new UniTaskCompletionSource<bool>();
            this.RequestFlow(async (Context context) => {
                var ok = await this.TryChangeNickName(context, newNickName);
                tcs.TrySetResult(ok);
            });
            return await tcs.Task;
        }

        private async UniTask<bool> TryChangeNickName(Context context, string newNickName) {
            if (newNickName == this.lastRejectedNickName) {
                return false;
            }
            try {
                var response = await context.Server.UserChangeNickName(new ChangeNickNameRequest {
                                newNick = newNickName,
                }, ServerCallRetryStrategy.RetryWithUserDialog);

                if (response.success) {
                    return true;
                }

                this.lastRejectedNickName = newNickName;
                return false;
            }
            catch {
                this.lastRejectedNickName = newNickName;
                return false;
            }
        }

        private async UniTask ConfirmAndClose(Context context, string newNickName) {
            this.RequestFlow(this.Close);
        }

        private async UniTask Close(Context context) {
            await this.promptScreen.DisposeAsync();
            await this.fadeScreen.DisposeAsync();
            this.Stop();
        }
    }
}