using Sirenix.OdinInspector.Editor.Validation;

[assembly: RegisterValidator(typeof(Multicast.OdinValidators.SpriteStyleAssetValidator))]

namespace Multicast.OdinValidators {
    using CodeWriter.StyleComponents.StyleAssets;
    using Sirenix.OdinInspector.Editor.Validation;
    using UnityEngine;

    public class SpriteStyleAssetValidator : RootObjectValidator<SpriteStyleAsset> {
        protected override void Validate(ValidationResult result) {
            var asset = this.ValueEntry.SmartValue;

            for (int i = 0; i < Mathf.Min(asset.StyleNames.Length, asset.StyleValues.Length); i++) {
                var name  = asset.StyleNames[i];
                var value = asset.StyleValues[i];

                if (value == null) {
                    result.AddError($"Style '{name}' is missing");
                }
            }
        }
    }
}