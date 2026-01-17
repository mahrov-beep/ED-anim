using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AITools.AIEditorWindowTool {
  public class AIEditorWindowCreator : AIEditorWindow {
    Dictionary<Type, string> renameDictionary = new Dictionary<Type, string>();
    string fileName = "NewAIWindow";
    Vector2 scrollPos;
    Vector2 scrollPosMain;
    bool IamDeveloper;
    HashSet<Type> cacheSubclasses;

    [MenuItem("GPTGenerated/" + nameof(AIEditorWindowCreator))]
    public static void ShowWindow() {
      var window = GetWindow<AIEditorWindowCreator>(nameof(AIEditorWindowCreator));
      window.Show();
    }

    protected override void OnEnable() {
      Reflection_UpdateSubclassesList();
      base.OnEnable();
    }

    protected override void OnDisable() {
      base.OnDisable();
    }

    public override void OnGUI() {
      base.OnGUI();
      IamDeveloper = EditorGUILayout.Toggle("Developer settings", IamDeveloper);
      if (!IamDeveloper) _showGPTPanel = false;

      scrollPosMain = EditorGUILayout.BeginScrollView(scrollPosMain, GUILayout.ExpandHeight(true));

      GUILayout.Label("Создание AIEditorWindow", EditorStyles.boldLabel);
      fileName = EditorGUILayout.TextField("Имя файла", fileName);

      if (GUILayout.Button("создать файл")) {
        CreateScript(fileName);
      }

      EditorGUILayout.Space();

      GUILayout.Label("Наследники AIEditorWindow:", EditorStyles.boldLabel);
      if (GUILayout.Button("Обновить список наследников")) {
        Reflection_UpdateSubclassesList();
      }

      DrawAIEditorWindowSubclassesList();

      EditorGUILayout.EndScrollView();
    }

    void CreateScript(string name) {
      if (string.IsNullOrEmpty(name)) {
        Debug.LogError("Введите корректное имя файла.");
        return;
      }
      var scriptFolderPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this)));
      if (string.IsNullOrEmpty(scriptFolderPath)) {
        Debug.LogError("Не удалось определить путь к скрипту AIEditorWindowCreator.");
        return;
      }
      string scriptContent =
        @"namespace AITools.AIEditorWindowTool {
  using UnityEditor;
  using UnityEngine;
  public class " + name + @" : AIEditorWindow {

    [MenuItem(""GPTGenerated/"" + nameof(" + name + @"))]
    public static void ShowWindow() {
      var window = GetWindow<" + name + @">(nameof(" + name + @"));
    }
  }
}";
      var path = Path.Combine(scriptFolderPath, name + ".cs");
      File.WriteAllText(path, scriptContent);
      var promptPath = Path.Combine(scriptFolderPath, name + ".cs.prompt");
      File.WriteAllText(promptPath, "");
      AssetDatabase.Refresh();
    }

    void Reflection_UpdateSubclassesList() {
      cacheSubclasses = new HashSet<Type>();
      var assemblies = AppDomain.CurrentDomain.GetAssemblies();
      foreach (var assembly in assemblies) {
        foreach (var type in assembly.GetTypes()) {
          if (type.IsSubclassOf(typeof(AIEditorWindow)) && type != typeof(AIEditorWindowCreator)) {
            cacheSubclasses.Add(type);
          }
        }
      }
      foreach (var t in cacheSubclasses) {
        if (!renameDictionary.ContainsKey(t)) renameDictionary[t] = t.Name;
      }
      var toRemove = new List<Type>();
      foreach (var kv in renameDictionary) {
        if (!cacheSubclasses.Contains(kv.Key)) toRemove.Add(kv.Key);
      }
      foreach (var rem in toRemove) {
        renameDictionary.Remove(rem);
      }
    }

    void DrawAIEditorWindowSubclassesList() {
      if (cacheSubclasses == null) return;
      bool changed = false;
      scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandHeight(true));
      foreach (var type in cacheSubclasses) {
        EditorGUILayout.BeginHorizontal();
        var oldName = renameDictionary[type];
        EditorGUI.BeginChangeCheck();
        var newName = EditorGUILayout.TextField(oldName);
        if (EditorGUI.EndChangeCheck()) {
          renameDictionary[type] = newName;
          RenameScript(type, oldName, newName);
          changed = true;
          EditorGUILayout.EndHorizontal();
          break;
        }
        if (GUILayout.Button("Open", EditorStyles.miniButton)) {
          GetWindow(type).Show();
        }
        if (GUILayout.Button("copy", EditorStyles.miniButton)) {
          CopyScript(type);
          changed = true;
          EditorGUILayout.EndHorizontal();
          break;
        }
        if (GUILayout.Button(EditorGUIUtility.IconContent("TreeEditor.Trash"), GUILayout.Width(25))) {
          DeleteConfirmation(type);
          changed = true;
          EditorGUILayout.EndHorizontal();
          break;
        }
        EditorGUILayout.EndHorizontal();
        if (changed) break;
      }
      EditorGUILayout.EndScrollView();
      if (changed) {
        Reflection_UpdateSubclassesList();
        return;
      }
    }

    void CopyScript(Type type) {
      var scriptFolderPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this)));
      if (string.IsNullOrEmpty(scriptFolderPath)) {
        Debug.LogError("Не удалось определить путь к скрипту AIEditorWindowCreator.");
        return;
      }
      var oldName = type.Name;
      var newName = oldName + "Copy";
      var oldPath = Path.Combine(scriptFolderPath, oldName + ".cs");
      if (!File.Exists(oldPath)) {
        Debug.LogError("Исходный файл для копирования не найден.");
        return;
      }
      var content = File.ReadAllText(oldPath);
      content = content.Replace("public class " + oldName, "public class " + newName);
      content = content.Replace("nameof(" + oldName + ")", "nameof(" + newName + ")");
      var newPath = Path.Combine(scriptFolderPath, newName + ".cs");
      File.WriteAllText(newPath, content);
      var promptPath = Path.Combine(scriptFolderPath, newName + ".cs.prompt");
      File.WriteAllText(promptPath, "");
      AssetDatabase.Refresh();
    }

    void RenameScript(Type type, string oldName, string newName) {
      if (string.IsNullOrEmpty(newName)) {
        Debug.LogError("Новое имя не может быть пустым.");
        renameDictionary[type] = oldName;
        return;
      }
      var scriptFolderPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this)));
      if (string.IsNullOrEmpty(scriptFolderPath)) {
        Debug.LogError("Не удалось определить путь к скрипту AIEditorWindowCreator.");
        renameDictionary[type] = oldName;
        return;
      }
      var oldPath = Path.Combine(scriptFolderPath, oldName + ".cs");
      var oldPromptPath = Path.Combine(scriptFolderPath, oldName + ".cs.prompt");
      if (!File.Exists(oldPath)) {
        Debug.LogError("Исходный файл для переименования не найден.");
        renameDictionary[type] = oldName;
        return;
      }
      var content = File.ReadAllText(oldPath);
      content = content.Replace("public class " + oldName, "public class " + newName);
      content = content.Replace("nameof(" + oldName + ")", "nameof(" + newName + ")");
      var newPath = Path.Combine(scriptFolderPath, newName + ".cs");
      File.WriteAllText(newPath, content);
      if (File.Exists(oldPromptPath)) {
        var newPromptPath = Path.Combine(scriptFolderPath, newName + ".cs.prompt");
        AssetDatabase.DeleteAsset(oldPromptPath);
        File.WriteAllText(newPromptPath, "");
      }
      AssetDatabase.DeleteAsset(oldPath);
      AssetDatabase.Refresh();
    }

    void DeleteConfirmation(Type type) {
      if (EditorUtility.DisplayDialog("Удаление файла",
        $"Вы действительно хотите удалить файл {type.Name}.cs ?",
        "Удалить", "Отмена")) {
        DeleteScript(type);
      }
    }

    void DeleteScript(Type type) {
      var scriptFolderPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this)));
      if (string.IsNullOrEmpty(scriptFolderPath)) {
        Debug.LogError("Не удалось определить путь к скрипту AIEditorWindowCreator.");
        return;
      }
      var scriptPath = Path.Combine(scriptFolderPath, type.Name + ".cs");
      var promptPath = Path.Combine(scriptFolderPath, type.Name + ".cs.prompt");
      if (File.Exists(scriptPath)) AssetDatabase.DeleteAsset(scriptPath);
      if (File.Exists(promptPath)) AssetDatabase.DeleteAsset(promptPath);
      AssetDatabase.Refresh();
      cacheSubclasses.Remove(type);
    }
  }
}