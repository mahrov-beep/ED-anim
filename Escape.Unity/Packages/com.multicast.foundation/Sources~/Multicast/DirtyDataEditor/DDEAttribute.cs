// ReSharper disable InconsistentNaming

namespace Multicast.DirtyDataEditor {
    using System;

    public static class DDE {
        public const string Empty = "__$$$DDE$$$__Empty";
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class DDEAttribute : Attribute {
        public string Key             { get; }
        public bool   HasDefaultValue { get; }
        public object DefaultValue    { get; }

        public DDEAttribute(string key) {
            this.Key             = key;
            this.DefaultValue    = null;
            this.HasDefaultValue = false;
        }

        public DDEAttribute(string key, object defaultValue) {
            this.Key             = key;
            this.DefaultValue    = defaultValue;
            this.HasDefaultValue = true;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class DDEImplAttribute : Attribute {
        public Type   BaseType       { get; }
        public string TypeFieldValue { get; }

        public DDEImplAttribute(Type baseType, string typeFieldValue) {
            this.BaseType       = baseType;
            this.TypeFieldValue = typeFieldValue;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class DDEBaseAttribute : Attribute {
        public string TypeField { get; }

        public DDEBaseAttribute(string typeField) {
            this.TypeField = typeField;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class DDEObjectAttribute : Attribute {
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class RegisterDirtyDataParserAttribute : Attribute {
        public Type ParserType { get; }

        public RegisterDirtyDataParserAttribute(Type parserType) {
            this.ParserType = parserType;
        }
    }
}