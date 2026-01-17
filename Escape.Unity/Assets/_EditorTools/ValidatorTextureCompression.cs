#if !UNITY_CLOUD_BUILD && UNITY_EDITOR
 using Sirenix.OdinInspector;
 using Sirenix.OdinInspector.Editor.Validation;
 using System;
 using System.Collections.Generic;
 using UnityEditor;
 using UnityEngine;
 using UnityEngine.Assertions;
 [assembly: RegisterValidationRule(typeof(ValidatorTextureCompression))]
 public enum AstcQuality { Fast, Normal, Best }
 [Serializable]
 public struct ExpectedSettings {
    public TextureImporterFormat Format;
    [Range(32, 8192)] public int MaxSize;
    public AstcQuality Quality;
 }
 static class BulkFlushManager {
    private static readonly HashSet<string> Dirty = new HashSet<string>();
    private const double Delay = 0.5;
    private static double _last;
    private static bool _hooked;
    public static void Register(string path) {
        Dirty.Add(path);
        _last = EditorApplication.timeSinceStartup;
        if (_hooked) return;
        _hooked = true;
        EditorApplication.update += Tick;
    }
    private static void Tick() {
        if (EditorApplication.timeSinceStartup - _last < Delay) return;
        EditorApplication.update -= Tick;
        _hooked = false;
        AssetDatabase.StartAssetEditing();
        foreach (var p in Dirty)
            AssetDatabase.ImportAsset(p, ImportAssetOptions.ForceUpdate);
        AssetDatabase.StopAssetEditing();
        AssetDatabase.SaveAssets();
        Dirty.Clear();
    }
 }
 public class ValidatorTextureCompression : RootObjectValidator<Texture2D> {
    [SerializeField] private ExpectedSettings Android = new ExpectedSettings { Format = TextureImporterFormat.ASTC_8x8, MaxSize = 512, Quality = AstcQuality.Normal };
    [SerializeField] private ExpectedSettings IOS = new ExpectedSettings { Format = TextureImporterFormat.ASTC_6x6, MaxSize = 512, Quality = AstcQuality.Normal };
    [SerializeField] private List<TextureFormat> Ignore = new List<TextureFormat> { TextureFormat.R16 };
    [SerializeField] private bool AllowUpscale = false;
    private static readonly Dictionary<TextureImporterFormat,int> AstcWeight=new(){
        {TextureImporterFormat.ASTC_4x4,5},
        {TextureImporterFormat.ASTC_5x5,4},
        {TextureImporterFormat.ASTC_6x6,3},
        {TextureImporterFormat.ASTC_8x8,2},
        {TextureImporterFormat.ASTC_10x10,1},
        {TextureImporterFormat.ASTC_12x12,0}
    };
    private static int Weight(TextureImporterFormat f)=>AstcWeight.TryGetValue(f,out var w)?w:int.MaxValue;
    private static readonly Dictionary<int,int> QualityWeight=new(){{0,0},{50,1},{100,2}};
    private static int Weight(int q)=>QualityWeight.TryGetValue(q,out var w)?w:int.MaxValue;
    protected override void Validate(ValidationResult r){
        if (BuildPipeline.isBuildingPlayer)
            return;

        if (!AssetDatabase.IsMainAsset(Object))return;
        if(Ignore.Contains(Object.format))return;
        var path=AssetDatabase.GetAssetPath(Object);
        var imp=AssetImporter.GetAtPath(path) as TextureImporter;
        Assert.IsNotNull(imp);
        var issues=new List<string>();
        GatherIssues(imp,"Android",Android,AllowUpscale,issues);
        GatherIssues(imp,"iOS",IOS,AllowUpscale,issues);
        if(issues.Count>0){
            r.AddError($"{Object.name}: {string.Join("; ",issues)}")
             .WithFix("Apply settings",()=>FixAll(imp));
        }
    }
    private static void GatherIssues(TextureImporter imp,string platform,ExpectedSettings exp,bool upscale,List<string> list){
        var ps=imp.GetPlatformTextureSettings(platform);
        if(!ps.overridden)list.Add($"{platform} Override disabled");
        if(upscale){
            if(ps.format!=exp.Format)list.Add($"{platform} Format {ps.format}→{exp.Format}");
            if(ps.maxTextureSize!=exp.MaxSize)list.Add($"{platform} Size {ps.maxTextureSize}→{exp.MaxSize}");
            if(Weight(ps.compressionQuality)!=Weight(MapQuality(exp.Quality)))list.Add($"{platform} Quality {ps.compressionQuality}→{MapQuality(exp.Quality)}");
        }else{
            if(Weight(ps.format)>Weight(exp.Format))list.Add($"{platform} Format {ps.format}→{exp.Format}");
            if(ps.maxTextureSize>exp.MaxSize)list.Add($"{platform} Size {ps.maxTextureSize}→{exp.MaxSize}");
            if(Weight(ps.compressionQuality)>Weight(MapQuality(exp.Quality)))list.Add($"{platform} Quality {ps.compressionQuality}→{MapQuality(exp.Quality)}");
        }
    }
    private void FixAll(TextureImporter imp){
        Apply(imp,"Android",Android,AllowUpscale);
        Apply(imp,"iOS",IOS,AllowUpscale);
        BulkFlushManager.Register(imp.assetPath);
    }
    private static void Apply(TextureImporter imp,string platform,ExpectedSettings exp,bool upscale){
        var ps=imp.GetPlatformTextureSettings(platform);
        ps.overridden=true;
        if(upscale){
            ps.format=exp.Format;
            ps.maxTextureSize=exp.MaxSize;
            ps.compressionQuality=MapQuality(exp.Quality);
        }else{
            if(Weight(ps.format)>Weight(exp.Format))ps.format=exp.Format;
            if(ps.maxTextureSize>exp.MaxSize)ps.maxTextureSize=exp.MaxSize;
            if(Weight(ps.compressionQuality)>Weight(MapQuality(exp.Quality)))ps.compressionQuality=MapQuality(exp.Quality);
        }
        ps.textureCompression=TextureImporterCompression.Compressed;
        imp.SetPlatformTextureSettings(ps);
        EditorUtility.SetDirty(imp);
    }
    private static int MapQuality(AstcQuality q)=>q switch{
        AstcQuality.Fast=>0,
        AstcQuality.Best=>100,
        _=>50
    };
 }
#endif