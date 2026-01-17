namespace Game.UI.Controllers.Features.MailBox {
    using System;
    using Cysharp.Threading.Tasks;
    using JetBrains.Annotations;
    using Multicast;
    using Shared.UserProfile.Commands.MailBox;
    using Sirenix.OdinInspector;
    using UnityEngine;

    [Serializable, RequireFieldsInit]
    public struct MailBoxFeatureControllerArgs : IFlowControllerArgs {
    }

    public class MailBoxFeatureController : FlowController<MailBoxFeatureControllerArgs> {
        [CanBeNull] private IControllerBase mainBoxController;

        protected override async UniTask Activate(Context context) {
            MailBoxFeatureEvents.Open.Listen(this.Lifetime, () => this.RequestFlow(this.OpenMailBox));
            MailBoxFeatureEvents.Collect.Listen(this.Lifetime, args => this.RequestFlow(this.Collect, args));
        }

        private async UniTask OpenMailBox(Context context) {
            if (this.mainBoxController is { IsRunning: true }) {
                Debug.LogError($"[{this}] MailBox already opened");
                return;
            }

            this.mainBoxController = await context.RunChild(new MailBoxControllerArgs());
        }

        private async UniTask Collect(Context context, MailBoxFeatureEvents.CollectArgs args) {
            await using (await context.RunFadeScreenDisposable())
            await using (await context.RunProgressScreenDisposable("claiming")) {
                await context.Server.ExecuteUserProfile(new UserProfileMailBoxClaimMessageCommand {
                    MailMessageGuid = args.MessageGuid,
                }, ServerCallRetryStrategy.RetryWithUserDialog);
            }
        }

        [Button]
        private void RaiseOpenMailBox() {
            MailBoxFeatureEvents.Open.Raise();
        }
    }
}