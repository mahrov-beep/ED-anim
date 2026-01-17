namespace Game.Shared.UserProfile.Data {
    using System;
    using Multicast.ServerData;

    public class SdGameModesRepo : SdRepo<SdGameMode> {
        public SdValue<string> SelectedGameMode { get; }

        public SdGameModesRepo(SdArgs args, Func<SdArgs, SdGameMode> factory) : base(args, factory) {
            this.SelectedGameMode = this.Child(1);
        }
    }
}