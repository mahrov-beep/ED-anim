namespace Multicast {
    using System;
    using System.Collections.Generic;
    using Collections;
    using JetBrains.Annotations;
    using UniMob;
    using UnityEngine;
    using UnityEngine.Pool;
    using UserData;

    public abstract class KeyedModelBase<TDef, TData, TModel> : Model
        where TDef : Def
        where TData : class, IDataObject
        where TModel : Model<TDef, TData> {
        [Inject] private readonly Func<Lifetime, TDef, TData, TModel> factory;

        private readonly LookupCollection<TDef> defs;
        private readonly IDataDict<TData>       data;

        private protected readonly Func<TData, string> KeySelector;
        private protected readonly MutableAtom<int>    Version;
        private protected readonly List<TModel>        ValuesModels = new();

        private protected readonly Dictionary<string, (LifetimeController lc, TModel model)> Models = new();

        private event Action<TModel> CreateCallback = delegate { };

        protected List<TModel> Values {
            get {
                this.EnsureAccessAllowed();

                this.Version.Get();

                return this.ValuesModels;
            }
        }

        protected KeyedModelBase(Lifetime lifetime, LookupCollection<TDef> defs, IDataDict<TData> data, Func<TData, string> keySelector)
            : base(lifetime) {
            this.defs        = defs;
            this.data        = data;
            this.KeySelector = keySelector;

            this.Version = Atom.Value(this.Lifetime, 0);
        }

        public override void Initialize() {
            base.Initialize();

            this.CreateModelsInternal();

            if (!this.Lifetime.IsDisposed) {
                this.Lifetime.RegisterToEvent<Action>(
                    e => this.data.SelfChanged += e,
                    e => this.data.SelfChanged -= e,
                    () => this.CreateModelsInternal()
                );
            }
        }

        private void CreateModelsInternal() {
            using var _ = Atom.NoWatch;

            using (ListPool<(LifetimeController, TModel)>.Get(out var removedValues)) {
                foreach (var valueModel in this.ValuesModels) {
                    var itemGuid = valueModel.Data.MyKey;

                    if (this.data.TryGetValue(itemGuid, out var existData)) {
                        if (existData == valueModel.Data) {
                            continue;
                        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                        if (valueModel.Data != existData) {
                            Debug.LogError($"Data mismatch found in '{this.GetType().Name}' for identifier '{existData.MyKey}'. " +
                                           $"Probably identifier was reused. Please use unique identifiers for models");
                        }
#endif
                    }

                    if (this.Models.TryGetValue(itemGuid, out var pair)) {
                        removedValues.Add(pair);
                    }
                }

                foreach (var (lc, removedValue) in removedValues) {
                    var itemGuid = removedValue.Data.MyKey;

                    lc.Dispose();

                    this.Models.Remove(itemGuid);
                    this.ValuesModels.Remove(removedValue);
                }

                if (removedValues.Count > 0) {
                    this.Version.Invalidate();
                }
            }

            using (ListPool<(LifetimeController, TModel)>.Get(out var newValues)) {
                foreach (var itemData in this.data) {
                    var itemGuid = itemData.MyKey;

                    if (this.Models.ContainsKey(itemGuid)) {
                        continue;
                    }

                    var itemKey = this.KeySelector(itemData);

                    if (!this.defs.TryGet(itemKey, out var itemDef)) {
                        continue;
                    }

                    var lc = this.Lifetime.CreateNested();

                    TModel itemModel;
                    using (ModelSafety.EnterModelCtor(typeof(TModel))) {
                        itemModel = this.factory.Invoke(lc.Lifetime, itemDef, itemData);
                    }

                    newValues.Add((lc, itemModel));
                }

                foreach (var (lc, valueModel) in newValues) {
                    valueModel.Initialize();

                    this.Models.Add(valueModel.Data.MyKey, (lc, valueModel));
                    this.ValuesModels.Add(valueModel);

                    this.CreateCallback?.Invoke(valueModel);
                }

                if (newValues.Count > 0) {
                    this.Version.Invalidate();
                }

                newValues.Clear();
            }
        }

        [PublicAPI]
        public bool TryFind([RequireStaticDelegate] Func<TModel, bool> predicate, out TModel model) {
            this.EnsureAccessAllowed();

            foreach (var it in this.Values) {
                if (predicate.Invoke(it)) {
                    model = it;
                    return true;
                }
            }

            model = default;
            return false;
        }

        [PublicAPI]
        public bool TryFind<TState>(TState state, [RequireStaticDelegate] Func<TState, TModel, bool> predicate, out TModel model) {
            this.EnsureAccessAllowed();

            foreach (var it in this.Values) {
                if (predicate.Invoke(state, it)) {
                    model = it;
                    return true;
                }
            }

            model = default;
            return false;
        }

        /// <summary>
        /// Вызывает метод callback на всех уже существующих моделях
        /// с заданным ключем key, а также на всех моделях с таким ключем,
        /// которые будут добавлены пока активна lifetime.
        ///
        /// В случае если передан пустой ключ метод не делает ничего.
        /// </summary>
        [PublicAPI]
        public void ProcessEachWithKey(Lifetime lt, [CanBeNull] string key, Action<TModel> callback) {
            this.EnsureAccessAllowed();

            if (string.IsNullOrEmpty(key)) {
                return;
            }

            this.ProcessEach(lt, model => {
                if (this.KeySelector.Invoke(model.Data) == key) {
                    callback.Invoke(model);
                }
            });
        }

        /// <summary>
        /// Вызывает метод callback на всех уже существующих моделях,
        /// а также на всех моделях которые будут добавлены пока активна lifetime.
        /// </summary>
        [PublicAPI]
        public void ProcessEach(Lifetime lt, Action<TModel> callback) {
            this.EnsureAccessAllowed();

            foreach (var valuesModel in this.ValuesModels) {
                callback.Invoke(valuesModel);
            }

            lt.RegisterToEvent(
                e => this.CreateCallback += e,
                e => this.CreateCallback -= e,
                callback);
        }
    }
}