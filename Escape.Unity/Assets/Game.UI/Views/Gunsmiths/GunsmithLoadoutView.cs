namespace Game.UI.Views.Gunsmiths {
    using UniMob.UI;
    using Multicast;
    using Multicast.Numerics;
    using Shared;
    using Sirenix.OdinInspector;
    using UniMob.UI.Widgets;
    using UnityEngine;

    public class GunsmithLoadoutView : AutoView<IGunsmithLoadoutState> {
        [SerializeField, Required] private ViewPanel loadout;

        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("gunsmith_loadout_key", () => this.State.GunsmithLoadoutKey, SharedConstants.Game.GunsmithLoadouts.GUNSMITH_LOADOUT_DEFAULT),
            this.Variable("loadout_quality", () => this.State.LoadoutQuality, 99),
            this.Variable("can_buy", () => this.State.CanBuy, true),
            this.Variable("buy_cost", () => this.State.BuyCost, Cost.Create(cost => {
                cost.Add(SharedConstants.Game.Currencies.BADGES, 999);
                cost.Add(SharedConstants.Game.Currencies.CRYPT, 90);
            })),
        };

        protected override AutoViewEventBinding[] Events => new[] {
            this.Event("buy", () => this.State.Buy()),
        };

        protected override void Render() {
            base.Render();

            this.loadout.Render(this.State.Loadout);
        }
    }

    public interface IGunsmithLoadoutState : IViewState {
        string GunsmithLoadoutKey { get; }

        int LoadoutQuality { get; }

        bool CanBuy  { get; }
        Cost BuyCost { get; }

        IState Loadout { get; }

        void Buy();
    }
}