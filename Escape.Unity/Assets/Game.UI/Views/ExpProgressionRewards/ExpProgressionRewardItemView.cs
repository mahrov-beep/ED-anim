namespace Game.UI.Views.ExpProgressionRewards {
    using UniMob.UI;
    using Multicast;
    using Sirenix.OdinInspector;
    using UniMob.UI.Widgets;
    using UnityEngine;

    public class ExpProgressionRewardItemView : AutoView<IExpProgressionRewardItemState> {
        [SerializeField, Required] private ViewPanel reward;

        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("level", () => this.State.Level, 5),
            this.Variable("selected", () => this.State.Selected),
            this.Variable("can_claim", () => this.State.CanClaim, true),
            this.Variable("claimed", () => this.State.Claimed),
        };

        protected override AutoViewEventBinding[] Events => new[] {
            this.Event("select", () => this.State.Select()),
        };

        protected override void Render() {
            base.Render();

            this.reward.Render(this.State.Reward);
        }
    }

    public interface IExpProgressionRewardItemState : IViewState {
        IState Reward { get; }

        int Level { get; }

        bool Selected { get; }
        bool CanClaim { get; }
        bool Claimed  { get; }

        void Select();
    }
}