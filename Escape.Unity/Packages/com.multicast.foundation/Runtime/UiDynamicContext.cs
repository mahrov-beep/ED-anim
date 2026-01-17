namespace Multicast {
    using System;
    using System.Collections.Generic;
    using JetBrains.Annotations;
    using UniMob;
    using UniMob.UI;

    public class UiDynamicContext {
        private readonly Dictionary<Type, List<object>> dataTypeToValues = new();
        private readonly HashSet<object>                collections      = new();

        [PublicAPI]
        public Atom<List<Widget>> Get<TData>(Func<TData, Widget> convert)
            where TData : class, IUiDynamicData {
            var dataType = typeof(TData);

            var collection = new UiDynamicCollection<TData>(convert);

            this.collections.Add(collection);

            if (this.dataTypeToValues.TryGetValue(dataType, out var values)) {
                foreach (TData value in values) {
                    collection.Add(value);
                }
            }

            return collection.Values;
        }

        [PublicAPI]
        public void Add<TData>(TData value)
            where TData : class, IUiDynamicData {
            var dataType = typeof(TData);

            if (!this.dataTypeToValues.TryGetValue(dataType, out var values)) {
                this.dataTypeToValues[dataType] = values = new List<object>();
            }

            values.Add(value);

            foreach (var collection in this.collections) {
                if (collection is UiDynamicCollection<TData> typedCollection) {
                    typedCollection.Add(value);
                }
            }
        }

        [PublicAPI]
        public void Remove<TData>(TData value)
            where TData : class, IUiDynamicData {
            var dataType = typeof(TData);

            if (!this.dataTypeToValues.TryGetValue(dataType, out var values)) {
                this.dataTypeToValues[dataType] = values = new List<object>();
            }

            values.Remove(value);

            foreach (var collection in this.collections) {
                if (collection is UiDynamicCollection<TData> typedCollection) {
                    typedCollection.Remove(value);
                }
            }
        }

        private class UiDynamicCollection<TData>
            where TData : class, IUiDynamicData {
            private readonly Func<TData, Widget>       convert;
            private readonly MutableAtom<List<Widget>> values  = Atom.Value(new List<Widget>());
            private readonly Dictionary<TData, Widget> mapping = new();

            public Atom<List<Widget>> Values => this.values;

            public UiDynamicCollection(Func<TData, Widget> convert) {
                this.convert = convert;
            }

            internal void Add(TData data) {
                if (this.mapping.ContainsKey(data)) {
                    throw new InvalidOperationException("Item already added");
                }

                var convertedData = this.convert.Invoke(data);

                if (convertedData.Key == null && 
                    convertedData is StatefulWidget statefulWidget) {
                    statefulWidget.Key = Key.Of(data);
                }

                this.mapping[data] = convertedData;

                this.values.Value.Add(convertedData);
                this.values.Invalidate();
            }

            internal void Remove(TData data) {
                if (!this.mapping.TryGetValue(data, out var convertedData)) {
                    return;
                }

                this.mapping.Remove(data);

                this.values.Value.Remove(convertedData);
                this.values.Invalidate();
            }
        }
    }

    public interface IUiDynamicData {
    }
}