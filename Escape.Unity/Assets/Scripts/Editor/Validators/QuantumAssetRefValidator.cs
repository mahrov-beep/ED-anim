using Quantum;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor.Validation;

[assembly: RegisterValidator(typeof(QuantumAssetRefValidator<>), Priority = -9999)]

public class QuantumAssetRefValidator<T> : ValueValidator<AssetRef<T>> where T : AssetObject {
    protected override void Validate(ValidationResult result) {
        if (this.Property.GetAttribute<RequiredAttribute>() == null &&
            this.Property.GetAttribute<RequiredRefAttribute>() == null) {
            return;
        }

        var assetRef = this.ValueEntry.SmartValue;

        if (!assetRef.IsValid) {
            result.AddError($"[Quantum] {this.Property.NiceName} is missing");
            return;
        }

        var asset = QuantumUnityDB.GetGlobalAssetEditorInstance(assetRef);

        if (asset == null) {
            result.AddError($"[Quantum] {this.Property.NiceName} is missing");
            return;
        }
    }
}