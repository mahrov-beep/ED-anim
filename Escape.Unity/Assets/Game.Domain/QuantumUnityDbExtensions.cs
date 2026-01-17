using System;
using JetBrains.Annotations;
using Quantum;

public static class QuantumUnityDbExtensions {
    [PublicAPI]
    [MustUseReturnValue]
    public static bool HasRequiredAsset<T>(this IResourceManager db, string assetPath, out string errorMessage) where T : AssetObject {
        var assetGuid = db.GetAssetGuid(assetPath);

        if (!assetGuid.IsValid) {
            errorMessage = $"Required asset '{assetPath}' not exist";
            return false;
        }

        var actualAssetType = db.GetAssetType(assetGuid);

        if (!typeof(T).IsAssignableFrom(actualAssetType)) {
            errorMessage = $"Required asset '{assetPath}' type mismatch: actual={actualAssetType.Name}, expected={typeof(T).Name}";
            return false;
        }

        errorMessage = null;
        return true;
    }

    [PublicAPI]
    [MustUseReturnValue]
    public static T GetRequiredAsset<T>(this IResourceManager db, string assetPath) where T : AssetObject {
        var assetGuid = db.GetAssetGuid(assetPath);

        if (!assetGuid.IsValid) {
            throw new Exception($"Required asset '{assetPath}' not exist");
        }

        var asset = db.GetAsset(assetGuid);

        if (asset == null) {
            throw new Exception($"Required asset '{assetPath}' is null");
        }

        if (asset is not T typedAsset) {
            throw new Exception($"Required asset '{assetPath}' type mismatch: actual={asset?.GetType().Name}, expected={typeof(T).Name}");
        }

        return typedAsset;
    }
}