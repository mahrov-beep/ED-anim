using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class RemoveMissingScriptsInPrefabs : EditorWindow {
    [MenuItem("Tools/Remove Missing Scripts In Prefabs")]
    private static void ShowWindow() {
        GetWindow<RemoveMissingScriptsInPrefabs>("Remove Missing Scripts In Prefabs");
    }

    private void OnGUI() {
        if (GUILayout.Button("Remove All Missing Scripts In All Prefabs")) {
            RemoveAllMissingScripts();
        }
    }

    private void RemoveAllMissingScripts() {
        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        int total = 0;
        int changed = 0;

        for (int i = 0; i < guids.Length; i++) {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            bool wasChanged = false;
            var allObjects = new List<GameObject>();
            GetAllChildren(prefab.transform, allObjects);

            foreach (var go in allObjects) {
                int countBefore = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
                if (countBefore > 0) {
                    Undo.RegisterCompleteObjectUndo(go, "Remove missing scripts");
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                    wasChanged = true;
                    total += countBefore;
                }
            }

            if (wasChanged) {
                EditorUtility.SetDirty(prefab);
                PrefabUtility.SavePrefabAsset(prefab);
                changed++;
            }
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"Удалено всех missing scripts: {total}  |  Префабов изменено: {changed}");
    }

    private void GetAllChildren(Transform root, List<GameObject> list) {
        list.Add(root.gameObject);
        foreach (Transform child in root)
            GetAllChildren(child, list);
    }
}
