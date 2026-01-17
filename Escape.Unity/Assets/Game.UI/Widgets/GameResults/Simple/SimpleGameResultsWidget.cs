namespace Game.UI.Widgets.GameResults.Simple {
    using System;
    using System.Linq;
    using Multicast;
    using Multicast.Numerics;
    using Quantum;
    using Rewards;
    using Shared;
    using Shared.Balance;
    using Shared.UserProfile.Data;
    using UniMob;
    using UniMob.UI;
    using Views.GameResults.Simple;

    [RequireFieldsInit]
    public class SimpleGameResultsWidget : StatefulWidget {
        public string PlayedGameId;

        public Action OnContinue;
    }

    public class SimpleGameResultsState : ViewState<SimpleGameResultsWidget>, ISimpleGameResultsState {
        [Inject] private SdUserProfile userProfile;
        [Inject] private GameDef       gameDef;

        private readonly StateHolder rewardsState;
        private          Cost        loadoutEarnings;

        public SimpleGameResultsState() {
            this.rewardsState = this.CreateChild(_ => new RewardsRowWidget {
                Rewards   = this.RewardWithoutRating.EnumerateAllRewards(),
                Alignment = MainAxisAlignment.End,
            });
        }

        [Atom] private SdGameResult PlayedGame => this.userProfile.PlayedGames.Get(this.Widget.PlayedGameId);

        [Atom] private GameResults PlayerGameResults => this.PlayedGame.GameResult.Value;

        [Atom] private Reward RewardWithoutRating => this.PlayerGameResults.GetReward() is var reward && reward.AmountTypeIs(RewardAmountType.LootBox)
            ? Reward.LootBox(reward.GetItemType(), reward.ItemKey, reward.LootBoxRewards.Where(it => !IsRatingReward(it)).ToArray())
            : reward;

        [Atom]
        private int RatingAmount => this.PlayerGameResults.GetReward() is var reward && reward.AmountTypeIs(RewardAmountType.LootBox)
            ? reward.LootBoxRewards.Where(it => IsRatingReward(it)).Sum(it => it.IntAmount)
            : 0;

        public int Kills => this.PlayerGameResults.GetKills();

        public override WidgetViewReference View => UiConstants.Views.GameResults.Simple.Screen;

        public bool IsLoadoutLost => this.PlayerGameResults.GetIsLoadoutLost();

        public IState Rewards => this.rewardsState.Value;

        public string RatingString => this.RatingAmount > 0 ? $"+{this.RatingAmount}" : $"{this.RatingAmount}";

        public Cost LoadoutEarnings {
            get {
                if (!this.userProfile.Loadouts.TryFindByLockedForGame(this.PlayedGame.GameId, out var loadout)) {
                    return Cost.Empty;
                }

                var before = ItemBalance.CalculateSellCost(this.gameDef, loadout.LoadoutSnapshot.Value);
                var after  = ItemBalance.CalculateSellCost(this.gameDef, this.PlayerGameResults.GetLoadout());

                var delta = new IntCost((after - before).Where(it => it.Value > 0).ToDictionary(it => it.Key, it => it.Value));

                return delta;
            }
        }

        public Cost LoadoutCost => ItemBalance.CalculateSellCost(this.gameDef, this.PlayerGameResults.GetLoadout());

        public void Continue() {
            this.Widget.OnContinue?.Invoke();
        }

        private static bool IsRatingReward(Reward r) {
            return r.ItemTypeIs(SharedConstants.RewardTypes.CURRENCY) && r.ItemKey == SharedConstants.Game.Currencies.RATING;
        }
    }
}