namespace Multicast {
    using System;
    using System.Collections.Generic;

    public interface IDataObject {
        string MyKey { get; }
    }

    public interface IDataDict<T> : IDataObject, IEnumerable<T> {
        event Action SelfChanged;

        bool TryGetValue(string key, out T value);
        T GetOrCreate(string key, out bool created);
    }
}