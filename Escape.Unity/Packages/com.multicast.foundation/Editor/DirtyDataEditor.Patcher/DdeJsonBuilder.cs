namespace Multicast.DirtyDataEditor.Patcher {
    using System;
    using System.Dynamic;
    using JetBrains.Annotations;

    public static class DdeJsonBuilder {
        [PublicAPI]
        public static ExpandoObject New(Action<dynamic> f) {
            var obj = new ExpandoObject();
            f.Invoke(obj);
            return obj;
        }
    }
}