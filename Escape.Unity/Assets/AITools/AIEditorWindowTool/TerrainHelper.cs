using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Quantum;

namespace AITools.AIEditorWindowTool {
    public class TerrainHelper : AIEditorWindow {

        private Terrain selectedTerrain;
        private TerrainData terrainData;
        private QuantumStaticTerrainCollider3D quantumCollider;
        private Vector2 scrollPos;
        private int selectedResolutionIndex;

        private static readonly int[] validResolutions = new int[] { 33, 65, 129, 257, 513, 1025, 2049, 4097 };

        [MenuItem("GPTGenerated/" + nameof(TerrainHelper))]
        public static void ShowWindow() {
            GetWindow<TerrainHelper>(nameof(TerrainHelper));
        }

        protected override void OnEnable() {
            base.OnEnable();
            Selection.selectionChanged += UpdateSelection;
            UpdateSelection();
        }

        protected override void OnDisable() {
            base.OnDisable();
            Selection.selectionChanged -= UpdateSelection;
        }

        private void UpdateSelection() {
            selectedTerrain = Selection.activeGameObject != null ? Selection.activeGameObject.GetComponent<Terrain>() : null;
            terrainData = selectedTerrain != null ? selectedTerrain.terrainData : null;
            quantumCollider = selectedTerrain != null ? selectedTerrain.GetComponent<QuantumStaticTerrainCollider3D>() : null;

            if (terrainData != null) {
                selectedResolutionIndex = Mathf.Max(0, System.Array.IndexOf(validResolutions, terrainData.heightmapResolution));
            }

            Repaint();
        }

        public override void OnGUI() {
            base.OnGUI();

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            GUILayout.Label("Настройки разрешения карты высот Terrain", EditorStyles.boldLabel);

            if (terrainData == null) {
                EditorGUILayout.HelpBox("Выберите объект с компонентом Terrain.", MessageType.Warning);
            } else {
                int newSelectedResolutionIndex = EditorGUILayout.Popup("Разрешение карты высот", selectedResolutionIndex, GetResolutionOptions());
                if (newSelectedResolutionIndex != selectedResolutionIndex) {
                    selectedResolutionIndex = newSelectedResolutionIndex;
                }

                int newHeightmapResolution = validResolutions[selectedResolutionIndex];

                GUI.enabled = newHeightmapResolution != terrainData.heightmapResolution;
                if (GUILayout.Button("Применить новое разрешение карты высот", GUILayout.Height(30))) {
                    if (EditorUtility.DisplayDialog("Подтверждение изменения",
                        $"Вы действительно хотите поменять разрешение на {newHeightmapResolution}? Данные террейна могут быть изменены.",
                        "Применить", "Отмена")) {

                        Undo.RegisterCompleteObjectUndo(terrainData, "Изменение разрешения карты высот");
                        Vector3 size = terrainData.size;
                        terrainData.heightmapResolution = newHeightmapResolution;
                        terrainData.size = size;
                        EditorUtility.SetDirty(terrainData);
                        UpdateSelection();
                    }
                }
                GUI.enabled = true;

                GUILayout.Space(10);
                GUILayout.Label("Информация о Terrain", EditorStyles.boldLabel);

                EditorGUILayout.LabelField("Terrain имя", selectedTerrain.name);
                EditorGUILayout.Vector3Field("Положение террейна", selectedTerrain.transform.position);
                EditorGUILayout.Vector3Field("Размер террейна", terrainData.size);
                EditorGUILayout.LabelField("Разрешение карты высот", terrainData.heightmapResolution.ToString());
                EditorGUILayout.LabelField("Количество вершин", (terrainData.heightmapResolution * terrainData.heightmapResolution).ToString());

                EditorGUILayout.LabelField("Количество слоёв Alphamap", terrainData.alphamapLayers.ToString());
                EditorGUILayout.LabelField("Alphamap Resolution", terrainData.alphamapResolution.ToString());
                EditorGUILayout.LabelField("Base Map Resolution", terrainData.baseMapResolution.ToString());

                EditorGUILayout.LabelField("Количество детализированных слоёв", terrainData.detailPrototypes.Length.ToString());
                EditorGUILayout.LabelField("Detail Resolution", terrainData.detailResolution.ToString());

                EditorGUILayout.LabelField("Количество типов деревьев", terrainData.treePrototypes.Length.ToString());
                EditorGUILayout.LabelField("Количество экземпляров деревьев", terrainData.treeInstances.Length.ToString());
                EditorGUILayout.ObjectField("Материал террейна", selectedTerrain.materialTemplate, typeof(Material), false);
                EditorGUILayout.LabelField("Максимальная высота", terrainData.size.y.ToString());
                EditorGUILayout.LabelField("Waving Grass Strength", terrainData.wavingGrassStrength.ToString());
                EditorGUILayout.LabelField("Waving Grass Amount", terrainData.wavingGrassAmount.ToString());
                EditorGUILayout.LabelField("Waving Grass Tint", terrainData.wavingGrassTint.ToString());

#if UNITY_2019_3_OR_NEWER
                EditorGUILayout.LabelField("Поддержка Terrain Holes", terrainData.IsHole(0, 0).ToString());
#endif

                GUILayout.Space(10);
                GUILayout.Label("QuantumStaticTerrainCollider3D", EditorStyles.boldLabel);
                if (quantumCollider != null && quantumCollider.Asset != null) {
                    EditorGUILayout.LabelField("Resolution", quantumCollider.Asset.Resolution.ToString());
                    EditorGUILayout.LabelField("Высота карты длина", quantumCollider.Asset.HeightMap?.Length.ToString() ?? "Пусто");
                    EditorGUILayout.Vector3Field("Quantum позиция террейна", quantumCollider.Asset.Position.ToUnityVector3());
                    EditorGUILayout.Vector3Field("Quantum масштаб террейна", quantumCollider.Asset.Scale.ToUnityVector3());
                    EditorGUILayout.LabelField("Размер HoleMask", quantumCollider.Asset.HoleMask?.Length.ToString() ?? "Пусто");
                    EditorGUILayout.LabelField("SmoothSphereMeshCollisions", quantumCollider.SmoothSphereMeshCollisions.ToString());
                    EditorGUILayout.LabelField("Mutable Mode", quantumCollider.Settings.MutableMode.ToString());
                } else {
                    EditorGUILayout.HelpBox("QuantumStaticTerrainCollider3D или его Asset отсутствует.", MessageType.Warning);
                }

                GUILayout.Space(10);
                GUILayout.Label("Данные доступные в Runtime:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("• Размер Terrain (размеры)");
                EditorGUILayout.LabelField("• Разрешение высотной карты");
                EditorGUILayout.LabelField("• Масштаб высотной карты");
                EditorGUILayout.LabelField("• Количество слоёв Alphamap");
                EditorGUILayout.LabelField("• Разрешение Alphamap");
                EditorGUILayout.LabelField("• Количество деталей и деревьев");
                EditorGUILayout.LabelField("• Базовое разрешение карты");
                EditorGUILayout.LabelField("• Материал Terrain");
                EditorGUILayout.LabelField("• Позиция и максимальная высота Terrain");
            }

            EditorGUILayout.EndScrollView();
        }

        private string[] GetResolutionOptions() {
            List<string> labels = new List<string>(validResolutions.Length);
            foreach (int res in validResolutions) {
                labels.Add($"{res} x {res}");
            }
            return labels.ToArray();
        }
    }
}