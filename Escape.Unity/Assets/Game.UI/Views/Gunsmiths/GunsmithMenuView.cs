namespace Game.UI.Views.Gunsmiths {
    using UniMob.UI;
    using Multicast;
    using Shared;
    using Sirenix.OdinInspector;
    using UniMob.UI.Widgets;
    using UnityEngine;

    public class GunsmithMenuView : AutoView<IGunsmithMenuState> {
        [SerializeField, Required] private ViewPanel loadouts;
        [SerializeField, Required] private ViewPanel header;

        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("gunsmith_key", () => this.State.GunsmithKey, SharedConstants.Game.Gunsmiths.GUNSMITH_1),
        };

        protected override AutoViewEventBinding[] Events => new[] {
            this.Event("close", () => this.State.Close()),
        };

        protected override void Render() {
            base.Render();

            this.loadouts.Render(this.State.Loadouts);
            this.header.Render(this.State.Header);
        }
    }

    public interface IGunsmithMenuState : IViewState {
        string GunsmithKey { get; }

        IState Loadouts { get; }
        IState Header   { get; }

        void Close();
    }
}