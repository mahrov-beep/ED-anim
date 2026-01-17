namespace Game.UI.Widgets.MainMenu {
    using Controllers.Features.CoinFarm;
    using Domain.CoinFarms;
    using Domain.Features;
    using Multicast;
    using UniMob.UI;
    using UnityEngine;
    using Views.MainMenu;

    [RequireFieldsInit]
    public class MainMenuCoinFarmWidget : StatefulWidget {
        public string CoinFarmKey;
    }

    public class MainMenuCoinFarmState : ViewState<MainMenuCoinFarmWidget>, IMainMenuCoinFarmState {
        [Inject] private CoinFarmsModel coinFarmsModel;
        [Inject] private FeaturesModel  featuresModel;

        private CoinFarmModel FarmModel => this.coinFarmsModel.Get(this.Widget.CoinFarmKey);

        public override WidgetViewReference View => default;

        public string CoinFarmKey => this.FarmModel.Key;
        public string CurrencyKey => this.FarmModel.CurrencyKey;

        public float CurrentProduceRatio {
            get {
                Ticker.TickEveryFrame();
                return this.FarmModel.CurrentProdiceRatio;
            }
        }

        public int ProduceQuantity => this.FarmModel.ProduceQuantity;

        public int CurrentStorageAmount => this.FarmModel.CurrentStorageAmount;
        public int MaxStorageAmount     => this.FarmModel.MaxStorageAmount;

        public bool CanCollect => this.FarmModel.IsCollectAllowed;

        public bool IsLocked {
            get {
                if (!this.featuresModel.IsFeatureUnlocked(this.FarmModel.LockedByFeatureKey)) {
                    return true;
                }

                return false;
            }
        }

        public int LockedByLevel {
            get {
                if (this.featuresModel.TryGetFeatureUnlockExpProgressionReward(this.FarmModel.LockedByFeatureKey, out var expProgressionRewardModel)) {
                    return expProgressionRewardModel.LevelToComplete +1;
                }

                return 0;
            }
        }

        public void Collect() {
            CoinFarmFeatureEvents.Collect.Raise(new CoinFarmFeatureEvents.CollectArgs {
                CoinFarmKey = this.FarmModel.Key,
            });
        }
    }
}