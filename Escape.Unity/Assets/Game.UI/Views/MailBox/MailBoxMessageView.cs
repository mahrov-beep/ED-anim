namespace Game.UI.Views.MailBox {
    using UniMob.UI;
    using Multicast;
    using Sirenix.OdinInspector;
    using UniMob.UI.Widgets;
    using UnityEngine;

    public class MailBoxMessageView : AutoView<IMailBoxMessageState> {
        [SerializeField, Required] private ViewPanel rewards;

        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("title_localized", () => this.State.TitleLocalized, "Title"),
            this.Variable("is_claimed", () => this.State.IsClaimed),
            this.Variable("is_enough_space_in_storage", () => this.State.IsEnoughSpaceInStorage),
        };

        protected override AutoViewEventBinding[] Events => new[] {
            this.Event("claim", () => this.State.Claim()),
        };

        protected override void Render() {
            base.Render();

            this.rewards.Render(this.State.Rewards);
        }
    }

    public interface IMailBoxMessageState : IViewState {
        string TitleLocalized { get; }

        bool IsClaimed              { get; }
        bool IsEnoughSpaceInStorage { get; }

        IState Rewards { get; }

        void Claim();
    }
}