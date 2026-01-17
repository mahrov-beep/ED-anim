
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Photon.Deterministic;
using Quantum;

public class ColliderGenerator : EditorWindow
{
    private string assetsFolderPath = "Assets/ConstructionSite";

    [MenuItem("Tools/Quantum/Generate Colliders for Prefabs")]
    private static void ShowWindow()
    {
        GetWindow<ColliderGenerator>().Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("Collider Generator", EditorStyles.boldLabel);
        assetsFolderPath = EditorGUILayout.TextField("Assets Folder Path:", assetsFolderPath);

        if (GUILayout.Button("Generate Box Colliders"))
        {
            ProcessPrefabs();
        }
    }

    private void ProcessPrefabs()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { assetsFolderPath });
        int totalPrefabs = 0;
        int modifiedPrefabs = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefabRoot);

            bool modified = false;

            var lodGroup = instance.GetComponentInChildren<LODGroup>();
            Renderer targetRenderer = null;

            if (lodGroup != null)
            {
                var lods = lodGroup.GetLODs();
                if (lods.Length > 0 && lods[0].renderers.Length > 0)
                {
                    targetRenderer = lods[0].renderers[0];
                }
            }
            else
            {
                // Fallback: Get first MeshRenderer in hierarchy
                targetRenderer = instance.GetComponentInChildren<MeshRenderer>();
            }

            if (targetRenderer != null)
            {
                var meshFilter = targetRenderer.GetComponent<MeshFilter>();
                if (meshFilter != null)
                {
                    var bounds = meshFilter.sharedMesh.bounds;
                    var worldBounds = TransformBounds(bounds, targetRenderer.transform);

                    BoxCollider box = instance.AddComponent<BoxCollider>();
                    box.center = instance.transform.InverseTransformPoint(worldBounds.center);
                    box.size = worldBounds.size;

                    var quantumCollider = instance.AddComponent<QuantumStaticBoxCollider3D>();
                    quantumCollider.SourceCollider = box;

                    modified = true;
                }
            }

            foreach (var meshCollider in instance.GetComponentsInChildren<MeshCollider>())
            {
                meshCollider.enabled = false;
            }

            if (modified)
            {
                PrefabUtility.SaveAsPrefabAsset(instance, path);
                modifiedPrefabs++;
            }

            DestroyImmediate(instance);
            totalPrefabs++;
        }

        Debug.Log($"Processed {totalPrefabs} prefabs. Modified {modifiedPrefabs} prefabs.");
    }

    private Bounds TransformBounds(Bounds bounds, Transform transform)
    {
        Vector3 center = transform.TransformPoint(bounds.center);
        Vector3 size = Vector3.Scale(bounds.size, transform.lossyScale);
        return new Bounds(center, size);
    }
}
