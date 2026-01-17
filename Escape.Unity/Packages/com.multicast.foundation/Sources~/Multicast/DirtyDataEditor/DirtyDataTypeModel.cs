namespace Multicast.DirtyDataEditor {
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Multicast.Pool;

    public static class DirtyDataTypeModel {
        private static readonly Dictionary<Type, DirtyDataTypeDefinition> CachedTypes = new Dictionary<Type, DirtyDataTypeDefinition>();
        private static readonly List<PolymorphicTypeInfo> PolymorphicTypes = new List<PolymorphicTypeInfo>();

        public struct PolymorphicTypeInfo {
            public Type   BaseType;
            public Type   SelfType;
            public string FieldValue;
        }

        static DirtyDataTypeModel() {
            foreach (var assembly in DirtyDataUtils.DependantAssemblies) {
                var types = assembly.GetTypes();
                foreach (var type in types) {
                    if (type.GetCustomAttribute<DDEImplAttribute>(false) is { } implAttr) {
                        RegisterPolymorphicType(type, implAttr.BaseType, implAttr.TypeFieldValue);
                    }

                    if (type.GetCustomAttribute<DDEObjectAttribute>(false) is { }) {
                        RegisterType(type);
                    }
                }
            }
        }

        public static void Initialize() {
        }

        internal static DirtyDataTypeDefinition GetTypeDefinitionCached(Type type) {
            if (!CachedTypes.TryGetValue(type, out var typeDefinition)) {
                throw new DirtyDataParseException($"Type '{type}' not marked as [DDEObject]");
            }

            return typeDefinition;
        }

        internal static Type GetPolymorphicTypeCached(Type baseType, string typeFieldValue) {
            foreach (var typeInfo in PolymorphicTypes) {
                if (typeInfo.BaseType == baseType && typeInfo.FieldValue == typeFieldValue) {
                    return typeInfo.SelfType;
                }
            }

            throw new DirtyDataParseException($"Polymorphic type not found: baseType = {baseType}, name={typeFieldValue}");
        }

        private static void RegisterPolymorphicType(Type type, Type baseType, string typeFieldValue) {
            PolymorphicTypes.Add(new PolymorphicTypeInfo {
                SelfType   = type,
                BaseType   = baseType,
                FieldValue = typeFieldValue,
            });
        }

        private static void RegisterType(Type type) {
            using (ListPool<DirtyDataEditorPropertyDefinition>.Get(out var properties)) {
                var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                foreach (var fi in fields) {
                    if (fi.GetCustomAttribute<DDEAttribute>() is { } ddeAttribute) {
                        var key          = ddeAttribute.Key;
                        var optional     = ddeAttribute.HasDefaultValue;
                        var defaultValue = ddeAttribute.DefaultValue;

                        var def = new DirtyDataEditorPropertyDefinition(key, fi, optional, defaultValue);

                        foreach (var otherDef in properties) {
                            if (otherDef.NameHash == def.NameHash) {
                                throw new DirtyDataParseException(
                                    $"Name collision found between '{def.Name}' and '{otherDef.Name}' in type '{type}'. " +
                                    $"Please rename one of this properties");
                            }
                        }

                        properties.Add(def);
                    }
                }

                var propertyValues = new DirtyDataEditorPropertyDefinition[properties.Count];
                properties.CopyTo(propertyValues);
                var typeDefinition = new DirtyDataTypeDefinition(type, propertyValues);

                CachedTypes.Add(type, typeDefinition);
            }
        }
    }

    internal class DirtyDataTypeDefinition {
        private readonly Type type;


        public DirtyDataTypeDefinition(Type type, DirtyDataEditorPropertyDefinition[] properties) {
            this.type       = type;
            this.Properties = properties;

            var ddeBaseAttr = type.GetCustomAttribute<DDEBaseAttribute>();
            if (ddeBaseAttr != null) {
                this.IsPolymorphic        = true;
                this.PolymorphicField     = ddeBaseAttr.TypeField;
                this.PolymorphicFieldHash = DirtyDataUtils.GetHash(ddeBaseAttr.TypeField);
            }
        }

        public DirtyDataEditorPropertyDefinition[] Properties { get; }

        public bool   IsPolymorphic        { get; }
        public string PolymorphicField     { get; }
        public long   PolymorphicFieldHash { get; }

        public string Name => this.type.Name;

        public bool TryGetProperty(long hash, out DirtyDataEditorPropertyDefinition property) {
            foreach (var it in this.Properties) {
                if (it.NameHash == hash) {
                    property = it;
                    return true;
                }
            }

            property = default;
            return false;
        }

        public object CreatePolymorphicInstance(string typeFieldValue, out Type instanceType) {
            if (!this.IsPolymorphic) {
                throw new InvalidOperationException("Must not call CreateInstance for non-polymorphic type");
            }

            instanceType = DirtyDataTypeModel.GetPolymorphicTypeCached(this.type, typeFieldValue);

            return DirtyDataTypeActivator.CreateInstance(instanceType);
        }

        public object CreateInstance() {
            if (this.IsPolymorphic) {
                throw new InvalidOperationException("Must not call CreateInstance for polymorphic type");
            }

            return DirtyDataTypeActivator.CreateInstance(this.type);
        }
    }

    internal class DirtyDataEditorPropertyDefinition {
        private readonly FieldInfo fieldInfo;

        public string Name         { get; }
        public long   NameHash     { get; }
        public bool   Optional     { get; }
        public object DefaultValue { get; }

        public Type Type => this.fieldInfo.FieldType;

        public DirtyDataEditorPropertyDefinition(string name, FieldInfo fieldInfo, bool optional, object defaultValue) {
            this.fieldInfo    = fieldInfo;
            this.Name         = name;
            this.NameHash     = DirtyDataUtils.GetHash(name);
            this.Optional     = optional;
            this.DefaultValue = defaultValue;
        }

        public void SetValue(object obj, object value) {
            this.fieldInfo.SetValue(obj, value);
        }
    }
}