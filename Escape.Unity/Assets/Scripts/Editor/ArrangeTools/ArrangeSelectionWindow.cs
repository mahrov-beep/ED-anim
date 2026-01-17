namespace Scripts.Editor {
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public class ArrangeSelectionWindow : EditorWindow {
        private enum PresetType { Grid, Line, Circle, Arc, Spiral }
        private enum AxisPlane { XZ, XY, YZ }
        private enum AxisDirection { XPositive, XNegative, YPositive, YNegative, ZPositive, ZNegative }

        private PresetType preset = PresetType.Grid;
        private bool useAnchorRotation = true;

        private int gridRows = 1;
        private int gridColumns = 1;
        private float gridSpacingX = 1f;
        private float gridSpacingZ = 1f;

        private float lineSpacing = 1f;
        private AxisDirection lineDirection = AxisDirection.XPositive;

        private float circleRadius = 5f;
        private float circleStartAngle = 0f;
        private float circleArc = 360f;
        private AxisPlane circlePlane = AxisPlane.XZ;
        private bool circleClockwise = false;

        private float spiralRadiusStart = 1f;
        private float spiralRadiusStep = 0.5f;
        private float spiralAngleStep = 30f;
        private float spiralStartAngle = 0f;
        private AxisPlane spiralPlane = AxisPlane.XZ;

        [MenuItem("Tools/Arrange Selection...")]
        public static void ShowWindow() {
            GetWindow<ArrangeSelectionWindow>("Arrange");
        }

        private void OnGUI() {
            var selection = Selection.GetTransforms(SelectionMode.TopLevel | SelectionMode.Editable);
            var anchor = Selection.activeTransform;

            EditorGUILayout.LabelField("Selected objects", selection.Length.ToString());
            EditorGUILayout.LabelField("Anchor", anchor != null ? anchor.name : "<none>");
            EditorGUILayout.Space(4);

            preset = (PresetType)EditorGUILayout.EnumPopup("Preset", preset);
            useAnchorRotation = EditorGUILayout.ToggleLeft("Use anchor rotation", useAnchorRotation);
            EditorGUILayout.Space(4);

            switch (preset) {
                case PresetType.Grid:
                    gridRows = Mathf.Max(1, EditorGUILayout.IntField("Rows", gridRows));
                    gridColumns = Mathf.Max(1, EditorGUILayout.IntField("Columns", gridColumns));
                    gridSpacingX = EditorGUILayout.FloatField("Spacing X", gridSpacingX);
                    gridSpacingZ = EditorGUILayout.FloatField("Spacing Z", gridSpacingZ);
                    break;
                case PresetType.Line:
                    lineDirection = (AxisDirection)EditorGUILayout.EnumPopup("Direction", lineDirection);
                    lineSpacing = EditorGUILayout.FloatField("Spacing", lineSpacing);
                    break;
                case PresetType.Circle:
                    circleRadius = EditorGUILayout.FloatField("Radius", circleRadius);
                    circleStartAngle = EditorGUILayout.FloatField("Start angle", circleStartAngle);
                    circlePlane = (AxisPlane)EditorGUILayout.EnumPopup("Plane", circlePlane);
                    circleClockwise = EditorGUILayout.ToggleLeft("Clockwise", circleClockwise);
                    break;
                case PresetType.Arc:
                    circleRadius = EditorGUILayout.FloatField("Radius", circleRadius);
                    circleStartAngle = EditorGUILayout.FloatField("Start angle", circleStartAngle);
                    circleArc = EditorGUILayout.FloatField("Arc", circleArc);
                    circlePlane = (AxisPlane)EditorGUILayout.EnumPopup("Plane", circlePlane);
                    circleClockwise = EditorGUILayout.ToggleLeft("Clockwise", circleClockwise);
                    break;
                case PresetType.Spiral:
                    spiralRadiusStart = EditorGUILayout.FloatField("Start radius", spiralRadiusStart);
                    spiralRadiusStep = EditorGUILayout.FloatField("Radius step", spiralRadiusStep);
                    spiralStartAngle = EditorGUILayout.FloatField("Start angle", spiralStartAngle);
                    spiralAngleStep = EditorGUILayout.FloatField("Angle step", spiralAngleStep);
                    spiralPlane = (AxisPlane)EditorGUILayout.EnumPopup("Plane", spiralPlane);
                    break;
            }

            EditorGUILayout.Space(8);
            using (new EditorGUI.DisabledScope(anchor == null || selection.Length < 2)) {
                if (GUILayout.Button("Apply")) {
                    ApplyArrangement(anchor, selection);
                }
            }
        }

        private void ApplyArrangement(Transform anchor, Transform[] selection) {
            if (anchor == null) return;
            var ordered = new List<Transform>();
            foreach (var t in selection) {
                if (t != null && t != anchor) ordered.Add(t);
            }
            if (ordered.Count == 0) return;

            Undo.RecordObjects(ordered.ToArray(), "Arrange Selection");

            var basis = useAnchorRotation ? anchor.rotation : Quaternion.identity;

            switch (preset) {
                case PresetType.Grid:
                    ApplyGrid(anchor, ordered, basis);
                    break;
                case PresetType.Line:
                    ApplyLine(anchor, ordered, basis);
                    break;
                case PresetType.Circle:
                    ApplyCircleOrArc(anchor, ordered, basis, circleRadius, circleStartAngle, 360f, circlePlane, circleClockwise);
                    break;
                case PresetType.Arc:
                    ApplyCircleOrArc(anchor, ordered, basis, circleRadius, circleStartAngle, circleArc, circlePlane, circleClockwise);
                    break;
                case PresetType.Spiral:
                    ApplySpiral(anchor, ordered, basis);
                    break;
            }

            SceneView.RepaintAll();
        }

        private void ApplyGrid(Transform anchor, List<Transform> items, Quaternion basis) {
            int cols = Mathf.Max(1, gridColumns);
            int totalPoints = items.Count + 1;
            int requiredRows = Mathf.CeilToInt((float)totalPoints / cols);
            int rows = Mathf.Max(gridRows, requiredRows);
            for (int i = 0; i < items.Count; i++) {
                int k = i + 1;
                int r = k / cols;
                int c = k % cols;
                var local = new Vector3(c * gridSpacingX, 0f, r * gridSpacingZ);
                var world = anchor.position + basis * local;
                items[i].position = world;
            }
        }

        private void ApplyLine(Transform anchor, List<Transform> items, Quaternion basis) {
            var dir = GetDirection(lineDirection);
            for (int i = 0; i < items.Count; i++) {
                var local = dir * lineSpacing * (i + 1);
                var world = anchor.position + basis * local;
                items[i].position = world;
            }
        }

        private void ApplyCircleOrArc(Transform anchor, List<Transform> items, Quaternion basis, float radius, float startAngleDeg, float arcDeg, AxisPlane plane, bool clockwise) {
            int movedCount = items.Count;
            if (movedCount == 0) return;
            int totalPoints = movedCount + 1;
            float arcAbs = Mathf.Abs(arcDeg);
            bool isFull = Mathf.Approximately(Mathf.Repeat(arcAbs, 360f), 0f) || Mathf.Approximately(arcAbs, 360f);
            float step = isFull ? (360f / totalPoints) : (arcAbs / Mathf.Max(1, totalPoints - 1));
            float sign = clockwise ? -1f : 1f;

            float startRad = startAngleDeg * Mathf.Deg2Rad;
            Vector3 startLocal = PlaneOffset(plane, radius, startRad);
            Vector3 origin = anchor.position - basis * startLocal;

            for (int i = 0; i < movedCount; i++) {
                int k = i + 1;
                float a = (startAngleDeg + sign * step * k) * Mathf.Deg2Rad;
                Vector3 local = PlaneOffset(plane, radius, a);
                var world = origin + basis * local;
                items[i].position = world;
            }
        }

        private void ApplySpiral(Transform anchor, List<Transform> items, Quaternion basis) {
            float r0 = spiralRadiusStart;
            float a0 = spiralStartAngle * Mathf.Deg2Rad;
            Vector3 startLocal = PlaneOffset(spiralPlane, r0, a0);
            Vector3 origin = anchor.position - basis * startLocal;
            for (int i = 0; i < items.Count; i++) {
                int k = i + 1;
                float r = spiralRadiusStart + spiralRadiusStep * k;
                float a = (spiralStartAngle + spiralAngleStep * k) * Mathf.Deg2Rad;
                Vector3 local = PlaneOffset(spiralPlane, r, a);
                var world = origin + basis * local;
                items[i].position = world;
            }
        }

        private static Vector3 GetDirection(AxisDirection d) {
            switch (d) {
                case AxisDirection.XNegative: return Vector3.left;
                case AxisDirection.YPositive: return Vector3.up;
                case AxisDirection.YNegative: return Vector3.down;
                case AxisDirection.ZPositive: return Vector3.forward;
                case AxisDirection.ZNegative: return Vector3.back;
                default: return Vector3.right;
            }
        }

        private static Vector3 PlaneOffset(AxisPlane plane, float radius, float angleRad) {
            switch (plane) {
                case AxisPlane.XY: return new Vector3(Mathf.Cos(angleRad) * radius, Mathf.Sin(angleRad) * radius, 0f);
                case AxisPlane.YZ: return new Vector3(0f, Mathf.Cos(angleRad) * radius, Mathf.Sin(angleRad) * radius);
                default: return new Vector3(Mathf.Cos(angleRad) * radius, 0f, Mathf.Sin(angleRad) * radius);
            }
        }
    }
}


