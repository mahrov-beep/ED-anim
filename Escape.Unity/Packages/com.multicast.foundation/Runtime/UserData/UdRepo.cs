namespace Multicast.UserData {
    using System;
    using JetBrains.Annotations;

    public class UdRepo<TData> : UdObject
        where TData : UdObjectBase {
        public UdDict<TData> Lookup { get; }

        public UdRepo(UdArgs args, Func<UdArgs, TData> factory) : base(args) {
            this.Lookup = new UdDict<TData>(this.Child("list"), factory);
        }

        [PublicAPI]
        public TData Get(string key) {
            return this.Lookup.Get(key);
        }
    }
}