namespace Game.UI.Controllers.Features.ExpProgressionRewards {
    using Multicast;

    public static class ExpProgressionRewardsFeatureEvents {
        public static readonly EventSource Open  = new();
        public static readonly EventSource Close = new();
    }
}