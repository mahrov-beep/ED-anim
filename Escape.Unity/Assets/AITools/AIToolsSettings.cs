namespace AITools {
  using UnityEditor;

  [FilePath("UserSettings/" + nameof(AIToolsSettings) + ".asset",
          FilePathAttribute.Location.ProjectFolder)]
  public sealed class AIToolsSettings : ScriptableSingleton<AIToolsSettings> {
    public string openAIAPIKey;
    public int timeout;
    public void Save() => Save(true);
    void OnDisable() => Save();
  }

  sealed class AIToolsSettingsProvider : SettingsProvider {
    AIToolsSettingsProvider() : base(
            "Project/AI Tools",
            SettingsScope.Project) { }

    public override void OnGUI(string search) {
      var settings = AIToolsSettings.instance;

      var key = settings.openAIAPIKey;
      var timeout = settings.timeout;

      EditorGUI.BeginChangeCheck();

      key = EditorGUILayout.TextField("OpenAI API Key", key);
      timeout = EditorGUILayout.IntField("Timeout", timeout);

      if (EditorGUI.EndChangeCheck()) {
        settings.openAIAPIKey = key;
        settings.timeout = timeout;
        settings.Save();
      }
    }

    [SettingsProvider]
    public static SettingsProvider CreateCustomSettingsProvider()
      => new AIToolsSettingsProvider();
  }

}
