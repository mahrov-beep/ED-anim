namespace Multicast {
    using System;
    using System.Collections.Generic;
    using Collections;
    using JetBrains.Annotations;
    using UniMob;
    using UserData;

    public abstract class KeyedMultiInstanceModel<TDef, TData, TModel> : KeyedModelBase<TDef, TData, TModel>
        where TDef : Def
        where TData : class, IDataObject
        where TModel : Model<TDef, TData> {
        protected KeyedMultiInstanceModel(
            Lifetime lifetime, LookupCollection<TDef> defs, IDataDict<TData> data, Func<TData, string> keySelector)
            : base(lifetime, defs, data, keySelector) {
        }

        protected KeyedMultiInstanceModel(Lifetime lifetime, DefAsset<TDef> defAsset, IDataDict<TData> data, Func<TData, string> keySelector)
            : this(lifetime, defAsset.GetLookup(), data, keySelector) {
        }

        [PublicAPI]
        public TModel GetByGuid(string guid) {
            this.EnsureAccessAllowed();

            if (!this.TryGetByGuid(guid, out var model)) {
                throw new InvalidOperationException($"Model of type {typeof(TModel).Name} with guid '{guid}' not exists");
            }

            return model;
        }

        [PublicAPI]
        public bool TryGetByGuid(string guid, out TModel model) {
            this.EnsureAccessAllowed();

            this.Version.Get();

            if (this.Models.TryGetValue(guid, out var pair)) {
                model = pair.Item2;
                return true;
            }

            model = default;
            return false;
        }

        [PublicAPI]
        [MustUseReturnValue]
        public IEnumerable<TModel> EnumerateByKey(string key) {
            this.EnsureAccessAllowed();

            this.Version.Get();

            for (var i = 0; i < this.ValuesModels.Count; i++) {
                var valueModel = this.ValuesModels[i];
                var valueKey   = this.KeySelector(valueModel.Data);

                if (valueKey == key) {
                    yield return valueModel;
                }
            }
        }
    }
}