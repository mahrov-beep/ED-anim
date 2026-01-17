namespace Game.UI.Views.GameResults.Simple {
    using UniMob.UI;
    using Multicast;
    using Multicast.Numerics;
    using Shared;
    using Sirenix.OdinInspector;
    using UniMob.UI.Widgets;
    using UnityEngine;

    public class SimpleGameResultsView : AutoView<ISimpleGameResultsState> {
        [SerializeField, Required] private ViewPanel rewards;

        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("is_loadout_lost", () => this.State.IsLoadoutLost),
            this.Variable("rating_string", () => this.State.RatingString, "+123"),
            this.Variable("kills", () => this.State.Kills, 9),
            this.Variable("loadout_earnings", () => this.State.LoadoutEarnings, Cost.Create(cost => {
                cost.Add(SharedConstants.Game.Currencies.BADGES, 999);
                cost.Add(SharedConstants.Game.Currencies.CRYPT, 90);
            })),
            this.Variable("loadout_cost", () => this.State.LoadoutCost, Cost.Create(cost => {
                cost.Add(SharedConstants.Game.Currencies.BADGES, 999);
                cost.Add(SharedConstants.Game.Currencies.CRYPT, 90);
            })),
        };

        protected override AutoViewEventBinding[] Events => new[] {
            this.Event("continue", () => this.State.Continue()),
        };

        protected override void Render() {
            base.Render();

            this.rewards.Render(this.State.Rewards);
        }
    }

    public interface ISimpleGameResultsState : IViewState {
        bool IsLoadoutLost { get; }

        IState Rewards { get; }

        int    Kills           { get; }
        string RatingString    { get; }
        Cost   LoadoutEarnings { get; }
        Cost   LoadoutCost     { get; }

        void Continue();
    }
}