using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

namespace AITools.AIEditorWindowTool
{
    public class SplineHelper : AIEditorWindow
    {
        private Vector2 scrollPos;
        private List<GameObject> splineObjects = new List<GameObject>();

        [MenuItem("GPTGenerated/" + nameof(SplineHelper))]
        public static void ShowWindow()
        {
            GetWindow<SplineHelper>("SplineHelper");
        }

        public override void OnGUI()
        {
            base.OnGUI();

            if (GUILayout.Button("Find All Splines In Scene"))
                FindAllSplines();

            GUILayout.Label($"Splines found: {splineObjects.Count}", EditorStyles.boldLabel);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            foreach (var splineObj in splineObjects)
            {
                if (GUILayout.Button(splineObj.name))
                {
                    Selection.activeGameObject = splineObj;
                    EditorGUIUtility.PingObject(splineObj);
                }
            }
            EditorGUILayout.EndScrollView();
        }

        private void FindAllSplines()
        {
            splineObjects.Clear();
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.isLoaded)
                return;

            GameObject[] allObjects = scene.GetRootGameObjects();
            foreach (var obj in allObjects)
                splineObjects.AddRange(obj.GetComponentsInChildren<Component>(true)
                    .Where(c => c != null && c.GetType().Name.Contains("Spline"))
                    .Select(c => c.gameObject));
            
            splineObjects = splineObjects.Distinct().ToList();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }
    }
}