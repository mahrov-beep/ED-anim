namespace Game.UI.Controllers.Features.TraderShop {
    using Multicast;

    public static class TraderShopFeatureEvents {
        public static readonly EventSource Open  = new();
        public static readonly EventSource Close = new();

        public static readonly EventSource Sell = new();
    }
}