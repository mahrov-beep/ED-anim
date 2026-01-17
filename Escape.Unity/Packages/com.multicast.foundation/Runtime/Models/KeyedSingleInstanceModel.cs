namespace Multicast {
    using System;
    using System.Collections.Generic;
    using Collections;
    using JetBrains.Annotations;
    using UniMob;
    using UserData;

    public abstract class KeyedSingleInstanceModel<TDef, TData, TModel> : KeyedModelBase<TDef, TData, TModel>, IModelWithUserDataConfigurator
        where TDef : Def
        where TData : class, IDataObject
        where TModel : Model<TDef, TData> {
        private readonly LookupCollection<TDef> defs;
        private readonly IDataDict<TData>       data;

        protected KeyedSingleInstanceModel(Lifetime lifetime, LookupCollection<TDef> defs, IDataDict<TData> data)
            : base(lifetime, defs, data, it => it.MyKey) {
            this.defs = defs;
            this.data = data;
        }

        protected KeyedSingleInstanceModel(Lifetime lifetime, DefAsset<TDef> defAsset, IDataDict<TData> data)
            : this(lifetime, defAsset.GetLookup(), data) {
        }

        protected bool AutoConfigureData { get; set; }

        void IModelWithUserDataConfigurator.ConfigureUserData() {
            this.ConfigureSelfUserData();

            if (this.AutoConfigureData) {
                foreach (var childDef in this.defs.Items) {
                    var childData = this.data.GetOrCreate(childDef.key, out var created);
                    this.ConfigureData(childDef, childData, created);
                }
            }
        }

        protected virtual void ConfigureSelfUserData() {
        }

        protected virtual void ConfigureData(TDef childDef, TData childData, bool created) {
        }

        [PublicAPI]
        public TModel Get(string key) {
            this.EnsureAccessAllowed();

            if (!this.TryGet(key, out var model)) {
                throw new InvalidOperationException($"Model of type {typeof(TModel).Name} with key '{key}' not exists");
            }

            return model;
        }

        [PublicAPI]
        public bool TryGet(string key, out TModel model) {
            this.EnsureAccessAllowed();

            this.Version.Get();

            if (this.Models.TryGetValue(key, out var pair)) {
                model = pair.model;
                return true;
            }

            model = default;
            return false;
        }
    }
}