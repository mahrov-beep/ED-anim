namespace Multicast.ServerData {
    using System;
    using JetBrains.Annotations;
    using Multicast.Collections;

    public static class SdExtensions {
        public delegate void ConfigureDataDelegate<TDef, TData>(TDef def, TData data, bool created)
            where TDef : Def
            where TData : SdObjectBase;

        public static void ConfigureDataFrom<TDef, TData>([NotNull] this SdDict<TData> gameData, [NotNull] LookupCollection<TDef> defs, [CanBeNull] ConfigureDataDelegate<TDef, TData> configure = null)
            where TDef : Def
            where TData : SdObjectBase {
            if (gameData == null) {
                throw new ArgumentNullException(nameof(gameData));
            }

            if (defs == null) {
                throw new ArgumentNullException(nameof(defs));
            }

            foreach (var childDef in defs.Items) {
                var childData = gameData.GetOrCreate(childDef.key, out var created);
                configure?.Invoke(childDef, childData, created);
            }
        }
    }
}
