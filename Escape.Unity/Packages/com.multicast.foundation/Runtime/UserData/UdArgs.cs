namespace Multicast.UserData {
    public readonly struct UdArgs {
        internal UdArgs(UdObjectBase parent, string key) {
            this.Parent = parent;
            this.Key    = key;
        }

        public UdObjectBase Parent { get; }
        public string       Key    { get; }
    }
}