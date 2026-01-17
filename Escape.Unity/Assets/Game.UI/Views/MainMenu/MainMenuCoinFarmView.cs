namespace Game.UI.Views.MainMenu {
    using UniMob.UI;
    using Multicast;
    using Shared;

    public class MainMenuCoinFarmView : AutoView<IMainMenuCoinFarmState> {
        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("coin_farm_key", () => this.State.CoinFarmKey, SharedConstants.Game.CoinFarms.COIN_FARM_BADGES),
            this.Variable("currency_key", () => this.State.CurrencyKey, SharedConstants.Game.Currencies.BADGES),

            this.Variable("storage_current_amount", () => this.State.CurrentStorageAmount, 43),
            this.Variable("storage_max_amount", () => this.State.MaxStorageAmount, 100),

            this.Variable("produce_quantity", () => this.State.ProduceQuantity, 2),
            this.Variable("current_produce_ratio", () => this.State.CurrentProduceRatio, 0.7f),

            this.Variable("can_collect", () => this.State.CanCollect, true),

            this.Variable("is_locked", () => this.State.IsLocked),
            this.Variable("locked_by_level", () => this.State.LockedByLevel, 99),
        };

        protected override AutoViewEventBinding[] Events => new[] {
            this.Event("collect", () => this.State.Collect()),
        };
    }

    public interface IMainMenuCoinFarmState : IViewState {
        string CoinFarmKey { get; }

        string CurrencyKey { get; }

        float CurrentProduceRatio { get; }

        int CurrentStorageAmount { get; }
        int MaxStorageAmount     { get; }

        int ProduceQuantity { get; }

        bool IsLocked      { get; }
        int  LockedByLevel { get; }

        bool CanCollect { get; }

        void Collect();
    }
}