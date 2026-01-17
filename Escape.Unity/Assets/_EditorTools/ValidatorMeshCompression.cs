#if !UNITY_CLOUD_BUILD && UNITY_EDITOR
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor.Validation;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
[assembly: RegisterValidationRule(typeof(ValidatorModelCompression))]

public class ValidatorModelCompression : RootObjectValidator<Object> {
    [SerializeField] private ModelImporterMeshCompression Expected = ModelImporterMeshCompression.Medium;
    // [SerializeField] private List<string> Ignore = new();
    [SerializeField] private bool AllowLowerCompression = false;

    private static readonly HashSet<string> Ext = new() { ".fbx", ".obj", ".dae", ".blend", ".3ds", ".dxf" };
    private static readonly Dictionary<ModelImporterMeshCompression, int> Wgt = new(){
        {ModelImporterMeshCompression.Off,0},
        {ModelImporterMeshCompression.Low,1},
        {ModelImporterMeshCompression.Medium,2},
        {ModelImporterMeshCompression.High,3}
    };
    private static int W(ModelImporterMeshCompression c) => Wgt.TryGetValue(c, out var w) ? w : int.MaxValue;

    protected override void Validate(ValidationResult r) {
        if (BuildPipeline.isBuildingPlayer)
            return;

        if (!AssetDatabase.IsMainAsset(Object)) return;
        var path = AssetDatabase.GetAssetPath(Object);
        if (path == null || !Ext.Contains(Path.GetExtension(path).ToLower())) return;
        // if (Ignore.Contains(Path.GetFileNameWithoutExtension(path))) return;
        var imp = AssetImporter.GetAtPath(path) as ModelImporter;
        if (imp == null) return;
        var cur = imp.meshCompression;
        var bad = AllowLowerCompression ? cur != Expected : W(cur) < W(Expected);
        if (!bad) return;
        r.AddError($"{Object.name}: Compression {cur}→{Expected}")
         .WithFix("Apply", () => Fix(imp));
    }
    private void Fix(ModelImporter imp) {
        if (AllowLowerCompression || W(imp.meshCompression) < W(Expected))
            imp.meshCompression = Expected;
        EditorUtility.SetDirty(imp);
        BulkFlushManager.Register(imp.assetPath);
    }
}
#endif
