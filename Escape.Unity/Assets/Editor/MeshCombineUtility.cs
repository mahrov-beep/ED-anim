#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class MeshCombineUtility {
  [MenuItem("Tools/Combine Selected Meshes", priority = 2000)]
  static void CombineSelectedMeshes() {
    var selection = Selection.gameObjects;
    if (selection == null || selection.Length == 0) {
      Debug.LogWarning("Select one or more GameObjects that contain meshes to combine.");
      return;
    }

    var anchor = selection[0];

    var meshFilters = new List<MeshFilter>();
    var skinnedRenderers = new List<SkinnedMeshRenderer>();

    foreach (var go in selection) {
      if (go == null) {
        continue;
      }

      meshFilters.AddRange(go.GetComponentsInChildren<MeshFilter>());
      skinnedRenderers.AddRange(go.GetComponentsInChildren<SkinnedMeshRenderer>());
    }

    if (meshFilters.Count == 0 && skinnedRenderers.Count == 0) {
      Debug.LogWarning("No MeshFilter or SkinnedMeshRenderer components found in the current selection.");
      return;
    }

    var combineList = new List<CombineInstance>();
    var tempMeshes = new List<Mesh>();

    Matrix4x4 anchorWorldToLocal = anchor.transform.worldToLocalMatrix;

    foreach (var meshFilter in meshFilters) {
      if (meshFilter.sharedMesh == null) {
        Debug.LogWarning($"MeshFilter on '{meshFilter.name}' has no mesh assigned, skipping.");
        continue;
      }

      combineList.Add(new CombineInstance {
        mesh = meshFilter.sharedMesh,
        transform = anchorWorldToLocal * meshFilter.transform.localToWorldMatrix,
      });
    }

    foreach (var skinned in skinnedRenderers) {
      if (skinned.sharedMesh == null) {
        Debug.LogWarning($"SkinnedMeshRenderer on '{skinned.name}' has no mesh assigned, skipping.");
        continue;
      }

      var bakedMesh = new Mesh();
      skinned.BakeMesh(bakedMesh, true);
      tempMeshes.Add(bakedMesh);

      combineList.Add(new CombineInstance {
        mesh = bakedMesh,
        transform = anchorWorldToLocal * skinned.transform.localToWorldMatrix,
      });
    }

    if (combineList.Count == 0) {
      Debug.LogWarning("Nothing to combine, aborting.");
      CleanupTempMeshes(tempMeshes);
      return;
    }

    var combinedMesh = new Mesh { name = anchor.name + "_Combined" };
    combinedMesh.CombineMeshes(combineList.ToArray(), true, true, false);

    var savePath = EditorUtility.SaveFilePanelInProject(
            "Save Combined Mesh",
            combinedMesh.name,
            "asset",
            "Choose location for the combined mesh asset.");

    if (string.IsNullOrEmpty(savePath)) {
      Object.DestroyImmediate(combinedMesh);
      CleanupTempMeshes(tempMeshes);
      return;
    }

    AssetDatabase.CreateAsset(combinedMesh, savePath);
    AssetDatabase.SaveAssets();

    var preview = new GameObject(anchor.name + "_Preview", typeof(MeshFilter), typeof(MeshRenderer));
    preview.hideFlags = HideFlags.DontSave;
    preview.GetComponent<MeshFilter>().sharedMesh = combinedMesh;

    var previewRenderer = preview.GetComponent<MeshRenderer>();
    var materials = GetFirstAvailableMaterials(meshFilters, skinnedRenderers);
    if (materials != null) {
      previewRenderer.sharedMaterials = materials;
    }

    preview.transform.SetPositionAndRotation(anchor.transform.position, anchor.transform.rotation);
    preview.transform.localScale = anchor.transform.lossyScale;

    CleanupTempMeshes(tempMeshes);
    Debug.Log($"Combined mesh created at {savePath}. Preview object '{preview.name}' instantiated in the scene.", preview);
  }

  static Material[] GetFirstAvailableMaterials(IList<MeshFilter> meshFilters, IList<SkinnedMeshRenderer> skinnedRenderers) {
    foreach (var meshFilter in meshFilters) {
      var renderer = meshFilter.GetComponent<MeshRenderer>();
      if (renderer != null && renderer.sharedMaterials != null && renderer.sharedMaterials.Length > 0) {
        return renderer.sharedMaterials;
      }
    }

    foreach (var skinned in skinnedRenderers) {
      if (skinned.sharedMaterials != null && skinned.sharedMaterials.Length > 0) {
        return skinned.sharedMaterials;
      }
    }

    return null;
  }

  static void CleanupTempMeshes(List<Mesh> meshes) {
    foreach (var mesh in meshes) {
      if (mesh != null) {
        Object.DestroyImmediate(mesh);
      }
    }
  }
}
#endif
