namespace Game.UI.Views.Common {
    using UniMob.UI;
    using Multicast;
    using Multicast.Numerics;
    using Sirenix.OdinInspector;
    using UniMob.UI.Widgets;
    using UnityEngine;

    public class ConfirmationScreenView : AutoView<IConfirmationScreenState> {
        [SerializeField, Required] private ViewPanel content;

        protected override AutoViewEventBinding[] Events => new[] {
            this.Event("confirm", () => this.State.Confirm()),
            this.Event("decline", () => this.State.Decline()),
        };

        protected override void Render() {
            base.Render();

            this.content.Render(this.State.Content);
        }
    }

    public interface IConfirmationScreenState : IViewState {
        void   Confirm();
        void   Decline();
        IState Content { get; }
    }
}