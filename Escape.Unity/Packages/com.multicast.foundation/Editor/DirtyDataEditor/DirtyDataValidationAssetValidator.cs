using Multicast.DirtyDataEditor;
using Sirenix.OdinInspector.Editor.Validation;

[assembly: RegisterValidator(typeof(DirtyDataValidationAssetValidator))]

namespace Multicast.DirtyDataEditor {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Collections;
    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor.Validation;
    using UnityEditor.AddressableAssets;
    using UnityEngine;
    using Utilities;

    public class DirtyDataValidationAssetValidator : RootObjectValidator<DirtyDataValidationAsset> {
        private ValidationResult           currentResult;
        private Dictionary<string, object> lookups;

        public override RevalidationCriteria RevalidationCriteria { get; }
            = RevalidationCriteria.OnValueChange;

        protected override void Validate(ValidationResult result) {
            this.currentResult = result;

            var cache = EditorAddressablesCache<TextAsset>.Instance.Select(it => new Multicast.TextAsset(it.text));

            DirtyDataParser.Errors.Clear();

            object asset;
            try {
                asset = this.Object.Parse(cache);
            }
            finally {
                foreach (var error in DirtyDataParser.Errors) {
                    this.currentResult.AddError(error);
                }
            }

            if (asset == null) {
                this.currentResult.AddError("Failed to parse DDE");
                return;
            }

            this.Validate(asset);

            var selfValidationResult = new SelfValidationResult();

            this.Object.Validate(asset, selfValidationResult);

            for (int i = 0; i < selfValidationResult.Count; i++) {
                var selfResult = selfValidationResult[i];

                switch (selfResult.ResultType) {
                    case SelfValidationResult.ResultType.Error:
                        result.AddError(selfResult.Message);
                        break;

                    case SelfValidationResult.ResultType.Warning:
                        result.AddWarning(selfResult.Message);
                        break;
                }
            }

            this.currentResult = null;
            this.lookups       = null;
        }

        public void Validate(object instance) {
            var fields = instance.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

            this.lookups = fields
                .Select(fi => new {fi = fi, val = fi.GetValue(instance)})
                .ToDictionary(it => it.fi.Name, it => it.val);

            foreach (var fi in fields) {
                var itemInstance = fi.GetValue(instance);

                if (fi.FieldType.IsGenericType && typeof(LookupCollection<>).IsAssignableFrom(fi.FieldType.GetGenericTypeDefinition())) {
                    foreach (var kvp in (IEnumerable) itemInstance) {
                        var key   = (string) kvp.GetType().GetProperty("Key", BindingFlags.Instance | BindingFlags.Public)?.GetValue(kvp);
                        var value = kvp.GetType().GetProperty("Value", BindingFlags.Instance | BindingFlags.Public)?.GetValue(kvp);

                        this.ValidateDefinition(value, $"DDE::{fi.Name}[{key}]");
                    }
                }
                else {
                    this.ValidateDefinition(itemInstance, $"DDE::{fi.Name}");
                }
            }
        }

        private void ValidateDefinition(object instance, string propertyPath) {
            var type = instance.GetType();

            if (type.GetCustomAttribute<DDEObjectAttribute>() == null) {
                return;
            }

            foreach (var fi in type.GetFields(BindingFlags.Public | BindingFlags.Instance)) {
                var itemInstance = fi.GetValue(instance);

                if (fi.GetCustomAttribute<DDEAddressableAttribute>() != null) {
                    this.ValidateAddressable(itemInstance, $"{propertyPath}::{fi.Name}");
                }

                if (fi.GetCustomAttribute<DDEExternalKeyAttribute>() is { } attr) {
                    this.ValidateExternalKey(itemInstance, $"{propertyPath}::{fi.Name}", attr);
                }

                if (Attribute.GetCustomAttributes(fi, typeof(DDENonNullWhenAttribute), false) is var notNullWhenList &&
                    notNullWhenList != null&& notNullWhenList.Length > 0 &&
                    fi.GetCustomAttribute<DDEAttribute>() is var ddeAttr) {
                    foreach (DDENonNullWhenAttribute notNullattr in notNullWhenList) {
                        this.ValidateNotNullWhen(itemInstance, $"{propertyPath}::{fi.Name}", ddeAttr, notNullattr, instance);
                    }
                }

                if (itemInstance != null && fi.FieldType.GetCustomAttribute<DDEObjectAttribute>() != null) {
                    this.ValidateDefinition(itemInstance, $"{propertyPath}::{fi.Name}<{itemInstance.GetType().Name}>");
                }

                switch (itemInstance) {
                    case IList list:
                        for (var index = 0; index < list.Count; index++) {
                            this.ValidateDefinition(list[index], $"{propertyPath}::{fi.Name}[{index}]");
                        }

                        break;

                    case IDictionary dict:
                        foreach (DictionaryEntry kvp in dict) {
                            this.ValidateDefinition(kvp.Value, $"{propertyPath}::{fi.Name}[{kvp.Key}]");
                        }

                        break;
                }
            }
        }

        private void ValidateAddressable(object instance, string propertyPath) {
            switch (instance) {
                case null:
                    return;

                case string path:
                    var exists = EditorAddressablesUtils.LoadAddressable(path) != null;
                    if (!exists) {
                        this.currentResult
                            .AddError($"Addressable not exists: {propertyPath}")
                            .WithMetaData("Value", path);
                    }

                    break;

                case IList list:
                    foreach (var it in list) {
                        this.ValidateAddressable(it, $"{propertyPath}[{it}]");
                    }

                    break;

                default:
                    this.currentResult
                        .AddError($"Addressable unsupported type: {propertyPath}")
                        .WithMetaData("Type", instance.GetType().Name);
                    break;
            }
        }

        private void ValidateNotNullWhen(object instance, string propertyPath, DDEAttribute ddeAttr, DDENonNullWhenAttribute attr, object parentInstance) {
            if (ddeAttr == null) {
                this.currentResult
                    .AddError($"[DDENonNullWhen] can be used only in pair with [DDE] attribute: {propertyPath}")
                    .WithMetaData("Field", attr.Field);

                return;
            }

            if (ddeAttr.HasDefaultValue == false) {
                this.currentResult
                    .AddError($"[DDENonNullWhen] can be used only when default value is set in [DDE] attribute: {propertyPath}")
                    .WithMetaData("Field", attr.Field);

                return;
            }

            var valueIsDefault = ddeAttr.DefaultValue?.Equals(instance) ?? (instance == null);
            if (!valueIsDefault) {
                return;
            }

            var condFi = parentInstance.GetType().GetField(attr.Field, BindingFlags.Instance | BindingFlags.Public);

            if (condFi == null) {
                this.currentResult
                    .AddError($"DDENonNullWhen field not exists: {propertyPath}")
                    .WithMetaData("Field", attr.Field);

                return;
            }

            var condValue = condFi.GetValue(parentInstance);

            var match = attr.Value?.Equals(condValue) ?? condValue == null;

            if (!match) {
                return;
            }

            this.currentResult
                .AddError($"DDENonNullWhen value must be non default: {propertyPath}")
                .WithMetaData("Value", condValue);
        }

        private void ValidateExternalKey(object instance, string propertyPath, DDEExternalKeyAttribute attr) {
            switch (instance) {
                case null:
                    return;

                case string key:
                    if (!this.lookups.TryGetValue(attr.Collection, out var lookup)) {
                        this.currentResult
                            .AddError($"ExternalKey collection not exists: {propertyPath}")
                            .WithMetaData("Collection", attr.Collection);

                        return;
                    }

                    var args   = new object[] {key, null};
                    var exists = (bool) lookup.GetType().GetMethod("TryGet", BindingFlags.Instance | BindingFlags.Public).Invoke(lookup, args);

                    if (!exists) {
                        this.currentResult
                            .AddError($"ExternalKey invalid: {propertyPath}")
                            .WithMetaData("Collection", attr.Collection)
                            .WithMetaData("Value", key);
                    }

                    break;

                case IDictionary dict:
                    foreach (DictionaryEntry kvp in dict) {
                        if (kvp.Key is string itemKey) {
                            this.ValidateExternalKey(itemKey, $"{propertyPath}[{itemKey}]", attr);
                        }
                        else {
                            this.currentResult
                                .AddError($"ExternalKey unsupported type: {propertyPath}");
                        }
                    }

                    break;

                case IList list:
                    foreach (var it in list) {
                        if (it is string itemKey) {
                            this.ValidateExternalKey(itemKey, $"{propertyPath}[{itemKey}]", attr);
                        }
                        else {
                            this.currentResult
                                .AddError($"ExternalKey unsupported type: {propertyPath}");
                        }
                    }

                    break;

                default:
                    this.currentResult
                        .AddError($"ExternalKey unsupported type: {propertyPath}");
                    break;
            }
        }
    }
}