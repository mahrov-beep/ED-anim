namespace Game.UI.Controllers.Features.Gunsmith {
    using System;
    using Multicast;

    public static class GunsmithFeatureEvents {
        public static readonly EventSource Open  = new();
        public static readonly EventSource Close = new();

        public static readonly EventSource<BuyLoadoutArgs> BuyLoadout = new();
        
        [Serializable, RequireFieldsInit]
        public struct BuyLoadoutArgs {
            public string gunsmithKey;
            public string gunsmithLoadoutGuid;
        }
    }
}