namespace Multicast.GameProperties {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;
    using UniMob;

    public class GamePropertiesModel : Model {
        private readonly UdGamePropertiesData                   data;
        private readonly MutableAtom<List<IAutoSyncedProperty>> autoSyncedProperties;

        internal List<IAutoSyncedProperty> AutoSyncedProperties => this.autoSyncedProperties.Value;

        [PublicAPI]
        public UdGamePropertiesData Data => this.data;

        public GamePropertiesModel(Lifetime lifetime, UdGamePropertiesData data,
            AppSharedFormulaContext formulaContext,
            AppSharedNumberFormulaContext numberFormulaContext)
            : base(lifetime) {
            this.data                 = data;
            this.autoSyncedProperties = Atom.Value(lifetime, new List<IAutoSyncedProperty>());

            formulaContext.RegisterGlobalVariableResolver(variableName => {
                if (data.Booleans.ContainsKey(variableName)) {
                    return () => this.Get((BoolGamePropertyName) variableName) ? 1 : 0;
                }

                if (data.Ints.ContainsKey(variableName)) {
                    return () => this.Get((IntGamePropertyName) variableName);
                }

                return null;
            });

            numberFormulaContext.RegisterGlobalVariableResolver(variableName => {
                if (data.Floats.ContainsKey(variableName)) {
                    return () => this.Get((FloatGamePropertyName) variableName);
                }

                return null;
            });
        }

        [Atom] internal bool IsSynced => this.autoSyncedProperties.Value.All(it => it.IsSynced);

        [PublicAPI] public bool Get(BoolGamePropertyName name)
            => this.Data.TryGetBool(name, out var value) ? value : name.DefaultValue;

        [PublicAPI] public int Get(IntGamePropertyName name)
            => this.Data.TryGetInt(name, out var value) ? value : name.DefaultValue;

        [PublicAPI] public float Get(FloatGamePropertyName name)
            => this.Data.TryGetFloat(name, out var value) ? value : name.DefaultValue;

        [PublicAPI]
        public void RegisterAutoSyncedProperty(Lifetime lifetime, BoolGamePropertyName name, Func<bool> func) {
            this.RegisterAutoSyncedPropertyInternal(lifetime, new AutoSyncedProperty<bool, BoolGamePropertyName>(
                name, func, this.data.TryGetBool, this.data.SetBool));
        }

        [PublicAPI]
        public void RegisterAutoSyncedProperty(Lifetime lifetime, IntGamePropertyName name, Func<int> func) {
            this.RegisterAutoSyncedPropertyInternal(lifetime, new AutoSyncedProperty<int, IntGamePropertyName>(
                name, func, this.data.TryGetInt, this.data.SetInt));
        }

        [PublicAPI]
        public void RegisterAutoSyncedProperty(Lifetime lifetime, FloatGamePropertyName name, Func<float> func) {
            this.RegisterAutoSyncedPropertyInternal(lifetime, new AutoSyncedProperty<float, FloatGamePropertyName>(
                name, func, this.data.TryGetFloat, this.data.SetFloat));
        }

        private void RegisterAutoSyncedPropertyInternal(Lifetime lifetime, IAutoSyncedProperty property) {
            lifetime.Bracket(
                () => {
                    this.autoSyncedProperties.Value.Add(property);
                    this.autoSyncedProperties.Invalidate();
                },
                () => {
                    this.autoSyncedProperties.Value.Remove(property);
                    this.autoSyncedProperties.Invalidate();
                }
            );
        }

        internal interface IAutoSyncedProperty {
            bool IsSynced { get; }

            void SyncIfRequired();
        }

        private class AutoSyncedProperty<T, TName> : IAutoSyncedProperty where TName : struct {
            private readonly TName                name;
            private readonly Func<T>              actualGetter;
            private readonly DataGetter<T, TName> dataGetter;
            private readonly DataSetter<T, TName> dataSetter;

            public AutoSyncedProperty(TName name, Func<T> actualGetter,
                DataGetter<T, TName> dataGetter, DataSetter<T, TName> dataSetter) {
                this.name         = name;
                this.actualGetter = actualGetter;
                this.dataGetter   = dataGetter;
                this.dataSetter   = dataSetter;
            }

            public T ActualValue => this.actualGetter.Invoke();

            public bool IsSynced {
                get {
                    if (!this.dataGetter.Invoke(this.name, out var dataValue)) {
                        return false;
                    }

                    return EqualityComparer<T>.Default.Equals(this.ActualValue, dataValue);
                }
            }

            public void SyncIfRequired() {
                if (this.IsSynced) {
                    return;
                }

                var value = this.ActualValue;

                this.dataSetter.Invoke(this.name, value);
            }
        }

        private delegate bool DataGetter<T, TName>(TName name, out T value) where TName : struct;

        private delegate void DataSetter<T, TName>(TName name, T value) where TName : struct;
    }
}