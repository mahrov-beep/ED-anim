namespace AITools.AIEditorWindowTool {
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using UnityEditor;
  using UnityEngine;

  public class ErrorLogsCollector : AIEditorWindow {
    private List<LogEntry> errorLogs = new();
    private Vector2 scrollPosition;

    [MenuItem("GPTGenerated/" + nameof(ErrorLogsCollector))]
    public static void ShowWindow() {
      var window = GetWindow<ErrorLogsCollector>(nameof(ErrorLogsCollector));
    }

    protected override void OnEnable() {
      base.OnEnable();
      Application.logMessageReceived += OnLogReceived;
    }

    protected override void OnDisable() {
      base.OnDisable();
      Application.logMessageReceived -= OnLogReceived;
    }

    private void OnLogReceived(string condition, string stackTrace, LogType type) {
      if (type == LogType.Error || type == LogType.Exception) {
        errorLogs.Add(new LogEntry {
          message = condition,
          stackTrace = stackTrace,
          type = type
        });
      }
    }

    public override void OnGUI() {
      base.OnGUI();

      EditorGUILayout.Space(10);
      EditorGUILayout.LabelField($"Всего ошибок: {errorLogs.Count}", EditorStyles.boldLabel);
      EditorGUILayout.Space(5);

      if (errorLogs.Count == 0) {
        EditorGUILayout.HelpBox("Нет ошибок", MessageType.Info);
        return;
      }

      EditorGUILayout.BeginHorizontal();
      if (GUILayout.Button("Очистить список")) {
        errorLogs.Clear();
        Repaint();
      }
      EditorGUILayout.EndHorizontal();

      EditorGUILayout.Space(10);

      var lastLog = errorLogs.LastOrDefault();
      if (lastLog != null) {
        EditorGUILayout.LabelField("Последний лог:", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Скопировать последний лог")) {
          EditorGUIUtility.systemCopyBuffer = lastLog.message;
        }
        if (GUILayout.Button("Скопировать последний лог + стек трейс")) {
          EditorGUIUtility.systemCopyBuffer = $"{lastLog.message}\n\n{lastLog.stackTrace}";
        }
        EditorGUILayout.EndHorizontal();
      }

      EditorGUILayout.Space(5);

      var lastTwoLogs = errorLogs.Skip(Mathf.Max(0, errorLogs.Count - 2)).ToList();
      if (lastTwoLogs.Count >= 2) {
        EditorGUILayout.LabelField("Последние 2 лога:", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Скопировать последние 2 лога")) {
          var sb = new StringBuilder();
          for (int i = 0; i < lastTwoLogs.Count; i++) {
            sb.AppendLine($"=== Ошибка {i + 1} ===");
            sb.AppendLine(lastTwoLogs[i].message);
            sb.AppendLine();
          }
          EditorGUIUtility.systemCopyBuffer = sb.ToString();
        }
        if (GUILayout.Button("Скопировать последние 2 лога + стек трейс")) {
          var sb = new StringBuilder();
          for (int i = 0; i < lastTwoLogs.Count; i++) {
            sb.AppendLine($"=== Ошибка {i + 1} ===");
            sb.AppendLine(lastTwoLogs[i].message);
            sb.AppendLine();
            sb.AppendLine(lastTwoLogs[i].stackTrace);
            sb.AppendLine();
          }
          EditorGUIUtility.systemCopyBuffer = sb.ToString();
        }
        EditorGUILayout.EndHorizontal();
      }

      EditorGUILayout.Space(5);

      EditorGUILayout.LabelField("Все ошибки:", EditorStyles.boldLabel);
      EditorGUILayout.BeginHorizontal();
      if (GUILayout.Button("Скопировать все логи")) {
        var sb = new StringBuilder();
        for (int i = 0; i < errorLogs.Count; i++) {
          sb.AppendLine($"=== Ошибка {i + 1} ===");
          sb.AppendLine(errorLogs[i].message);
          sb.AppendLine();
        }
        EditorGUIUtility.systemCopyBuffer = sb.ToString();
      }
      if (GUILayout.Button("Скопировать все логи + стек трейс")) {
        var sb = new StringBuilder();
        for (int i = 0; i < errorLogs.Count; i++) {
          sb.AppendLine($"=== Ошибка {i + 1} ===");
          sb.AppendLine(errorLogs[i].message);
          sb.AppendLine();
          sb.AppendLine(errorLogs[i].stackTrace);
          sb.AppendLine();
        }
        EditorGUIUtility.systemCopyBuffer = sb.ToString();
      }
      EditorGUILayout.EndHorizontal();

      EditorGUILayout.Space(10);
      EditorGUILayout.LabelField("Список ошибок:", EditorStyles.boldLabel);

      scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
      int indexToRemove = -1;
      for (int i = errorLogs.Count - 1; i >= 0; i--) {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"[{errorLogs.Count - i}] {errorLogs[i].type}", EditorStyles.miniBoldLabel);
        
        GUILayout.FlexibleSpace();
        
        if (GUILayout.Button("Copy", GUILayout.Width(50))) {
          EditorGUIUtility.systemCopyBuffer = errorLogs[i].message;
        }
        if (GUILayout.Button("Copy+Stack", GUILayout.Width(80))) {
          EditorGUIUtility.systemCopyBuffer = $"{errorLogs[i].message}\n\n{errorLogs[i].stackTrace}";
        }
        if (GUILayout.Button("✕", GUILayout.Width(25))) {
          indexToRemove = i;
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.TextArea(errorLogs[i].message, EditorStyles.wordWrappedLabel);
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(2);
      }
      EditorGUILayout.EndScrollView();

      if (indexToRemove >= 0) {
        errorLogs.RemoveAt(indexToRemove);
        Repaint();
      }
    }

    private class LogEntry {
      public string message;
      public string stackTrace;
      public LogType type;
    }
  }
}