namespace Game.Services.Graphics {
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEngine;
    using UnityEngine.Rendering;

    public partial class GraphicsSettingsService {
        private static class UrpReflectionAdapter {
            private enum MemberKind {
                Property,
                Field,
                EnumProperty,
                EnumField,
            }

            private readonly struct MemberKey : IEquatable<MemberKey> {
                public MemberKey(Type ownerType, string name, MemberKind kind, Type valueType) {
                    this.OwnerType = ownerType;
                    this.Name      = name;
                    this.Kind      = kind;
                    this.ValueType = valueType;
                }

                public Type OwnerType { get; }
                public string Name { get; }
                public MemberKind Kind { get; }
                public Type ValueType { get; }

                public bool Equals(MemberKey other) {
                    return ReferenceEquals(this.OwnerType, other.OwnerType)
                           && string.Equals(this.Name, other.Name, StringComparison.Ordinal)
                           && this.Kind == other.Kind
                           && ReferenceEquals(this.ValueType, other.ValueType);
                }

                public override bool Equals(object obj) => obj is MemberKey other && this.Equals(other);

                public override int GetHashCode() {
                    unchecked {
                        var hashCode = this.OwnerType != null ? this.OwnerType.GetHashCode() : 0;
                        hashCode = (hashCode * 397) ^ (this.Name != null ? StringComparer.Ordinal.GetHashCode(this.Name) : 0);
                        hashCode = (hashCode * 397) ^ (int)this.Kind;
                        hashCode = (hashCode * 397) ^ (this.ValueType != null ? this.ValueType.GetHashCode() : 0);
                        return hashCode;
                    }
                }
            }

            private static readonly Dictionary<MemberKey, MemberInfo> MemberCache = new();
            private static readonly object CacheLock = new();

            public static object GetPipelineAsset() {
                return QualitySettings.renderPipeline ?? GraphicsSettings.currentRenderPipeline;
            }

            public static bool TrySetValue<T>(object urp, string propertyName, T value) {
                var prop = FindProperty(urp, propertyName, typeof(T));
                if (prop != null) {
                    prop.SetValue(urp, value);
                    return true;
                }

                var field = FindField(urp, propertyName, typeof(T));
                if (field != null) {
                    field.SetValue(urp, value);
                    return true;
                }

                return false;
            }

            public static bool TrySetEnum(object urp, string propertyName, int value) {
                var prop = FindEnumProperty(urp, propertyName);
                if (prop != null) {
                    prop.SetValue(urp, Enum.ToObject(prop.PropertyType, value));
                    return true;
                }

                var field = FindEnumField(urp, propertyName);
                if (field != null) {
                    field.SetValue(urp, Enum.ToObject(field.FieldType, value));
                    return true;
                }

                return false;
            }

            private static PropertyInfo FindProperty(object urp, string name, Type type) {
                if (urp == null) {
                    return null;
                }

                var ownerType = urp.GetType();
                var key       = new MemberKey(ownerType, name, MemberKind.Property, type);
                if (TryGetCached(key, out var cached)) {
                    return cached as PropertyInfo;
                }

                PropertyInfo found = null;
                foreach (var candidate in GetMemberNames(name)) {
                    var prop = ownerType.GetProperty(candidate, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (prop != null && prop.CanWrite && prop.PropertyType == type) {
                        found = prop;
                        break;
                    }
                }

                Cache(key, found);
                return found;
            }

            private static PropertyInfo FindEnumProperty(object urp, string name) {
                if (urp == null) {
                    return null;
                }

                var ownerType = urp.GetType();
                var key       = new MemberKey(ownerType, name, MemberKind.EnumProperty, null);
                if (TryGetCached(key, out var cached)) {
                    return cached as PropertyInfo;
                }

                PropertyInfo found = null;
                foreach (var candidate in GetMemberNames(name)) {
                    var prop = ownerType.GetProperty(candidate, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (prop != null && prop.CanWrite && prop.PropertyType.IsEnum) {
                        found = prop;
                        break;
                    }
                }

                Cache(key, found);
                return found;
            }

            private static FieldInfo FindField(object urp, string name, Type type) {
                if (urp == null) {
                    return null;
                }

                var ownerType = urp.GetType();
                var key       = new MemberKey(ownerType, name, MemberKind.Field, type);
                if (TryGetCached(key, out var cached)) {
                    return cached as FieldInfo;
                }

                FieldInfo found = null;
                foreach (var candidate in GetMemberNames(name)) {
                    var field = ownerType.GetField(candidate, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (field != null && field.FieldType == type) {
                        found = field;
                        break;
                    }
                }

                Cache(key, found);
                return found;
            }

            private static FieldInfo FindEnumField(object urp, string name) {
                if (urp == null) {
                    return null;
                }

                var ownerType = urp.GetType();
                var key       = new MemberKey(ownerType, name, MemberKind.EnumField, null);
                if (TryGetCached(key, out var cached)) {
                    return cached as FieldInfo;
                }

                FieldInfo found = null;
                foreach (var candidate in GetMemberNames(name)) {
                    var field = ownerType.GetField(candidate, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (field != null && field.FieldType.IsEnum) {
                        found = field;
                        break;
                    }
                }

                Cache(key, found);
                return found;
            }

            private static IEnumerable<string> GetMemberNames(string baseName) {
                yield return baseName;

                if (!string.IsNullOrEmpty(baseName)) {
                    var upper = char.ToUpperInvariant(baseName[0]) + baseName.Substring(1);
                    yield return upper;
                    yield return "m_" + upper;
                }
            }

            private static bool TryGetCached(MemberKey key, out MemberInfo member) {
                lock (CacheLock) {
                    if (MemberCache.TryGetValue(key, out var cached)) {
                        member = cached;
                        return true;
                    }
                }

                member = null;
                return false;
            }

            private static void Cache(MemberKey key, MemberInfo member) {
                lock (CacheLock) {
                    MemberCache[key] = member;
                }
            }
        }
    }
}
