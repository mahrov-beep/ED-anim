namespace Game.Shared.UserProfile.Data.Storage {
    using System;
    using Multicast.ServerData;

    public class SdStorageItemRepo : SdRepo<SdStorageItem> {
        public SdStorageItemRepo(SdArgs args, Func<SdArgs, SdStorageItem> factory) : base(args, factory) {
        }
    }
}