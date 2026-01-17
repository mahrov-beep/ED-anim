// ReSharper disable InconsistentNaming


namespace Multicast.DirtyDataEditor {
    using System;
    using System.Diagnostics;

    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field)]
    public class DDEAddressableAttribute : Attribute {
    }

    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field)]
    public class DDEExternalKeyAttribute : Attribute {
        public string Collection { get; }

        public DDEExternalKeyAttribute(string collection) {
            this.Collection = collection;
        }
    }

    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class DDENonNullWhenAttribute : Attribute {
        public string Field { get; }
        public object Value { get; }

        public DDENonNullWhenAttribute(string field, object value) {
            this.Field = field;
            this.Value = value;
        }
    }
}