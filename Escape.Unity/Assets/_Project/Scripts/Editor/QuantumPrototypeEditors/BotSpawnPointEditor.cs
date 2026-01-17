#if UNITY_EDITOR

using Quantum;
using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(QPrototypeBotSpawnPoint))]
public class BotSpawnPointEditor : Quantum.Editor.QuantumUnityComponentPrototypeEditor {
    public static float globalSnapRadius = 20f;

    public override void DrawInternalGUI() {
        var botSpawnPoint = (QPrototypeBotSpawnPoint)target;

        EditorGUILayout.LabelField($"Bot Spawn Point");
        EditorGUILayout.Space(10);

        globalSnapRadius = EditorGUILayout.FloatField("Snap Radius", globalSnapRadius);

        if (GUILayout.Button("Snap to closest nav mesh point")) {
            SnapToClosestNavMesh(botSpawnPoint, globalSnapRadius);
        }

        EditorGUILayout.Space(10);
        base.DrawInternalGUI();
    }

    private void SnapToClosestNavMesh(QPrototypeBotSpawnPoint botSpawnPoint, float radius) {
        var pos = botSpawnPoint.transform.position;
        if (NavMeshUtils.FindClosestNavMeshPoint(pos, radius, out var closest)) {
            Undo.RecordObject(botSpawnPoint.transform, "Snap BotSpawnPoint To NavMesh");
            botSpawnPoint.transform.position = closest;
            EditorUtility.SetDirty(botSpawnPoint);
            EditorUtility.SetDirty(botSpawnPoint.transform);

            SceneView.lastActiveSceneView.pivot = closest;
            SceneView.lastActiveSceneView.Repaint();
        } else {
            EditorUtility.DisplayDialog("Snap to NavMesh", "Не найдено подходящей точки NavMesh в радиусе", "OK");
        }
    }
}
#endif