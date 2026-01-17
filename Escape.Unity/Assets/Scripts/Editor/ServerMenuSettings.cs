using UnityEditor;
using UnityEngine;

public class ServerMenuSettings : EditorWindow {

    /// <summary>
    /// Must be equals to docker-compose.yml
    /// </summary>
    const string DEFAULT_POSTGRES_CONNECTION_STRING = "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=postgres";

    public static void Open() {
        var window = GetWindow<ServerMenuSettings>();
        window.titleContent = new GUIContent("Server Settings");
        window.Show();
    }

    private static string GetPrefsKey(string key) => $"Server.{Application.dataPath}.{key}";

    public static string PostgresConnection {
        get => EditorPrefs.GetString(GetPrefsKey("PostgresConnection"), DEFAULT_POSTGRES_CONNECTION_STRING);
        set => EditorPrefs.SetString(GetPrefsKey("PostgresConnection"), value);
    }

    public static bool DisableLoadoutLost {
        get => EditorPrefs.GetBool(GetPrefsKey("DisableLoadoutLost"), false);
        set => EditorPrefs.SetBool(GetPrefsKey("DisableLoadoutLost"), value);
    }

    public static bool EnableDockerPanel {
        get => EditorPrefs.GetBool(GetPrefsKey("EnableDockerPanel"), false);
        set => EditorPrefs.SetBool(GetPrefsKey("EnableDockerPanel"), value);
    }

    public static string VersionServiceUrl {
        get => EditorPrefs.GetString(GetPrefsKey("VersionServiceUrl"), "http://46.224.69.135:5200");
        set => EditorPrefs.SetString(GetPrefsKey("VersionServiceUrl"), value);
    }

    private void OnGUI() {
        EnableDockerPanel = EditorGUILayout.Toggle("Enable Docker Panel", EnableDockerPanel);
        GUILayout.Space(10);

        GUILayout.Label("Postgres Connection", EditorStyles.miniBoldLabel);
        PostgresConnection = EditorGUILayout.TextArea(PostgresConnection).Trim();

        GUILayout.Space(10);
        EditorGUILayout.HelpBox("Server rebuild required for changes to take effect", MessageType.Info);
        DisableLoadoutLost = EditorGUILayout.Toggle("Disable Loadout Lost", DisableLoadoutLost);

        GUILayout.Space(10);
        GUILayout.Label("Version Service", EditorStyles.miniBoldLabel);
        VersionServiceUrl = EditorGUILayout.TextField("URL", VersionServiceUrl);
    }
}