namespace Multicast.ServerData {
    using Multicast.Numerics;

    public static class SdValueDefaults<T> {
        public static readonly T DefaultValue;

        static SdValueDefaults() {
            SdValueDefaults<bool>.DefaultValue = false;
            SdValueDefaults<string>.DefaultValue = "";
            SdValueDefaults<ProtectedInt>.DefaultValue = 0;
            SdValueDefaults<ProtectedBigDouble>.DefaultValue = 0;
            SdValueDefaults<BigDouble>.DefaultValue = BigDouble.Zero;
            SdValueDefaults<FixedDouble>.DefaultValue = FixedDouble.Zero;
            SdValueDefaults<GameTime>.DefaultValue = default;
            SdValueDefaults<Reward>.DefaultValue = Reward.None;
        }
    }
}
