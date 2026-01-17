namespace Multicast.ServerData {
    using JetBrains.Annotations;

    public struct SdArgs {
        internal SdArgs(SdObjectBase parent, SdKey key, ISdObjectTracker tracker) {
            this.Key     = key;
            this.Parent  = parent;
            this.Tracker = tracker;
        }

        public             SdKey            Key     { get; }
        [CanBeNull] public SdObjectBase     Parent  { get; }
        [CanBeNull] public ISdObjectTracker Tracker { get; }
    }
}