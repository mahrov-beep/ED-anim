using UnityEngine;
using UnityEditor;

public class DownscaleTerrainWithFixedAlpha : EditorWindow {
    int newResolution = 513;

    [MenuItem("Tools/Downscale Terrain (Preserve Paint)")]
    public static void ShowWindow() {
        GetWindow<DownscaleTerrainWithFixedAlpha>("Downscale Terrain");
    }

    void OnGUI() {
        GUILayout.Label("New Heightmap Resolution", EditorStyles.boldLabel);
        newResolution = EditorGUILayout.IntPopup(newResolution,
            new[] { "33", "65", "129", "257", "513", "1025", "2049", "4097" },
            new[] { 33, 65, 129, 257, 513, 1025, 2049, 4097 });

        if (GUILayout.Button("Downscale Terrain")) {
            Downscale();
        }
    }

    void Downscale() {
        Terrain terrain = Selection.activeGameObject?.GetComponent<Terrain>();
        if (!terrain) {
            Debug.LogError("Выдели GameObject с Terrain.");
            return;
        }

        TerrainData oldData = terrain.terrainData;
        int oldRes = oldData.heightmapResolution;
        int oldVertices = oldRes * oldRes;
        int oldTriangles = (oldRes - 1) * (oldRes - 1) * 2;

        float[,] oldHeights = oldData.GetHeights(0, 0, oldRes, oldRes);
        float[,] newHeights = new float[newResolution, newResolution];

        for (int y = 0; y < newResolution; y++) {
            for (int x = 0; x < newResolution; x++) {
                float gx = (float)x / (newResolution - 1) * (oldRes - 1);
                float gy = (float)y / (newResolution - 1) * (oldRes - 1);
                newHeights[y, x] = oldHeights[Mathf.RoundToInt(gy), Mathf.RoundToInt(gx)];
            }
        }

        TerrainData newData = new TerrainData();
        newData.heightmapResolution = newResolution;
        newData.size = oldData.size;
        newData.SetHeights(0, 0, newHeights);

        newData.alphamapResolution = oldData.alphamapResolution;
        newData.baseMapResolution = oldData.baseMapResolution;
        newData.terrainLayers = oldData.terrainLayers;

        int oldAWidth = oldData.alphamapWidth;
        int oldAHeight = oldData.alphamapHeight;
        int layers = oldData.alphamapLayers;
        int newAWidth = newData.alphamapWidth;
        int newAHeight = newData.alphamapHeight;

        float[,,] oldAlpha = oldData.GetAlphamaps(0, 0, oldAWidth, oldAHeight);
        float[,,] newAlpha = new float[newAWidth, newAHeight, layers];

        for (int y = 0; y < newAHeight; y++) {
            for (int x = 0; x < newAWidth; x++) {
                float gx = (float)x / (newAWidth - 1) * (oldAWidth - 1);
                float gy = (float)y / (newAHeight - 1) * (oldAHeight - 1);
                int ix = Mathf.Clamp(Mathf.FloorToInt(gx), 0, oldAWidth - 1);
                int iy = Mathf.Clamp(Mathf.FloorToInt(gy), 0, oldAHeight - 1);

                float total = 0f;
                for (int l = 0; l < layers; l++) {
                    float value = oldAlpha[ix, iy, l];
                    newAlpha[x, y, l] = value;
                    total += value;
                }

                if (total > 0f) {
                    for (int l = 0; l < layers; l++) {
                        newAlpha[x, y, l] /= total;
                    }
                }
            }
        }

        newData.SetAlphamaps(0, 0, newAlpha);
        newData.RefreshPrototypes();

        newData.SetDetailResolution(oldData.detailResolution, oldData.detailResolutionPerPatch);
        newData.detailPrototypes = oldData.detailPrototypes;
        for (int i = 0; i < oldData.detailPrototypes.Length; i++) {
            var layer = oldData.GetDetailLayer(0, 0, oldData.detailWidth, oldData.detailHeight, i);
            newData.SetDetailLayer(0, 0, i, layer);
        }

        newData.treePrototypes = oldData.treePrototypes;
        newData.treeInstances = oldData.treeInstances;

        string path = "Assets/DownscaledTerrainData.asset";
        AssetDatabase.CreateAsset(newData, path);
        AssetDatabase.SaveAssets();

        terrain.Flush();
        terrain.terrainData = null;
        terrain.terrainData = newData;

        int newVertices = newResolution * newResolution;
        int newTriangles = (newResolution - 1) * (newResolution - 1) * 2;

        Debug.Log($"✅ Terrain downscaled: {oldRes} → {newResolution}");
        Debug.Log($"🔹 Vertices: {oldVertices} → {newVertices}");
        Debug.Log($"🔹 Triangles: {oldTriangles} → {newTriangles}");
        Debug.Log($"📦 New TerrainData saved at: {path}");
    }
}
