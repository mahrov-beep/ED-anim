namespace Game.Shared.UserProfile.Data.Gunsmiths {
    using System.Collections.Generic;
    using Multicast.Numerics;
    using Multicast.ServerData;

    public class SdGunsmith : SdArrayObject {
        public SdDict<SdGunsmithLoadout> Loadouts { get; }

        public SdValue<int> LastRefreshOnLevel { get; }

        public SdGunsmith(SdArgs args) : base(args) {
            this.Loadouts           = new SdDict<SdGunsmithLoadout>(this.Child(0), a => new SdGunsmithLoadout(a));
            this.LastRefreshOnLevel = this.Child(1);
        }
    }
}