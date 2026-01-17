#if !UNITY_CLOUD_BUILD && UNITY_EDITOR
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor.Validation;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
[assembly: RegisterValidationRule(typeof(ValidatorTextureMipmap))]
[Serializable]
public struct ExpectedMipmapSettings {
    public bool MipmapsEnabled;
    public bool Streaming;
    public TextureImporterMipFilter Filter;
    [Range(0, 10)] public int FadeStart;
    [Range(0, 10)] public int FadeEnd;
    public bool FadeoutToGray;
}
public class ValidatorTextureMipmap : RootObjectValidator<Texture2D> {
    [SerializeField]
    private ExpectedMipmapSettings SettingsExpected = new ExpectedMipmapSettings {
        MipmapsEnabled = true,
        Streaming = true,
        Filter = TextureImporterMipFilter.KaiserFilter,
        FadeStart = 1,
        FadeEnd = 3,
        FadeoutToGray = false
    };
    [SerializeField] private List<TextureFormat> Ignore = new() { TextureFormat.R16 };
    protected override void Validate(ValidationResult r) {
        if (BuildPipeline.isBuildingPlayer)
            return;

        if (!AssetDatabase.IsMainAsset(Object)) return;
        if (Ignore.Contains(Object.format)) return;
        var imp = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(Object));
        var issues = new List<string>();
        if (imp.mipmapEnabled != SettingsExpected.MipmapsEnabled) issues.Add($"Mipmaps {(imp.mipmapEnabled ? "On" : "Off")}→{(SettingsExpected.MipmapsEnabled ? "On" : "Off")}");
        if (imp.streamingMipmaps != SettingsExpected.Streaming) issues.Add($"Streaming {(imp.streamingMipmaps ? "On" : "Off")}→{(SettingsExpected.Streaming ? "On" : "Off")}");
        if (imp.mipmapFilter != SettingsExpected.Filter) issues.Add($"Filter {imp.mipmapFilter}→{SettingsExpected.Filter}");
        if (imp.mipmapFadeDistanceStart != SettingsExpected.FadeStart || imp.mipmapFadeDistanceEnd != SettingsExpected.FadeEnd)
            issues.Add($"Fade {imp.mipmapFadeDistanceStart}-{imp.mipmapFadeDistanceEnd}→{SettingsExpected.FadeStart}-{SettingsExpected.FadeEnd}");
        if (imp.fadeout != SettingsExpected.FadeoutToGray) issues.Add($"Fadeout {(imp.fadeout ? "On" : "Off")}→{(SettingsExpected.FadeoutToGray ? "On" : "Off")}");
        if (issues.Count > 0)
            r.AddError($"{Object.name}: {string.Join("; ", issues)}")
             .WithFix("Apply mipmap settings", () => Fix(imp));
    }
    private void Fix(TextureImporter imp) {
        imp.mipmapEnabled = SettingsExpected.MipmapsEnabled;
        imp.streamingMipmaps = SettingsExpected.Streaming;
        imp.mipmapFilter = SettingsExpected.Filter;
        imp.mipmapFadeDistanceStart = SettingsExpected.FadeStart;
        imp.mipmapFadeDistanceEnd = SettingsExpected.FadeEnd;
        imp.fadeout = SettingsExpected.FadeoutToGray;
        EditorUtility.SetDirty(imp);
        BulkFlushManager.Register(imp.assetPath);
    }
}
#endif
