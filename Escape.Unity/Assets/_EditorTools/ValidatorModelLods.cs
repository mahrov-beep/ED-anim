#if !UNITY_CLOUD_BUILD && UNITY_EDITOR
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor.Validation;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Autodesk.Fbx;

[assembly: RegisterValidationRule(typeof(ValidatorModelLods))]

public class ValidatorModelLods : RootObjectValidator<Object> {
    [SerializeField] private List<int> KeepLods = new List<int> { 0 };
    private static readonly HashSet<string> Ext = new HashSet<string> { ".fbx" };

    protected override void Validate(ValidationResult r) {
        if (BuildPipeline.isBuildingPlayer) return;
        if (!AssetDatabase.IsMainAsset(Object)) return;
        var path = AssetDatabase.GetAssetPath(Object);
        if (string.IsNullOrEmpty(path) || !Ext.Contains(Path.GetExtension(path).ToLower())) return;

        var presentLods = GetLodIndices(path);
        if (presentLods.Count <= 1) return;

        var toKeep = ResolveKeepLods(presentLods, KeepLods);
        var (totalVerts, removedVerts) = CountVerts(path, toKeep);
        if (totalVerts == 0) return;

        var origSize = new FileInfo(Path.GetFullPath(path)).Length;
        var savedMb = (origSize * (float)removedVerts / totalVerts) / (1024f * 1024f);

        var msg = $"{Object.name}: Mesh has lods [{string.Join(", ", presentLods)}], keep [{string.Join(", ", toKeep)}]. Save ~{savedMb:F2} MB (~{removedVerts}/{totalVerts} verts)";
        r.AddWarning(msg).WithFix("Remove LODs", () => Fix(path, toKeep));
    }

    private List<int> GetLodIndices(string assetPath) {
        var lods = new HashSet<int>();
        using (var mgr = FbxManager.Create()) {
            var ios = FbxIOSettings.Create(mgr, Globals.IOSROOT);
            mgr.SetIOSettings(ios);
            var imp = FbxImporter.Create(mgr, "imp");
            imp.Initialize(assetPath, -1, mgr.GetIOSettings());
            var scene = FbxScene.Create(mgr, "scene");
            imp.Import(scene);
            CollectLods(scene.GetRootNode(), lods);
            imp.Destroy();
        }
        return lods.OrderBy(x => x).ToList();
    }

    private void CollectLods(FbxNode node, HashSet<int> lods) {
        if (node == null) return;
        for (int i = 0; i < node.GetChildCount(); i++) {
            var child = node.GetChild(i);
            var attr = child.GetNodeAttribute();
            if (attr != null && attr.GetAttributeType() == FbxNodeAttribute.EType.eLODGroup) {
                for (int j = 0; j < child.GetChildCount(); j++) lods.Add(j);
            }
            CollectLods(child, lods);
        }
    }

    private List<int> ResolveKeepLods(List<int> present, List<int> desired) {
        return desired.Select(d => present.Contains(d) ? d : present.OrderBy(x => System.Math.Abs(x - d)).First())
                      .Distinct()
                      .OrderBy(x => x)
                      .ToList();
    }

    private (long totalVerts, long removedVerts) CountVerts(string assetPath, List<int> toKeep) {
        long total = 0, removed = 0;
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (prefab == null) return (0, 0);
        var groups = prefab.GetComponentsInChildren<LODGroup>(true);
        if (groups.Length == 0) {
            foreach (var mf in prefab.GetComponentsInChildren<MeshFilter>(true)) total += mf.sharedMesh ? mf.sharedMesh.vertexCount : 0;
            foreach (var sk in prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true)) total += sk.sharedMesh ? sk.sharedMesh.vertexCount : 0;
            return (total, 0);
        }
        foreach (var g in groups) {
            var lods = g.GetLODs();
            for (int i = 0; i < lods.Length; i++) {
                foreach (var rend in lods[i].renderers) {
                    var mesh = GetMesh(rend);
                    if (mesh == null) continue;
                    var v = mesh.vertexCount;
                    total += v;
                    if (!toKeep.Contains(i)) removed += v;
                }
            }
        }
        return (total, removed);
    }

    private Mesh GetMesh(Renderer r) {
        if (r is SkinnedMeshRenderer sk) return sk.sharedMesh;
        if (r is MeshRenderer) {
            var mf = r.GetComponent<MeshFilter>();
            return mf ? mf.sharedMesh : null;
        }
        return null;
    }

    private void Fix(string assetPath, List<int> toKeep) {
        var full = Path.GetFullPath(assetPath);
        using (var mgr = FbxManager.Create()) {
            var ios = FbxIOSettings.Create(mgr, Globals.IOSROOT);
            mgr.SetIOSettings(ios);
            var imp = FbxImporter.Create(mgr, "imp");
            imp.Initialize(assetPath, -1, mgr.GetIOSettings());
            var scene = FbxScene.Create(mgr, "scene");
            imp.Import(scene);
            RemoveLods(scene.GetRootNode(), toKeep);
            imp.Destroy();
            var exp = FbxExporter.Create(mgr, "exp");
            exp.Initialize(full, -1, mgr.GetIOSettings());
            exp.Export(scene);
            exp.Destroy();
        }
        AssetDatabase.ImportAsset(assetPath);
    }

    private void RemoveLods(FbxNode node, List<int> keep) {
        if (node == null) return;
        for (int i = 0; i < node.GetChildCount(); i++) {
            var child = node.GetChild(i);
            var attr = child.GetNodeAttribute();
            if (attr != null && attr.GetAttributeType() == FbxNodeAttribute.EType.eLODGroup) {
                for (int j = child.GetChildCount() - 1; j >= 0; j--) {
                    if (!keep.Contains(j)) child.RemoveChild(child.GetChild(j));
                }
                if (child.GetChildCount() > 0) {
                    var kept = child.GetChild(0);
                    var newName = Regex.Replace(kept.GetName(), "(?<=LOD)\\d+", "0", RegexOptions.IgnoreCase);
                    kept.SetName(newName);
                }
            }
            RemoveLods(child, keep);
        }
    }
}
#endif
