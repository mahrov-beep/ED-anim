namespace Game.UI.Controllers.Features.MailBox {
    using System;
    using Cysharp.Threading.Tasks;
    using Multicast;
    using Shared.UserProfile.Commands.MailBox;
    using Widgets.MailBox;

    [Serializable, RequireFieldsInit]
    public struct MailBoxControllerArgs : IFlowControllerArgs {
    }

    public class MailBoxController : FlowController<MailBoxControllerArgs> {
        private IUniTaskAsyncDisposable bgScreen;

        private IDisposableController mailBoxScreen;

        protected override async UniTask Activate(Context context) {
            this.bgScreen = await context.RunBgScreenDisposable();

            await using (await context.RunProgressScreenDisposable("fetching_messages")) {
                await context.Server.ExecuteUserProfile(new UserProfileMailBoxViewAllMessagesCommand(), ServerCallRetryStrategy.RetryWithUserDialog);
            }

            this.mailBoxScreen = await context.RunDisposable(new UiScreenControllerArgs {
                Route = UiConstants.Routes.MailBoxMenu,
                Page = () => new MailBoxMenuWidget {
                    OnClose = () => this.RequestFlow(this.Close),
                },
            });
        }

        private async UniTask Close(Context context) {
            await this.mailBoxScreen.DisposeAsync();
            await this.bgScreen.DisposeAsync();
            this.Stop();
        }
    }
}