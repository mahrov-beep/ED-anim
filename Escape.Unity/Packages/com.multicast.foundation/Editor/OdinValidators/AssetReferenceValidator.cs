using Sirenix.OdinInspector.Editor.Validation;

[assembly: RegisterValidator(typeof(Multicast.OdinValidators.AssetReferenceValidator<>))]

namespace Multicast.OdinValidators {
    using UnityEditor;
    using UnityEditor.AddressableAssets;
    using UnityEngine.AddressableAssets;

    public class AssetReferenceValidator<TAssetReference> : ValueValidator<TAssetReference>
        where TAssetReference : AssetReference {
        protected override void Validate(ValidationResult result) {
            var value = this.ValueEntry.SmartValue;

            if (!value.RuntimeKeyIsValid()) {
                result.ResultType = ValidationResultType.Error;
                result.Message    = "Invalid asset reference";
                return;
            }

            if (string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(value.AssetGUID))) {
                result.ResultType = ValidationResultType.Error;
                result.Message    = "Missing asset reference (File not found. Please check that selected asset exists)";
                return;
            }

            var aaSettings = AddressableAssetSettingsDefaultObject.Settings;
            if (aaSettings == null || aaSettings.FindAssetEntry(value.AssetGUID) == null) {
                result.ResultType = ValidationResultType.Error;
                result.Message    = "Missing asset reference (Asset not an addressable. Please check that asset marked as 'Addressable')";
                return;
            }
        }
    }
}