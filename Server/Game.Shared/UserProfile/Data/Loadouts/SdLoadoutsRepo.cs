namespace Game.Shared.UserProfile.Data.Loadouts {
    using System;
    using MessagePack;
    using Multicast.ServerData;
    using Quantum;

    public class SdLoadoutsRepo : SdRepo<SdLoadout> {
        public SdValue<string> SelectedLoadout { get; }

        public SdLoadoutsRepo(SdArgs args, Func<SdArgs, SdLoadout> factory) : base(args, factory) {
            this.SelectedLoadout = this.Child(1);
        }

        public GameSnapshotLoadout GetSelectedLoadoutClone() {
            return GetLoadoutClone(this.SelectedLoadout.Value);
        }

        public GameSnapshotLoadout GetLoadoutClone(string loadoutGuid) {
            var loadout  = this.Lookup.Get(loadoutGuid);
            var original = loadout.LoadoutSnapshot.Value;
            return original.DeepClone();
        }

        public bool TryFindByLockedForGame(string lockedForBattle, out SdLoadout result) {
            foreach (var sdLoadout in this.Lookup) {
                if (sdLoadout.LockedForGame.Value == lockedForBattle) {
                    result = sdLoadout;
                    return true;
                }
            }

            result = null;
            return false;
        }
    }
}