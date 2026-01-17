#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Game.ECS.Scripts.GameView;
using _Project.Scripts.GameView;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Editor утилита для быстрой настройки разведывательной гранаты
/// </summary>
public class ReconGrenadeSetupEditor : EditorWindow {
    private Material outlineMaterial;
    private bool includeInactive = true;
    private bool searchInPrefabs = false;
    private Vector2 scrollPosition;
    private List<GameObject> foundCharacters = new List<GameObject>();
    private bool autoRefresh = true;

    [MenuItem("Tools/Recon Grenade/Setup Wizard")]
    public static void ShowWindow() {
        var window = GetWindow<ReconGrenadeSetupEditor>("Recon Setup");
        window.minSize = new Vector2(400, 500);
        window.Show();
    }

    private void OnEnable() {
        RefreshCharacterList();
    }

    private void OnGUI() {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Recon Grenade Setup", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Этот инструмент помогает настроить компоненты ReconOutline на всех персонажах в сцене.",
            MessageType.Info
        );

        EditorGUILayout.Space(10);

        // Настройки
        EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
        outlineMaterial = (Material)EditorGUILayout.ObjectField(
            "Outline Material", 
            outlineMaterial, 
            typeof(Material), 
            false
        );

        includeInactive = EditorGUILayout.Toggle("Include Inactive", includeInactive);
        searchInPrefabs = EditorGUILayout.Toggle("Search in Prefabs", searchInPrefabs);
        autoRefresh = EditorGUILayout.Toggle("Auto Refresh List", autoRefresh);

        if (autoRefresh && Event.current.type == EventType.Layout) {
            RefreshCharacterList();
        }

        EditorGUILayout.Space(10);

        // Кнопки действий
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Refresh Character List", GUILayout.Height(30))) {
            RefreshCharacterList();
        }
        if (GUILayout.Button("Create Outline Material", GUILayout.Height(30))) {
            CreateOutlineMaterial();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);

        // Информация
        EditorGUILayout.LabelField($"Found Characters: {foundCharacters.Count}", EditorStyles.boldLabel);

        if (foundCharacters.Count == 0) {
            EditorGUILayout.HelpBox(
                searchInPrefabs 
                    ? "No character prefabs found. Make sure you have character prefabs in Assets folder."
                    : "No characters found in scene. Try enabling 'Search in Prefabs' or add characters to scene.",
                MessageType.Warning
            );
            return;
        }

        EditorGUILayout.Space(5);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        foreach (var character in foundCharacters) {
            if (character == null) continue;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.LabelField(character.name, GUILayout.Width(200));
            
            var outline = character.GetComponent<ReconOutline>();
            bool hasOutline = outline != null;
            
            GUI.color = hasOutline ? Color.green : Color.yellow;
            EditorGUILayout.LabelField(
                hasOutline ? "✓ Has Outline" : "✗ No Outline",
                GUILayout.Width(100)
            );
            GUI.color = Color.white;

            if (hasOutline) {
                if (GUILayout.Button("Remove", GUILayout.Width(70))) {
                    RemoveOutlineComponent(character);
                }
                if (GUILayout.Button("Update", GUILayout.Width(70))) {
                    UpdateOutlineComponent(character);
                }
            } else {
                if (GUILayout.Button("Add", GUILayout.Width(70))) {
                    AddOutlineComponent(character);
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }
        
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space(10);
       
        EditorGUILayout.LabelField("Batch Actions", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        
        GUI.enabled = outlineMaterial != null;
        if (GUILayout.Button("Add Outline to All", GUILayout.Height(35))) {
            AddOutlineToAll();
        }
        if (GUILayout.Button("Update All Outlines", GUILayout.Height(35))) {
            UpdateAllOutlines();
        }
        GUI.enabled = true;

        if (GUILayout.Button("Remove All Outlines", GUILayout.Height(35))) {
            if (EditorUtility.DisplayDialog(
                "Remove All Outlines",
                "Are you sure you want to remove ReconOutline from all characters?",
                "Yes", "No"
            )) {
                RemoveAllOutlines();
            }
        }
        
        EditorGUILayout.EndHorizontal();

        if (outlineMaterial == null) {
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox(
                "Please assign Outline Material or create a new one using 'Create Outline Material' button.",
                MessageType.Warning
            );
        }
    }

    private void RefreshCharacterList() {
        foundCharacters.Clear();
        
        if (searchInPrefabs) {            
            string[] guids = AssetDatabase.FindAssets("t:Prefab");
            foreach (string guid in guids) {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                
                if (prefab != null && HasCharacterComponents(prefab)) {
                    foundCharacters.Add(prefab);
                }
            }
        } else {            
            var allObjects = includeInactive 
                ? FindObjectsOfType<GameObject>(true)
                : FindObjectsOfType<GameObject>();
            
            foreach (var obj in allObjects) {
                if (HasCharacterComponents(obj)) {
                    foundCharacters.Add(obj);
                }
            }
        }
        
        foundCharacters = foundCharacters.OrderBy(c => c.name).ToList();
    }
    
    private bool HasCharacterComponents(GameObject obj) {        
        return obj.GetComponent<CharacterView>() != null ||
               obj.GetComponentInChildren<SkinnedMeshRenderer>() != null;
    }

    private void AddOutlineComponent(GameObject character) {
        if (character == null) return;
        
        var outline = character.GetComponent<ReconOutline>();
        if (outline == null) {
            outline = Undo.AddComponent<ReconOutline>(character);
        }

        UpdateOutlineMaterial(outline);
        
        EditorUtility.SetDirty(character);
        if (searchInPrefabs) {
            PrefabUtility.SavePrefabAsset(character);
        }
        Debug.Log($"Added ReconOutline to {character.name}");
    }

    private void RemoveOutlineComponent(GameObject character) {
        if (character == null) return;
        
        var outline = character.GetComponent<ReconOutline>();
        if (outline != null) {
            Undo.DestroyObjectImmediate(outline);
            EditorUtility.SetDirty(character);
            if (searchInPrefabs) {
                PrefabUtility.SavePrefabAsset(character);
            }
            Debug.Log($"Removed ReconOutline from {character.name}");
        }
    }

    private void UpdateOutlineComponent(GameObject character) {
        if (character == null) return;
        
        var outline = character.GetComponent<ReconOutline>();
        if (outline != null) {
            UpdateOutlineMaterial(outline);
            EditorUtility.SetDirty(character);
            if (searchInPrefabs) {
                PrefabUtility.SavePrefabAsset(character);
            }
        }
    }

    private void UpdateOutlineMaterial(ReconOutline outline) {
        if (outline == null || outlineMaterial == null) return;
        
        var so = new SerializedObject(outline);
        var prop = so.FindProperty("outlineMaterial");
        prop.objectReferenceValue = outlineMaterial;
        so.ApplyModifiedProperties();
    }

    private void AddOutlineToAll() {
        if (outlineMaterial == null) {
            EditorUtility.DisplayDialog(
                "Material Missing",
                "Please assign Outline Material first.",
                "OK"
            );
            return;
        }

        int added = 0;
        foreach (var character in foundCharacters) {
            if (character == null) continue;
            if (character.GetComponent<ReconOutline>() == null) {
                AddOutlineComponent(character);
                added++;
            }
        }

        Debug.Log($"Added ReconOutline to {added} characters");
        EditorUtility.DisplayDialog(
            "Complete",
            $"Added ReconOutline to {added} characters",
            "OK"
        );
    }

    private void UpdateAllOutlines() {
        if (outlineMaterial == null) {
            EditorUtility.DisplayDialog(
                "Material Missing",
                "Please assign Outline Material first.",
                "OK"
            );
            return;
        }

        int updated = 0;
        foreach (var character in foundCharacters) {
            if (character == null) continue;
            var outline = character.GetComponent<ReconOutline>();
            if (outline != null) {
                UpdateOutlineMaterial(outline);
                updated++;
            }
        }

        Debug.Log($"Updated {updated} ReconOutline components");
        EditorUtility.DisplayDialog(
            "Complete",
            $"Updated {updated} ReconOutline components",
            "OK"
        );
    }

    private void RemoveAllOutlines() {
        int removed = 0;
        foreach (var character in foundCharacters) {
            if (character == null) continue;
            var outline = character.GetComponent<ReconOutline>();
            if (outline != null) {
                RemoveOutlineComponent(character);
                removed++;
            }
        }

        Debug.Log($"Removed {removed} ReconOutline components");
        RefreshCharacterList();
    }

    private void CreateOutlineMaterial() {
        string path = EditorUtility.SaveFilePanelInProject(
            "Create Outline Material",
            "ReconOutlineMaterial",
            "mat",
            "Choose location for new material"
        );

        if (string.IsNullOrEmpty(path)) return;

        var shader = Shader.Find("Custom/ReconOutline");
        if (shader == null) {
            EditorUtility.DisplayDialog(
                "Shader Not Found",
                "Shader 'Custom/ReconOutline' not found. Make sure ReconOutline.shader is in the project.",
                "OK"
            );
            return;
        }

        Material material = new Material(shader);
        material.SetColor("_OutlineColor", new Color(0, 1, 0, 1)); // Green
        material.SetFloat("_OutlineWidth", 0.008f);
        material.SetColor("_EmissionColor", new Color(0, 1, 0, 1)); // Green emission

        AssetDatabase.CreateAsset(material, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        outlineMaterial = material;
        EditorGUIUtility.PingObject(material);

        Debug.Log($"Created ReconOutline material at {path}");
        EditorUtility.DisplayDialog(
            "Material Created",
            $"Material created at:\n{path}\n\nIt has been automatically assigned.",
            "OK"
        );
    }
}
#endif

