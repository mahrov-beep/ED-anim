namespace Multicast {
    using System;
    using JetBrains.Annotations;
    using UniMob;
    using UserData;

    public abstract class EnumModel<TKey, TData> : Model<Def, TData>
        where TKey : Enum
        where TData : class, IDataObject {
        [PublicAPI] public new TKey Key { get; }

        [PublicAPI] public string KeyAsString => base.Key;

        protected EnumModel(Lifetime lifetime, Def def, TData data) : base(lifetime, def, data) {
            this.Key = EnumNames<TKey>.GetValue(def.key);
        }
    }
}