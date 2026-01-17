namespace Game.UI.Controllers.Features.Quest {
    using System;
    using Cysharp.Threading.Tasks;
    using Domain.Quests;
    using JetBrains.Annotations;
    using Multicast;
    using Multicast.Routes;
    using Shared.UserProfile.Commands.Quests;
    using Sound;
    using UI;
    using UniMob.UI.Widgets;
    using UnityEngine;
    using Widgets.QuestMenu;

    [Serializable, RequireFieldsInit]
    public struct QuestMenuControllerArgs : IFlowControllerArgs {
    }

    public class QuestMenuController : FlowController<QuestMenuControllerArgs> {
        [Inject] private QuestDonateItemTasksModel questDonateItemTasksModel;

        private IUniTaskAsyncDisposable questScreen;
        private IUniTaskAsyncDisposable bgScreen;

        protected override async UniTask Activate(Context context) {
            await this.Open(context);

            QuestFeatureEvents.Close.Listen(this.Lifetime, () => this.RequestFlow(this.Close));

            QuestFeatureEvents.RevealQuest.Listen(this.Lifetime, args => this.RequestFlow(this.RevealQuest, args));
            QuestFeatureEvents.ClaimQuest.Listen(this.Lifetime, args => this.RequestFlow(this.ClaimQuest, args));
            QuestFeatureEvents.DonateItem.Listen(this.Lifetime, args => this.RequestFlow(this.DonateItem, args));
        }

        private async UniTask Open(Context context) {
            this.bgScreen = await context.RunBgScreenDisposable();
            await context.RunChild(new BackgroundAudioLowPassActivationControllerArgs());
            this.questScreen = await context.RunDisposable(new UiScreenControllerArgs {
                Route = UiConstants.Routes.QuestsMenu,
                Page = () => new QuestMenuWidget {
                    OnClose = () => this.RequestFlow(this.Close),
                },
            });
        }

        private async UniTask Close(Context context) {
            await this.questScreen.DisposeAsync();
            await this.bgScreen.DisposeAsync();
            this.Stop();
        }

        private async UniTask RevealQuest(Context context, QuestFeatureEvents.RevealQuestArgs args) {
            await using (await context.RunFadeScreenDisposable())
            await using (await context.RunProgressScreenDisposable("processing")) {
                await context.Server.ExecuteUserProfile(new UserProfileRevealQuestCommand {
                    QuestKey = args.questKey,
                }, ServerCallRetryStrategy.RetryWithUserDialog);
            }
        }

        private async UniTask ClaimQuest(Context context, QuestFeatureEvents.ClaimQuestArgs args) {
            await using (await context.RunFadeScreenDisposable())
            await using (await context.RunProgressScreenDisposable("processing")) {
                await context.Server.ExecuteUserProfile(new UserProfileClaimQuestCommand {
                    QuestKey = args.questKey,
                }, ServerCallRetryStrategy.RetryWithUserDialog);
            }
        }

        private async UniTask DonateItem(Context context, QuestFeatureEvents.DonateItemArgs args) {
            await using (await context.RunFadeScreenDisposable())
            await using (await context.RunProgressScreenDisposable("processing")) {
                var taskModel = this.questDonateItemTasksModel.Get(args.questDonateItemTaskKey);

                await context.Server.ExecuteUserProfile(new UserProfileDonateItemQuestsCommand {
                    QuestDonateItemTaskKey  = taskModel.Key,
                    StorageItemToDonateGuid = taskModel.PossibleStorageItemToDonateGuid,
                }, ServerCallRetryStrategy.RetryWithUserDialog);
            }
        }
    }
}