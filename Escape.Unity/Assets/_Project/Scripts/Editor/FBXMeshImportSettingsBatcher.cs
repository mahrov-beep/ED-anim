using UnityEngine;
using UnityEditor;

public class FBXMeshImportSettingsBatcher : EditorWindow {
    private ModelImporterMeshCompression meshCompression = ModelImporterMeshCompression.High;
    private bool readWriteEnabled = false;
    private bool generateColliders = false;

    [MenuItem("Tools/Batch FBX Mesh Import Settings")]
    static void Init() {
        GetWindow<FBXMeshImportSettingsBatcher>("Batch FBX Mesh Import Settings");
    }

    void OnGUI() {
        EditorGUILayout.LabelField("Meshes", EditorStyles.boldLabel);
        meshCompression = (ModelImporterMeshCompression)EditorGUILayout.EnumPopup("Mesh Compression", meshCompression);
        readWriteEnabled = EditorGUILayout.Toggle("Read/Write", readWriteEnabled);
        generateColliders = EditorGUILayout.Toggle("Generate Colliders", generateColliders);

        EditorGUILayout.Space();
        if (GUILayout.Button("Apply to All FBX in Project")) {
            ApplyToAllFBXMeshes();
        }
    }

    private void ApplyToAllFBXMeshes() {
        string[] guids = AssetDatabase.FindAssets("t:Model");
        int changed = 0;
        int skipped = 0;

        try {
            for (int i = 0; i < guids.Length; i++) {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (!path.ToLower().EndsWith(".fbx")) continue;
                var importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer == null) continue;

                // Показываем прогресс с именем файла
                bool canceled = EditorUtility.DisplayCancelableProgressBar(
                    "FBX Mesh Import Settings",
                    $"Processing {System.IO.Path.GetFileName(path)} ({i + 1}/{guids.Length})",
                    (float)i / guids.Length);

                if (canceled) {
                    Debug.LogWarning("Batch operation cancelled by user.");
                    break;
                }

                // Только если значения отличаются, меняем и сохраняем
                bool needReimport = 
                    importer.meshCompression != meshCompression ||
                    importer.isReadable != readWriteEnabled ||
                    importer.addCollider != generateColliders;

                if (needReimport) {
                    importer.meshCompression = meshCompression;
                    importer.isReadable = readWriteEnabled;
                    importer.addCollider = generateColliders;
                    EditorUtility.SetDirty(importer);
                    importer.SaveAndReimport();
                    changed++;
                } else {
                    skipped++;
                }
            }
        } finally {
            EditorUtility.ClearProgressBar();
        }
        Debug.Log($"Изменено FBX: {changed}, пропущено (уже совпадает): {skipped}");
    }
}
