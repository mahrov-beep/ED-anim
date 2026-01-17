namespace AITools.AIEditorWindowTool
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using UnityEditor;
    using UnityEngine;
    using OpenAI;
    using Object = UnityEngine.Object;

    public class ScriptGPTOverwriter : AIEditorWindow
    {
        [SerializeField] private MonoScript targetScript;
        [SerializeField] private string userInstructions = "";
        private Vector2 scroll;
        private bool isOverwriting;
        private EModels selectedModel = EModels.GPT4o;

        // ВАЖНО: НЕ ИСПОЛЬЗУЙТЕ НА ПРОДЕ. ОПАСНО!
        private readonly string dangerWarning =
            "ВНИМАНИЕ: ЭТО ОПАСНАЯ И ЭКСПЕРИМЕНТАЛЬНАЯ ТУЛЗА!\nВСЕ ИЗМЕНЕНИЯ ПРОИСХОДЯТ НЕОБРАТИМО! РЕЗЕРВНОЕ КОПИРОВАНИЕ ОБЯЗАТЕЛЬНО!";

        // Инструкции для ChatGPT (системные и пользовательские, не отображать в инспекторе)
        private readonly string systemInstruction =
            "- Ты выступаешь как ChatGPT, редактируешь Unity C# скрипты через EditorWindow по заданию пользователя. Внимательно следуй инструкциям.\n" +
            "- Не изменяй контекстные/несвязанные файлы, если не указано явно.\n" +
            "- Отвечай только полным содержимым изменённого скрипта (файл для перезаписи).\n" +
            "- Запрещено давать объяснения, комментарии или лишний текст.\n" +
            "- Код должен быть полностью готов к компиляции.\n";

        private readonly string userInstructionPrefix =
            "Вот что нужно изменить в файле:\n";

        [MenuItem("GPTGenerated/" + nameof(ScriptGPTOverwriter))]
        public static void ShowWindow()
        {
            var wnd = GetWindow<ScriptGPTOverwriter>();
            wnd.titleContent = new GUIContent("ScriptGPTOverwriter");
            wnd.Show();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        public override void OnGUI()
        {
            base.OnGUI();

            // Опасное предупреждение
            GUILayout.Space(6);
            var warningStyle = new GUIStyle(EditorStyles.label);
            warningStyle.wordWrap = true;
            warningStyle.alignment = TextAnchor.MiddleCenter;
            warningStyle.fontSize = 16;
            warningStyle.fontStyle = FontStyle.Bold;
            warningStyle.normal.textColor = Color.red;
            GUI.backgroundColor = Color.red;
            GUILayout.BeginVertical("box");
            GUILayout.Label(dangerWarning, warningStyle, GUILayout.ExpandWidth(true), GUILayout.Height(50));
            GUILayout.EndVertical();
            GUI.backgroundColor = Color.white;
            EditorGUILayout.Space(8);

            EditorGUILayout.LabelField("ScriptGPTOverwriter", EditorStyles.boldLabel);
            EditorGUILayout.Space(8);

            // Выбор MonoScript как отдельного поля, как в AIEditorWindow без списка
            EditorGUI.BeginDisabledGroup(isOverwriting);
            EditorGUILayout.LabelField("Целевой MonoScript для перезаписи:");
            targetScript = (MonoScript)EditorGUILayout.ObjectField(targetScript, typeof(MonoScript), false);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(6);

            EditorGUI.BeginDisabledGroup(isOverwriting);
            selectedModel = (EModels)EditorGUILayout.EnumPopup("OpenAI Model", selectedModel);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(8);

            EditorGUILayout.LabelField("Инструкция для изменения кода файла:");
            var style = new GUIStyle(EditorStyles.textArea) { wordWrap = true };
            userInstructions = EditorGUILayout.TextArea(userInstructions, style, GUILayout.MinHeight(40), GUILayout.MaxHeight(120));

            EditorGUILayout.Space(10);

            EditorGUI.BeginDisabledGroup(isOverwriting || targetScript == null || string.IsNullOrWhiteSpace(userInstructions));
            if (GUILayout.Button("Сделать изменения через GPT"))
            {
                ApplyChangesViaGPT();
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(16);

            if (targetScript != null)
            {
                EditorGUILayout.LabelField("Путь к скрипту:", EditorStyles.boldLabel);
                EditorGUILayout.TextField(AssetDatabase.GetAssetPath(targetScript));
            }
        }

        private async void ApplyChangesViaGPT()
        {
            if (targetScript == null) return;

            isOverwriting = true;
            try
            {
                string scriptPath = AssetDatabase.GetAssetPath(targetScript);
                if (!File.Exists(scriptPath))
                {
                    Debug.LogError("Script file does not exist: " + scriptPath);
                    isOverwriting = false;
                    return;
                }

                string currentCode = File.ReadAllText(scriptPath);

                var messages = new List<APIOpenAI.Message>
                {
                    // Системное сообщение
                    new APIOpenAI.Message("system", systemInstruction),

                    // Основная пользовательская инструкция
                    new APIOpenAI.Message("user",
                        $"Текущий файл: {Path.GetFileName(scriptPath)}\n" +
                        $"---\n{currentCode}\n---\n\n" +
                        userInstructionPrefix + userInstructions)
                };

                // Добавить контекстные скрипты из AIEditorWindow, если они есть (для целостности)
                AppendContextFiles(messages);

                string result = await APIOpenAI.RequestCompletionAsync(messages, selectedModel);

                if (!string.IsNullOrWhiteSpace(result))
                {
                    File.WriteAllText(scriptPath, result);
                    AssetDatabase.Refresh();
                }
                else
                {
                    Debug.LogWarning("OpenAI вернул пустой результат.");
                }
            }
            finally
            {
                isOverwriting = false;
            }
        }

        // Копия метода из AIEditorWindow для добавления контекстных файлов, если потребуется расширять
        private void AppendContextFiles(List<APIOpenAI.Message> messages)
        {
            // Здесь не используются дополнительные контекстные MonoScript'ы,
            // но если потребуется - их можно добавить по аналогии с AIEditorWindow.
        }
    }
}