using System.IO;
using _Project.Scripts.Scopes;
using Quantum;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class MinimapBakerProcessor {
    private const int    SCREENSHOT_SIZE_MULTIPLIER = 2;
    private const string FILE_NAME                  = "MinimapBaked.png";

    public static void Bake(QuantumMapData data) {
        BakeLevelToSprite(data);
    }

    public static void BakeLevelToSprite(QuantumMapData data) {
        var asset = data.GetAsset(true);

        var levelSize = GetLevelSize(asset.GridSizeX, asset.GridSizeY, asset.GridNodeSize);
        var (w, h) = GetScreenshotSize(levelSize);

        using var camScope = CameraScope.Acquire();
        SetupCamera(camScope.Camera, levelSize);

        using var rtScope = RenderTextureScope.Acquire(w, h);
        camScope.Camera.targetTexture = rtScope.Texture;

        var shot = Capture(camScope.Camera, rtScope.Texture, w, h);


        SaveToDisk(shot);
    }

    private static int GetLevelSize(int gridSizeX, int gridSizeY, int nodeSize) {
        var sizeX = gridSizeX * nodeSize;
        var sizeY = gridSizeY * nodeSize;
        Assert.Check(sizeX == sizeY, "Not square level is not supported!");
        return sizeX; // == sizeY
    }

    private static (int, int) GetScreenshotSize(int levelSize) {
        var s = levelSize * SCREENSHOT_SIZE_MULTIPLIER;
        return (s, s);
    }

    private static void SetupCamera(Camera cam, int levelSize) {
        var terrain          = Object.FindFirstObjectByType<Terrain>();
        var terrainMaxHeight = terrain ? terrain.terrainData.size.y : 10f;
    
        cam.transform.SetPositionAndRotation(Vector3.up * (terrainMaxHeight + 1f), Quaternion.Euler(90, 0, 0));
        cam.orthographic     = true;
        cam.orthographicSize = levelSize * 0.5f;
        cam.nearClipPlane    = 0.01f;
        cam.farClipPlane     = terrainMaxHeight + 10f;
    }

    private static Texture2D Capture(Camera cam, RenderTexture rt, int w, int h) {
        var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
        RenderTexture.active = rt;
        cam.Render();
        tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        tex.Apply();        
        cam.targetTexture = null;

        return tex;
    }

    private static void SaveToDisk(Texture2D tex) {
        var sceneName = SceneManager.GetActiveScene().name;
        var name      = $"{sceneName} - {FILE_NAME}";
        var path      = Path.Combine(Application.dataPath, "_Project/Sprites/Map", name);
        File.WriteAllBytes(path, tex.EncodeToPNG());

#if UNITY_EDITOR
        AssetDatabase.Refresh();
        var assetPath = "Assets/_Project/Sprites/Map/" + name;
        if (AssetImporter.GetAtPath(assetPath) is TextureImporter imp) {
            imp.textureType      = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.SaveAndReimport();
        }
        Debug.Log($"Sprite {name} saved to {assetPath}");
#endif
    }
}