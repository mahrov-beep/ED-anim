namespace Game.UI.Controllers.Features.Thresher {
    using System;
    using Multicast;

    public static class ThresherFeatureEvents {
        public static readonly EventSource Open  = new();
        public static readonly EventSource Close = new();

        public static readonly EventSource<LevelUpArgs> LevelUp = new();

        [Serializable, RequireFieldsInit]
        public struct LevelUpArgs {
            public string thresherKey;
        }
    }
}