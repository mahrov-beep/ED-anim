namespace Multicast.DropSystem {
    using System;
    using System.Linq;
    using JetBrains.Annotations;
    using UserData;

    public class UdDropRepo : UdObject {
        public UdDict<UdDrop> Dict { get; }

        public UdDropRepo(UdArgs args) : base(args) {
            this.Dict = new UdDict<UdDrop>(this.Child("list"), a => new UdDrop(a));
        }

        [PublicAPI]
        public bool IsEmpty => this.Dict.Count == 0;

        [PublicAPI]
        public UdDrop First => this.Dict.OrderBy(it => it.Index).First();

        [PublicAPI]
        public UdDrop Get(string guid) {
            return this.Dict.Get(guid);
        }
        
        [PublicAPI]
        public bool Contains(string guid) {
            return this.Dict.ContainsKey(guid);
        }

        [PublicAPI]
        public UdDrop Dequeue(string guid) {
            var drop = this.Dict.Get(guid);
            this.Dict.Remove(drop);
            return drop;
        }

        [PublicAPI]
        internal UdDrop CreateInternal(Drop drop, DropSourceType sourceType, string sourceName, string sourceKey) {
            var guid = Guid.NewGuid().ToString();
            var num = this.Dict.Count == 0
                ? 0
                : this.Dict.Max(it => it.Index) + 1;

            var udDrop = this.Dict.Create(guid);
            udDrop.Set(drop, num, sourceType, sourceName, sourceKey);
            return udDrop;
        }
    }

    public class UdDrop : UdObject {
        private readonly UdValue<Drop>   udDrop;
        private readonly UdValue<int>    udIndex;
        private readonly UdValue<int>    udSourceType;
        private readonly UdValue<string> udSourceName;
        private readonly UdValue<string> udSourceKey;

        [PublicAPI] public DropSourceType SourceType => (DropSourceType) this.udSourceType.Value;

        [PublicAPI] public string SourceName => this.udSourceName.Value;
        [PublicAPI] public string SourceKey  => this.udSourceKey.Value;

        [PublicAPI] public Drop   Drop => this.udDrop.Value;
        [PublicAPI] public string Guid => this.GetUdObjectKey();

        internal int Index => this.udIndex.Value;

        public UdDrop(UdArgs args) : base(args) {
            this.udDrop       = this.Child("drop");
            this.udIndex      = this.Child("ind");
            this.udSourceType = this.Child("src_type");
            this.udSourceName = this.Child("src_name");
            this.udSourceKey  = this.Child("src_key");
        }

        internal void Set(Drop drop, int index, DropSourceType sourceType, string sourceName, string sourceKey) {
            this.udDrop.Value       = drop;
            this.udIndex.Value      = index;
            this.udSourceType.Value = (int) sourceType;
            this.udSourceName.Value = sourceName;
            this.udSourceKey.Value  = sourceKey;
        }
    }

    public enum DropSourceType {
        InGame = 1,
        Ads    = 2,
        Iap    = 3,
    }
}