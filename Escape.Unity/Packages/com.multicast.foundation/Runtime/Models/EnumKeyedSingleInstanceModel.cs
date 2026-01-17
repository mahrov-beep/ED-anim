namespace Multicast {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Collections;
    using JetBrains.Annotations;
    using UniMob;
    using UserData;

    public abstract class EnumKeyedSingleInstanceModel<TKey, TData, TModel> : KeyedSingleInstanceModel<Def, TData, TModel>
        where TKey : Enum
        where TData : UdObject
        where TModel : EnumModel<TKey, TData> {
        protected EnumKeyedSingleInstanceModel(Lifetime lifetime, IDataDict<TData> data)
            : base(lifetime, CreateDefs(), data) {
        }

        private static LookupCollection<Def> CreateDefs() {
            var list = EnumNames<TKey>.Values.Select(it => new Def {key = it.Key}).ToList();
            return new LookupCollection<Def>(list);
        }

        [PublicAPI]
        public TModel Get(TKey key) {
            return this.Get(EnumNames<TKey>.GetName(key));
        }

        [PublicAPI]
        public bool TryGet(TKey key, out TModel model) {
            return this.TryGet(EnumNames<TKey>.GetName(key), out model);
        }

        protected sealed override void ConfigureData(Def childDef, TData childData, bool created) {
            base.ConfigureData(childDef, childData, created);
            this.ConfigureData(EnumNames<TKey>.GetValue(childDef.key), childDef.key, childData, created);
        }

        protected virtual void ConfigureData(TKey childKey, string childKeyAsString, TData childData, bool created) {
        }
    }
}