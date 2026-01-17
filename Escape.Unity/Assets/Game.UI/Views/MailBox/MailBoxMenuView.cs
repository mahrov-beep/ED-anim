namespace Game.UI.Views.MailBox {
    using UniMob.UI;
    using Multicast;
    using Sirenix.OdinInspector;
    using UniMob.UI.Widgets;
    using UnityEngine;

    public class MailBoxMenuView : AutoView<IMailBoxMenuState> {
        [SerializeField, Required] private ViewPanel messages;
        [SerializeField, Required] private ViewPanel header;

        protected override AutoViewEventBinding[] Events => new[] {
            this.Event("close", () => this.State.Close()),
        };

        protected override void Render() {
            base.Render();

            this.messages.Render(this.State.Messages);
            this.header.Render(this.State.Header);
        }
    }

    public interface IMailBoxMenuState : IViewState {
        IState Messages { get; }
        IState Header   { get; }

        void Close();
    }
}