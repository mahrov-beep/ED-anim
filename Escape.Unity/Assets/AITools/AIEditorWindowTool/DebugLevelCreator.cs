using UnityEditor;
using UnityEngine;

namespace AITools.AIEditorWindowTool {
    public class DebugLevelCreator : AIEditorWindow {
        [SerializeField] private Vector2    levelSize                = new Vector2(20f, 50f);
        [SerializeField] private float      wallHeight               = 3f;
        [SerializeField] private int        corridorCount            = 3;
        [SerializeField] private int        obstaclesPerCorridor     = 5;
        [SerializeField] private int        blockingWallsPerCorridor = 2;
        [SerializeField] private float      maxObstacleHeight        = 2f;
        [SerializeField] private GameObject wallPrefab;

        private SerializedObject   serializedObject;
        private SerializedProperty levelSizeProp;
        private SerializedProperty wallHeightProp;
        private SerializedProperty corridorCountProp;
        private SerializedProperty obstaclesPerCorridorProp;
        private SerializedProperty blockingWallsPerCorridorProp;
        private SerializedProperty maxObstacleHeightProp;
        private SerializedProperty wallPrefabProp;

        private const string RootName = "GeneratedLevelRoot";

        [MenuItem("GPTGenerated/" + nameof(DebugLevelCreator))]
        public static void ShowWindow() {
            GetWindow<DebugLevelCreator>(nameof(DebugLevelCreator));
        }

        protected override void OnEnable() {
            base.OnEnable();
            serializedObject             = new SerializedObject(this);
            levelSizeProp                = serializedObject.FindProperty("levelSize");
            wallHeightProp               = serializedObject.FindProperty("wallHeight");
            corridorCountProp            = serializedObject.FindProperty("corridorCount");
            obstaclesPerCorridorProp     = serializedObject.FindProperty("obstaclesPerCorridor");
            blockingWallsPerCorridorProp = serializedObject.FindProperty("blockingWallsPerCorridor");
            maxObstacleHeightProp        = serializedObject.FindProperty("maxObstacleHeight");
            wallPrefabProp               = serializedObject.FindProperty("wallPrefab");
        }

        public override void OnGUI() {
            base.OnGUI();
            serializedObject.Update();

            GUILayout.Label("Debug Level Creator", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(levelSizeProp, new GUIContent("Level Size (Width x Length)"));
            EditorGUILayout.Slider(wallHeightProp, 0.5f, 10f, new GUIContent("Wall Height"));
            EditorGUILayout.IntSlider(corridorCountProp, 1, 20, new GUIContent("Corridor Count"));
            EditorGUILayout.IntSlider(obstaclesPerCorridorProp, 0, 50, new GUIContent("Obstacles Per Corridor"));
            EditorGUILayout.IntSlider(blockingWallsPerCorridorProp, 0, 20, new GUIContent("Blocking Walls Per Corridor"));
            EditorGUILayout.Slider(maxObstacleHeightProp, 0.1f, 5f, new GUIContent("Max Obstacle Height"));
            EditorGUILayout.PropertyField(wallPrefabProp, new GUIContent("Wall Prefab (Cube)"));

            EditorGUILayout.Space();

            bool canGenerate =
                            wallPrefab != null &&
                            levelSize.x > 0.1f &&
                            levelSize.y > 0.1f &&
                            corridorCount > 0 &&
                            wallHeight > 0.1f &&
                            maxObstacleHeight > 0.05f &&
                            obstaclesPerCorridor >= 0 &&
                            blockingWallsPerCorridor >= 0;

            EditorGUI.BeginDisabledGroup(!canGenerate);
            if (GUILayout.Button("Generate")) {
                GenerateLevel();
            }
            EditorGUI.EndDisabledGroup();

            if (!canGenerate)
                EditorGUILayout.HelpBox("Assign Wall Prefab (cube) and set valid values for all fields.", MessageType.Warning);

            serializedObject.ApplyModifiedProperties();
        }

        private void GenerateLevel() {
            UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

            GameObject root = FindOrCreateRoot();

            float totalWidth    = levelSize.x;
            float totalLength   = levelSize.y;
            float corridorWidth = totalWidth / corridorCount;

            while (root.transform.childCount > 0) {
                GameObject child = root.transform.GetChild(0).gameObject;
                Undo.DestroyObjectImmediate(child);
            }

            // Spawn corridor walls (Length direction)
            for (int i = 0; i <= corridorCount; i++) {
                float   x        = -totalWidth / 2f + i * corridorWidth;
                Vector3 position = new Vector3(x, wallHeight / 2f, 0f);
                Vector3 scale    = new Vector3(0.2f, wallHeight, totalLength);
                CreateWallOrObstacle(root.transform, wallPrefab, position, scale, $"Wall_{i}");
            }

            // Spawn obstacles and blocking walls in each corridor
            for (int iCorridor = 0; iCorridor < corridorCount; iCorridor++) {
                float xStart  = -totalWidth / 2f + iCorridor * corridorWidth;
                float xEnd    = xStart + corridorWidth;
                float centerX = (xStart + xEnd) / 2f;

                // Generate HALF-BLOCKING walls
                for (int bw = 0; bw < blockingWallsPerCorridor; bw++) {
                    float z          = Random.Range(-totalLength / 2f + 2f, totalLength / 2f - 2f);
                    float blockWidth = corridorWidth * 0.95f * Random.Range(0.45f, 0.55f); // ~half width
                    bool  left       = Random.value > 0.5f;

                    float localX = left
                                    ? xStart + blockWidth / 2f
                                    : xEnd - blockWidth / 2f;

                    Vector3 bwPosition = new Vector3(localX, wallHeight / 2f, z);
                    Vector3 bwScale    = new Vector3(blockWidth, wallHeight, 0.3f);
                    CreateWallOrObstacle(root.transform, wallPrefab, bwPosition, bwScale, $"BlockingWall_{iCorridor}_{bw}");
                }

                // Generate small obstacles
                for (int j = 0; j < obstaclesPerCorridor; j++) {
                    float   obsX        = Random.Range(xStart + 0.6f, xEnd - 0.6f);
                    float   obsZ        = Random.Range(-totalLength / 2f + 1.2f, totalLength / 2f - 1.2f);
                    float   obsHeight   = Random.Range(0.2f, Mathf.Max(0.2f, maxObstacleHeight));
                    Vector3 obsPosition = new Vector3(obsX, obsHeight / 2f, obsZ);
                    Vector3 obsScale    = new Vector3(1f, obsHeight, 1f);

                    CreateWallOrObstacle(root.transform, wallPrefab, obsPosition, obsScale, $"Obstacle_{iCorridor}_{j}");
                }
            }

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        }

        private GameObject FindOrCreateRoot() {
            GameObject root = GameObject.Find(RootName);
            if (root == null) {
                root = new GameObject(RootName);
                Undo.RegisterCreatedObjectUndo(root, "Create Level Root");
            }
            return root;
        }

        private void CreateWallOrObstacle(Transform parent, GameObject prefab, Vector3 position, Vector3 scale, string name) {
            GameObject obj = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (obj != null) {
                Undo.RegisterCreatedObjectUndo(obj, "Instantiate Wall or Obstacle");
                obj.name = name;
                obj.transform.SetParent(parent);
                obj.transform.localPosition = position;
                obj.transform.localRotation = Quaternion.identity;
                obj.transform.localScale    = scale;
            }
        }
    }
}