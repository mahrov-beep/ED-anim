using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class SystemMetricsWindow : EditorWindow
{
    static string ExportPath => Path.GetFullPath(Path.Combine(Application.dataPath, "../../system_metrics.csv"));

    [MenuItem("Window/System Metrics")]
    static void Open() => GetWindow<SystemMetricsWindow>("System Metrics");

    void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Reset"))
            SystemMetrics.Reset();
        if (GUILayout.Button("Export CSV"))
            ExportToCsv();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("System", EditorStyles.boldLabel, GUILayout.Width(200));
        EditorGUILayout.LabelField("Avg", EditorStyles.boldLabel, GUILayout.Width(80));
        EditorGUILayout.LabelField("Last", EditorStyles.boldLabel, GUILayout.Width(80));
        EditorGUILayout.LabelField("Min", EditorStyles.boldLabel, GUILayout.Width(80));
        EditorGUILayout.LabelField("Max", EditorStyles.boldLabel, GUILayout.Width(80));
        EditorGUILayout.LabelField("Count", EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();

        foreach (var kvp in SystemMetrics.All.OrderBy(x => x.Key))
        {
            var d = kvp.Value;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(kvp.Key, GUILayout.Width(200));
            EditorGUILayout.LabelField($"{d.AverageMs:F3}", GUILayout.Width(80));
            EditorGUILayout.LabelField($"{d.LastMs:F3}", GUILayout.Width(80));
            EditorGUILayout.LabelField($"{d.MinMs:F3}", GUILayout.Width(80));
            EditorGUILayout.LabelField($"{d.MaxMs:F3}", GUILayout.Width(80));
            EditorGUILayout.LabelField($"{d.UpdateCount}");
            EditorGUILayout.EndHorizontal();
        }

        Repaint();
    }

    void ExportToCsv()
    {
        using var writer = new StreamWriter(ExportPath);
        writer.WriteLine("System,Avg_ms,Last_ms,Min_ms,Max_ms,Count");

        foreach (var kvp in SystemMetrics.All.OrderBy(x => x.Key))
        {
            var d = kvp.Value;
            writer.WriteLine($"{kvp.Key},{d.AverageMs:F3},{d.LastMs:F3},{d.MinMs:F3},{d.MaxMs:F3},{d.UpdateCount}");
        }

        Debug.Log($"Exported to {ExportPath}");
    }
}
