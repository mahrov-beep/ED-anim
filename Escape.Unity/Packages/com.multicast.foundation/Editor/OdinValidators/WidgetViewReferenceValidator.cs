using Multicast.OdinValidators;
using Sirenix.OdinInspector.Editor.Validation;

[assembly: RegisterValidator(typeof(WidgetViewReferenceValidator))]

namespace Multicast.OdinValidators {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Multicast;
    using Sirenix.OdinInspector.Editor.Validation;
    using UniMob.UI;
    using UnityEditor;
    using Utilities;

    public class WidgetViewReferenceValidator : GlobalValidator {
        public override IEnumerable RunValidation(ValidationResult result) {
            foreach (var fi in EnumerateWidgetViewReferenceFields()) {
                var error = ValidateField(fi);
                if (error == null) {
                    continue;
                }

                var typeName = fi.DeclaringType?.FullName?.Replace('+', '.');
                result.AddError($"WidgetViewReference broken: {error} at {typeName}::{fi.Name}");
            }

            return null;
        }

        private static string ValidateField(FieldInfo fi) {
            if (fi.FieldType != typeof(WidgetViewReference)) {
                return $"Field type must be WidgetViewReference, actual: {fi.FieldType.Name}";
            }

            var viewRef = (WidgetViewReference)fi.GetValue(null);

            if (viewRef.Type != WidgetViewReferenceType.Addressable) {
                return $"Field value type must be Addressable, actual: {viewRef.Type}";
            }

            if (string.IsNullOrEmpty(viewRef.Path)) {
                return "Field value path must be non empty";
            }

            var asset = EditorAddressablesUtils.LoadAddressable(viewRef.Path);
            if (asset == null) {
                return $"Addressable asset at path '{viewRef.Path}' not exist";
            }

            return null;
        }

        private static IEnumerable<FieldInfo> EnumerateWidgetViewReferenceFields() {
            return TypeCache.GetTypesWithAttribute<WidgetViewReferenceContainerAttribute>()
                .SelectMany(type => EnumerateNestedTypesRecursive(type))
                .SelectMany(type => type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                .ToArray();
        }

        private static IEnumerable<Type> EnumerateNestedTypesRecursive(Type type) {
            yield return type;

            foreach (var nestedType in type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)) {
                foreach (var result in EnumerateNestedTypesRecursive(nestedType)) {
                    yield return result;
                }
            }
        }
    }
}