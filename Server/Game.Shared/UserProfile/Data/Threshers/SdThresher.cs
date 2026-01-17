namespace Game.Shared.UserProfile.Data.Threshers {
    using System.Collections.Generic;
    using Multicast.ServerData;

    public class SdThresher : SdArrayObject {
        public SdValue<int> Level { get; }

        public SdThresher(SdArgs args) : base(args) {
            this.Level = new SdValue<int>(this.Child(0), 1);
        }
    }
}