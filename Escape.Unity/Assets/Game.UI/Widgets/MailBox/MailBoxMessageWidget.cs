namespace Game.UI.Widgets.MailBox {
    using System;
    using System.Buffers;
    using System.Linq;
    using Controllers.Features.MailBox;
    using Domain.Storage;
    using Domain.UserData;
    using Multicast;
    using Quantum;
    using Rewards;
    using Shared;
    using Shared.UserProfile.Data;
    using Shared.UserProfile.Data.MailBox;
    using Shared.UserProfile.Helpers;
    using UniMob;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using Views.MailBox;

    [RequireFieldsInit]
    public class MailBoxMessageWidget : StatefulWidget {
        public string MailBoxMessageGuid;
    }

    public class MailBoxMessageState : ViewState<MailBoxMessageWidget>, IMailBoxMessageState {
        [Inject] private SdUserProfile userProfile;
        [Inject] private GameDef       gameDef;
        [Inject] private SdUserProfile gameData;
        [Inject] private StorageModel  storageModel;

        public override WidgetViewReference View => UiConstants.Views.MailBox.Message;

        private SdMailBoxMessage SdMessage => this.userProfile.MailBox.Messages.Get(this.Widget.MailBoxMessageGuid);

        public string TitleLocalized => this.SdMessage.Type.Value switch {
            SdMailBoxMessageTypes.QuestReward => "Quest Reward",
            SdMailBoxMessageTypes.LootBoxReward => "LootBox content",
            SdMailBoxMessageTypes.BetaTestReward => "Welcome bonus for new players!",
            _ => "Unknown",
        };

        public bool IsClaimed => this.SdMessage.Claimed.Value;
        
        public bool IsEnoughSpaceInStorage {
            get {
                var rewards = this.SdMessage.Reward.Value.EnumerateAllRewards()
                    .Where(it => it.ItemTypeIs(SharedConstants.RewardTypes.ITEM)).ToArray();

                return this.storageModel.CanAddItems(rewards.Select(it => it.ItemKey).ToArray());
            }
        }

        [Atom] public IState Rewards => this.RenderChild(_ => new HorizontalScrollGridFlow {
            MainAxisAlignment  = MainAxisAlignment.Start,
            CrossAxisAlignment = CrossAxisAlignment.Center,
            MaxCrossAxisCount  = 1,
            UseMask            = false,
            Children = {
                new Container { Size = WidgetSize.FixedWidth(50), },
                new RewardsRowWidget {
                    MainAxisSize = AxisSize.Min,
                    Rewards      = this.SdMessage.Reward.Value.EnumerateAllRewards(),
                },
                new Container { Size = WidgetSize.FixedWidth(50), },
            },
        });

        public void Claim() {
            MailBoxFeatureEvents.Collect.Raise(new MailBoxFeatureEvents.CollectArgs {
                MessageGuid = this.SdMessage.MessageGuid,
            });
        }
    }
}