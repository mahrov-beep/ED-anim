using UnityEngine;
using UnityEditor;

public class SkinnedMeshRebinder : EditorWindow {
    private SkinnedMeshRenderer sourceSkinnedMesh;
    private Transform           targetRootBone;

    [MenuItem("Tools/Rebind Skinned Mesh To Another Rig")]
    private static void ShowWindow() {
        GetWindow<SkinnedMeshRebinder>("Rebind Skinned Mesh");
    }

    private void OnGUI() {
        sourceSkinnedMesh = (SkinnedMeshRenderer)EditorGUILayout.ObjectField("Source SkinnedMeshRenderer", sourceSkinnedMesh, typeof(SkinnedMeshRenderer), true);
        targetRootBone    = (Transform)EditorGUILayout.ObjectField("New Root Bone", targetRootBone, typeof(Transform), true);

        if (GUILayout.Button("Rebind To New Rig")) {
            if (sourceSkinnedMesh != null && targetRootBone != null) {
                RebindSkinnedMesh(sourceSkinnedMesh, targetRootBone);
                Debug.Log("SkinnedMeshRenderer bones successfully rebound.");
            } else {
                Debug.LogWarning("Please assign both SkinnedMeshRenderer and New Root Bone.");
            }
        }
    }

    private void RebindSkinnedMesh(SkinnedMeshRenderer smr, Transform newRigRoot) {
        var         oldBones = smr.bones;
        Transform[] newBones = new Transform[oldBones.Length];
        for (int i = 0; i < oldBones.Length; i++) {
            newBones[i] = FindChildByName(newRigRoot, oldBones[i].name);
            if (newBones[i] == null) {
                Debug.LogWarning($"Bone '{oldBones[i].name}' not found under new rig root.");
            }
        }
        smr.rootBone = newRigRoot;
        smr.bones    = newBones;
    }

    private Transform FindChildByName(Transform root, string name) {
        foreach (var t in root.GetComponentsInChildren<Transform>(true)) {
            if (t.name == name) return t;
        }
        return null;
    }
}