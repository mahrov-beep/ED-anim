namespace Game.UI.Widgets.ExpProgressionRewards {
    using System;
    using System.Linq;
    using CodeWriter.ViewBinding;
    using Domain.ExpProgressionRewards;
    using Domain.items;
    using JetBrains.Annotations;
    using Multicast;
    using Multicast.Numerics;
    using Quantum;
    using Shared;
    using UniMob;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using UnityEngine;
    using Views.ExpProgressionRewards;

    [RequireFieldsInit]
    public class ExpProgressionRewardsScreenWidget : StatefulWidget {
        public Action OnClose { get; set; }
    }

    public class ExpProgressionRewardsScreenState : ViewState<ExpProgressionRewardsScreenWidget>, IExpProgressionRewardsScreenState {
        [Inject] private ExpProgressionRewardsModel expProgressionRewardsModel;
        [Inject] private ItemsModel                 itemsModel;

        private readonly GlobalKey<HorizontalScrollGridFlowState> rewardsGlobalKey = new();

        private readonly StateHolder rewardsState;

        public ExpProgressionRewardsScreenState() {
            this.rewardsState = this.CreateChild(this.BuildRewards);
        }

        public override WidgetViewReference View => UiConstants.Views.ExpProgressionRewards.Screen;

        [Atom] public string SelectedTitle {
            get {
                var rewards = this.expProgressionRewardsModel.Selected.RewardsPreview;
                return string.Join(" + ", rewards.Select(it => GetTitle(it)));

                string GetTitle(Reward reward) {
                    return reward.GetItemType() switch {
                        SharedConstants.RewardTypes.CURRENCY => BindingsLocalization.Localize($"CURRENCY_NAME_{reward.ItemKey}"),
                        SharedConstants.RewardTypes.ITEM => BindingsLocalization.Localize($"ITEM_NAME_{reward.ItemKey}"),
                        SharedConstants.RewardTypes.FEATURE => BindingsLocalization.Localize($"FEATURE_NAME_{reward.ItemKey}"),
                        _ => string.Empty,
                    };
                }
            }
        }

        public string SelectedDesc {
            get {
                var rewards = this.expProgressionRewardsModel.Selected.RewardsPreview;
                if (rewards.Count == 1 && rewards[0] is var reward) {
                    return reward.GetItemType() switch {
                        SharedConstants.RewardTypes.CURRENCY => $"x<space=2>{reward.IntAmount}",
                        SharedConstants.RewardTypes.ITEM => EnumNames<ERarityType>.GetName(this.itemsModel.Get(reward.ItemKey).ItemAsset.rarity),
                        SharedConstants.RewardTypes.FEATURE => BindingsLocalization.Localize($"FEATURE_DESC_{reward.ItemKey}"),
                        _ => string.Empty,
                    };
                }

                return string.Empty;
            }
        }

        [Atom] public string SelectedRarity {
            get {
                return EnumNames<ERarityType>.GetName(GetRarity());

                ERarityType GetRarity() {
                    var list = this.expProgressionRewardsModel.Selected.RewardsPreview;
                    if (list.Count == 1 && list[0] is var reward) {
                        return reward.GetItemType() switch {
                            SharedConstants.RewardTypes.ITEM => this.itemsModel.Get(reward.ItemKey).ItemAsset.rarity,
                            SharedConstants.RewardTypes.FEATURE => ERarityType.Legendary,
                            _ => ERarityType.Common,
                        };
                    }

                    return ERarityType.Common;
                }
            }
        }

        public bool SelectedIsLocked => this.expProgressionRewardsModel.Selected.IsUnlocked == false;

        public IState Rewards => this.rewardsState.Value;

        public void Close() {
            this.Widget.OnClose?.Invoke();
        }

        [PublicAPI] public bool IsScrollMounted => this.rewardsGlobalKey.CurrentState != null;

        [PublicAPI] public void ScrollToSelected() {
            if (this.expProgressionRewardsModel.Selected is { } selected) {
                this.rewardsGlobalKey.CurrentState.ScrollTo(Key.Of(selected.Key), 0.3f, 75f);
            }
        }

        private Widget BuildRewards(BuildContext context) {
            return new HorizontalScrollGridFlow {
                Key                = this.rewardsGlobalKey,
                MainAxisAlignment  = MainAxisAlignment.Start,
                CrossAxisAlignment = CrossAxisAlignment.Center,
                UseMask            = false,
                MaxCrossAxisCount  = 1,
                Padding            = new RectPadding(left: 75, right: 150, 0, 0),
                Children = {
                    this.expProgressionRewardsModel.All.Select(it => this.BuildRewardItem(it)),
                },
                BackgroundContent = this.BuildRewardsBackground(),
            };
        }

        private Widget BuildRewardItem(ExpProgressionRewardModel rewardModel) {
            return new ExpProgressionRewardItemWidget {
                ExpProgressionRewardKey = rewardModel.Key,

                Key = Key.Of(rewardModel.Key),
            };
        }

        private Widget BuildRewardsBackground() {
            return new ExpProgressionRewardsBackgroundWidget();
        }
    }
}