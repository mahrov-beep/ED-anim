namespace Game.UI.Views.Friends {
    using System;
    using UniMob.UI;
    using Multicast;
    using Sirenix.OdinInspector;
    using UniMob.UI.Widgets;
    using UnityEngine;

    public class IncomingFriendsPanelView : AutoView<IIncomingFriendsPanelState> {
        [SerializeField, Required] private ViewPanel requestsListPanel;

        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("has_requests", () => this.State.HasRequests),
        };

        protected override AutoViewEventBinding[] Events => new[] {
            this.Event("close", () => this.State.Close()),
            this.Event("accept_all", () => this.State.AcceptAll()),
            this.Event("decline_all", () => this.State.DeclineAll()),
        };

        protected override void Render() {
            base.Render();

            this.requestsListPanel.Render(this.State.RequestsList);
        }
    }

    public interface IIncomingFriendsPanelState : IViewState {
        IState RequestsList { get; }
        bool   HasRequests  { get; }

        void Close();
        void AcceptAll();
        void DeclineAll();
    }
}