using System;

namespace Multicast.ServerData {
    public class SdRepo<TData> : SdObject
        where TData : SdObjectBase {
        public SdDict<TData> Lookup { get; }

        public SdRepo(SdArgs args, Func<SdArgs, TData> factory) : base(args) {
            this.Lookup = new SdDict<TData>(this.Child(0), factory);
        }

        public TData Get(string key) {
            return this.Lookup.Get(key);
        }
    }
}