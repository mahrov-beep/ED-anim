using JetBrains.Annotations;
using System;

namespace Multicast.ServerData {
    public static class SdFactory {
        public static TServerData Create<TServerData>(Func<SdArgs, TServerData> func, [CanBeNull] ISdObjectTracker tracker = null)
            where TServerData : SdObjectBase {
            if (func is null) {
                throw new ArgumentNullException(nameof(func));
            }

            return func.Invoke(new SdArgs(null, "ROOT", tracker));
        }
    }
}
