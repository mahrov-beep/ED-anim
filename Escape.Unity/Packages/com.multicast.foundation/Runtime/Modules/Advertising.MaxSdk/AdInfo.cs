#if APPLOVIN_MAX_SDK
namespace Multicast.Modules.Advertising.MaxSdk {
    using System;
    using JetBrains.Annotations;

    [Serializable]
    public class AdInfo {
        public volatile string placement = "uninitialized";
        public volatile bool   succeed;
        public volatile bool   closed;
        public volatile string adNetwork;
        public volatile string adUnitId;

        [CanBeNull] public volatile string error;

        public static AdInfo Create(string placement) {
            return new AdInfo {
                placement = placement,
                succeed   = false,
                closed    = false,
                adNetwork = "loading",
                adUnitId  = "loading",
                error     = null,
            };
        }
    }
}
#endif