#if UNITY_EDITOR

using Quantum;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(QPrototypeSpawnPoint))]
public class SpawnPointEditor : Quantum.Editor.QuantumUnityComponentPrototypeEditor {
    public static float globalSnapRadius = 20f;

    public override void DrawInternalGUI() {
        var spawnPoint = (QPrototypeSpawnPoint)target;

        EditorGUILayout.LabelField($"Spawn Point ID = {spawnPoint.Prototype.ID}");
        EditorGUILayout.Space(10);

        globalSnapRadius = EditorGUILayout.FloatField("Snap Radius", globalSnapRadius);

        if (GUILayout.Button("Snap to closest nav mesh point")) {
            SnapToClosestNavMesh(spawnPoint, globalSnapRadius);
        }

        EditorGUILayout.Space(10);
        base.DrawInternalGUI();
    }

    private void SnapToClosestNavMesh(QPrototypeSpawnPoint spawnPoint, float radius) {
        var pos = spawnPoint.transform.position;
        if (NavMeshUtils.FindClosestNavMeshPoint(pos, radius, out var closest)) {
            Undo.RecordObject(spawnPoint.transform, "Snap SpawnPoint To NavMesh");
            spawnPoint.transform.position = closest;
            EditorUtility.SetDirty(spawnPoint);
            EditorUtility.SetDirty(spawnPoint.transform);

            SceneView.lastActiveSceneView.pivot = closest;
            SceneView.lastActiveSceneView.Repaint();
        } else {
            EditorUtility.DisplayDialog("Snap to NavMesh", "Не найдено подходящей точки NavMesh в радиусе", "OK");
        }
    }
}
#endif