namespace Game.Domain.CoinFarms {
    using Multicast;
    using Multicast.Collections;
    using Shared;
    using Shared.Balance;
    using Shared.Defs;
    using Shared.UserProfile.Data;
    using Shared.UserProfile.Data.CoinFarms;
    using UniMob;

    public class CoinFarmsModel : KeyedSingleInstanceModel<CoinFarmDef, SdCoinFarm, CoinFarmModel> {
        public CoinFarmsModel(Lifetime lifetime, LookupCollection<CoinFarmDef> defs, SdUserProfile userProfile)
            : base(lifetime, defs, userProfile.CoinFarms.Lookup) {
            this.AutoConfigureData = true;
        }
    }

    public class CoinFarmModel : Model<CoinFarmDef, SdCoinFarm> {
        [Inject] private GameDef       gameDef;
        [Inject] private SdUserProfile userProfile;
        [Inject] private ITimeService  timeService;

        public CoinFarmModel(Lifetime lifetime, CoinFarmDef def, SdCoinFarm data) : base(lifetime, def, data) {
        }

        [Atom] private CoinFarmBalance Balance => new CoinFarmBalance(this.gameDef, this.userProfile, this.timeService);

        [Atom] public string CurrencyKey        => this.Def.CurrencyKey;
        [Atom] public string LockedByFeatureKey => this.Def.LockedByFeatureKey;

        [Atom] public int   ProduceQuantity      => this.Balance.GetCurrentProduceQuantity(this.Key);
        [Atom] public float CurrentProdiceRatio  => this.Balance.CalcCurrentProduceRatio(this.Key);
        [Atom] public int   CurrentStorageAmount => this.Balance.CalcCollectedRewardAmount(this.Key, out _);
        [Atom] public int   MaxStorageAmount     => this.Balance.GetCurrentStorageCapacity(this.Key);

        [Atom] public bool IsCollectAllowed => this.Balance.IsCollectAllowed(this.Key);
    }
}