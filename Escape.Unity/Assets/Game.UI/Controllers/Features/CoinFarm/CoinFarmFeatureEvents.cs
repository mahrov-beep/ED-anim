namespace Game.UI.Controllers.Features.CoinFarm {
    using Multicast;

    public static class CoinFarmFeatureEvents {
        public static readonly EventSource<CollectArgs> Collect = new();
        
        [RequireFieldsInit]
        public struct CollectArgs {
            public string CoinFarmKey;
        }
    }
}