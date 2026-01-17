#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.AI;
using Photon.Deterministic;
using Quantum;
using System.Collections.Generic;
using NavMesh = UnityEngine.AI.NavMesh;

[CustomEditor(typeof(QPrototypeWay))]
public class WaypointsEditor : Quantum.Editor.QuantumUnityComponentPrototypeEditor {
    public static float coverageSampleRadius       = 4f;
    public static float navMeshMinimumEdgeDistance = 0.5f;

    public override void DrawInternalGUI() {
        var way = (QPrototypeWay)target;
        EditorGUILayout.LabelField($"Way ID = {way.Prototype.id}");

        EditorGUILayout.Space(20);

        DrawCoverageRadiusField();
        EditorGUILayout.Space(5);
        DrawNavMeshMinimumEdgeDistanceField();

        EditorGUILayout.Space(20);

        DrawWaypointsList();
        EditorGUILayout.Space(5);
        DrawWaypointButtons();

        EditorGUILayout.Space(10);
        base.DrawInternalGUI();
    }

    private void DrawCoverageRadiusField() {
        EditorGUILayout.HelpBox("Это дистанция с которой точка притянется к ближайшему навмешу", MessageType.Info);
        coverageSampleRadius = EditorGUILayout.FloatField("Coverage Sample Radius", coverageSampleRadius);
    }

    private void DrawNavMeshMinimumEdgeDistanceField() {
        EditorGUILayout.HelpBox("Если край навмеша ближе этого значения к вейпоинту, то вейпоинт считается невалидным", MessageType.Info);
        navMeshMinimumEdgeDistance = EditorGUILayout.FloatField("NavMesh Min Edge Dist", navMeshMinimumEdgeDistance);
    }

    private void DrawWaypointsList() {
        var way = (QPrototypeWay)target;
        if (way.Prototype?.Points == null) return;

        for (var index = 0; index < way.Prototype.Points.Length; index++) {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Waypoint {index}", GUILayout.Width(100));

            if (GUILayout.Button("X", GUILayout.Width(20))) {
                RemoveWaypoint(way, index);
                EditorUtility.SetDirty(way.gameObject);
                break;
            }

            var fpPosition = way.Prototype.Points[index];
            var x          = fpPosition.X.AsFloat;
            var y          = fpPosition.Y.AsFloat;
            var z          = fpPosition.Z.AsFloat;
            var position   = new Vector3(x, y, z);

            var isValid = IsPointReachable(position, 0.2f)
                          && (index == 0 || IsReachableBetween(way.Prototype.Points[index - 1].ToUnityVector3(), position, 0.2f))
                          && GetDistanceToNavMeshBoundary(position) >= navMeshMinimumEdgeDistance;

            var defaultColor = GUI.backgroundColor;
            GUI.backgroundColor = isValid ? Color.green : Color.red;

            if (GUILayout.Button("Goto", GUILayout.Width(60))) {
                SceneView.lastActiveSceneView.pivot = position;
                SceneView.lastActiveSceneView.Repaint();
            }
            GUI.backgroundColor = defaultColor;

            var newX = EditorGUILayout.FloatField(x, GUILayout.Width(70));
            var newY = EditorGUILayout.FloatField(y, GUILayout.Width(70));
            var newZ = EditorGUILayout.FloatField(z, GUILayout.Width(70));

            if (!Mathf.Approximately(newX, x) || !Mathf.Approximately(newY, y) || !Mathf.Approximately(newZ, z)) {
                Undo.RecordObject(way, "Edit Waypoint");
                way.Prototype.Points[index] = new FPVector3(FP.FromFloat_UNSAFE(newX), FP.FromFloat_UNSAFE(newY), FP.FromFloat_UNSAFE(newZ));
                EditorUtility.SetDirty(way.gameObject);
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    private void DrawWaypointButtons() {
        var way = (QPrototypeWay)target;

        if (GUILayout.Button(nameof(AddWaypoint))) {
            AddWaypoint(way);
            EditorUtility.SetDirty(way.gameObject);
        }

        if (GUILayout.Button(nameof(InverseWaypoints))) {
            InverseWaypoints(way);
            EditorUtility.SetDirty(way.gameObject);
        }
    }

    private void OnSceneGUI() {
        DrawWaypointsHandles();
    }

    private void DrawWaypointsHandles() {
        var qPrototypeWay = (QPrototypeWay)target;
        if (qPrototypeWay.Prototype?.Points == null) return;

        var previousColor = Handles.color;
        var labelStyle    = new GUIStyle { fontSize = 50 };

        for (var index = 0; index < qPrototypeWay.Prototype.Points.Length; index++) {
            var position = qPrototypeWay.Prototype.Points[index].ToUnityVector3();
            position                              = SnapToNavMesh(position, coverageSampleRadius);
            qPrototypeWay.Prototype.Points[index] = position.ToFPVector3();

            EditorGUI.BeginChangeCheck();
            var newPosition = Handles.PositionHandle(position, Quaternion.identity);
            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(qPrototypeWay, "Move Waypoint");
                newPosition                           = SnapToNavMesh(newPosition, coverageSampleRadius);
                qPrototypeWay.Prototype.Points[index] = newPosition.ToFPVector3();
                EditorUtility.SetDirty(qPrototypeWay.gameObject);
            }

            var localReachable        = IsPointReachable(position, 0.2f);
            var fromPreviousReachable = index == 0 || IsReachableBetween(qPrototypeWay.Prototype.Points[index - 1].ToUnityVector3(), position, 0.2f);
            var distanceToNavMeshEdge = GetDistanceToNavMeshBoundary(position);
            var isValid               = localReachable && fromPreviousReachable && distanceToNavMeshEdge >= navMeshMinimumEdgeDistance;

            Handles.color = isValid ? Color.green : Color.red;
            Handles.SphereHandleCap(0, newPosition, Quaternion.identity, 0.2f, EventType.Repaint);
            Handles.Label(newPosition + Vector3.up * 0.3f, index.ToString(), labelStyle);

            if (!isValid) {
                Handles.color = Color.red;
                Handles.DrawWireDisc(position, Vector3.up, 1f);
            }

            if (index > 0) {
                var previousPosition = qPrototypeWay.Prototype.Points[index - 1].ToUnityVector3();
                Handles.DrawLine(previousPosition, newPosition);
            }
        }
        Handles.color = previousColor;
    }

    private bool IsPointReachable(Vector3 position, float maxDistance) {
        if (NavMesh.SamplePosition(position, out var hit, maxDistance, NavMesh.AllAreas)) {
            if (Vector3.Distance(position, hit.position) <= maxDistance) {
                if (!NavMesh.Raycast(position, hit.position, out _, NavMesh.AllAreas)) {
                    return true;
                }
            }
        }
        return false;
    }

    private bool IsReachableBetween(Vector3 from, Vector3 to, float maxDistance) {
        if (!NavMesh.SamplePosition(from, out var hitFrom, maxDistance, NavMesh.AllAreas)) return false;
        if (!NavMesh.SamplePosition(to, out var hitTo, maxDistance, NavMesh.AllAreas)) return false;
        var path = new NavMeshPath();
        NavMesh.CalculatePath(hitFrom.position, hitTo.position, NavMesh.AllAreas, path);
        return path.status == NavMeshPathStatus.PathComplete;
    }

    private float GetDistanceToNavMeshBoundary(Vector3 position) {
        return NavMesh.FindClosestEdge(position, out var hit, NavMesh.AllAreas) ? hit.distance : 0f;
    }

    private Vector3 SnapToNavMesh(Vector3 position, float maxDistance) {
        if (NavMesh.SamplePosition(position, out var hit, maxDistance, NavMesh.AllAreas)) {
            return hit.position;
        }
        return position;
    }

    private void AddWaypoint(QPrototypeWay waypoints) {
        var pointsList = new List<FPVector3>(waypoints.Prototype.Points) { waypoints.Prototype.Points[^1] };
        Undo.RecordObject(waypoints, "Add Waypoint");
        waypoints.Prototype.Points = pointsList.ToArray();
    }

    private void RemoveWaypoint(QPrototypeWay waypoints, int index) {
        var pointsList = new List<FPVector3>(waypoints.Prototype.Points);
        if (index >= 0 && index < pointsList.Count) {
            pointsList.RemoveAt(index);
            Undo.RecordObject(waypoints, "Remove Waypoint");
            waypoints.Prototype.Points = pointsList.ToArray();
        }
    }

    private static void RandomSortWaypoints(QPrototypeWay waypoints) {
        var pointsList = new List<FPVector3>(waypoints.Prototype.Points);
        for (var index = 0; index < pointsList.Count; index++) {
            var temp        = pointsList[index];
            var randomIndex = Random.Range(index, pointsList.Count);
            pointsList[index]       = pointsList[randomIndex];
            pointsList[randomIndex] = temp;
        }
        Undo.RecordObject(waypoints, "Random Sort Waypoints");
        waypoints.Prototype.Points = pointsList.ToArray();
    }

    private static void InverseWaypoints(QPrototypeWay waypoints) {
        var pointsList = new List<FPVector3>(waypoints.Prototype.Points);
        pointsList.Reverse();
        Undo.RecordObject(waypoints, "Inverse Waypoints");
        waypoints.Prototype.Points = pointsList.ToArray();
    }
}
#endif