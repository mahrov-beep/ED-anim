// ReSharper disable EnforceIfStatementBraces
// ReSharper disable ArrangeTypeMemberModifiers
namespace AITools.AIEditorWindowTool {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using OpenAI;
    using UI;
    using UnityEditor;
    using UnityEngine;
    using Debug = UnityEngine.Debug;

    public abstract class AIEditorWindow : EditorWindow {
        protected bool _showGPTPanel;

        EModels                            model;
        bool                               fileModifiedCached;
        double                             lastCheckTime;
        double                             lastAutoSaveTime;
        (string csPath, string promptPath) pathTuple;
        bool                               isGenerating;

        private ContextScriptsToolbar contextScriptsToolbar;

        Vector2 promptScrollPos;

        private string ContextPrefsKey { get; set; }

        [SerializeField]
        private PromptField promptField;
        private GitFilesToolbar gitFilesToolbar;

        protected virtual void OnEnable() {
            ContextPrefsKey       = $"{GetType().Name}/ContextScripts_List";
            contextScriptsToolbar = new ContextScriptsToolbar(ContextPrefsKey);

            pathTuple = GetScriptAndPromptPaths();

            lastAutoSaveTime = EditorApplication.timeSinceStartup;

            promptField = new PromptField(pathTuple.promptPath);

            var gitFiles = new[] {
                            pathTuple.csPath, pathTuple.csPath + ".meta",
                            pathTuple.promptPath, pathTuple.promptPath + ".meta",
            };
            gitFilesToolbar = new(gitFiles, () => $"commit {GetFileName()}.cs, {GetFileName()}.cs.prompt");

            Undo.undoRedoPerformed += Repaint;

            return;

            (string csPath, string promptPath) GetScriptAndPromptPaths() {
                var scriptAsset    = MonoScript.FromScriptableObject(this);
                var scriptFullPath = AssetDatabase.GetAssetPath(scriptAsset);
                var directory      = Path.GetDirectoryName(scriptFullPath);
                var fileName       = GetFileName();
                var csPath         = Path.Combine(directory!, fileName + ".cs");
                var promptPath     = Path.Combine(directory, fileName + ".cs.prompt");
                return (csPath, promptPath);
            }
        }

        protected virtual void OnDisable() {
            Undo.undoRedoPerformed -= Repaint;

            promptField.Save();
            contextScriptsToolbar.Save();
        }

        public virtual void OnGUI() {
            _showGPTPanel = EditorGUILayout.Foldout(_showGPTPanel, "AI Editor Tools");

            if (!_showGPTPanel) {
                return;
            }

            model = (EModels)EditorGUILayout.EnumPopup("Select Model", model);

            promptField.Draw(this);

            EditorGUILayout.Space(10);

            contextScriptsToolbar.Draw();

            EditorGUILayout.Space(10);

            if (model is EModels.None) return;

            EditorGUI.BeginDisabledGroup(isGenerating);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Write code"))   GenerateFromScratch();
            if (GUILayout.Button("Rework code"))  GenerateWithCurrentCode();
            // todo if (GUILayout.Button("Write prompt")) FillPromptFromCurrentCode();
            if (GUILayout.Button("Save prompt"))  promptField.Save();

            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();

            var time = EditorApplication.timeSinceStartup;

            gitFilesToolbar.Draw();

            if (time - lastAutoSaveTime >= 2.5) {
                promptField.Save();
                lastAutoSaveTime = time;
            }
        }

        async void GenerateFromScratch() {
            try {
                isGenerating = true;

                var (csPath, _) = pathTuple;

                if (!File.Exists(csPath)) {
                    Debug.LogError("Файл не найден: " + csPath);
                    return;
                }

                var messages = WrapPromptWithoutCode(GetFileName(), promptField.Prompt);

                if (contextScriptsToolbar.Count > 0) {
                    messages.Add(new APIOpenAI.Message(role: "user",
                                    $"Here are additional files for context, NOT modify them:\n" +
                                    $"{contextScriptsToolbar.GetCombinedContextText()}"));
                }

                var generatedCode = await APIOpenAI.RequestCompletionAsync(messages, model);

                if (string.IsNullOrEmpty(generatedCode)) {
                    Debug.LogError("Ошибка генерации кода");
                    return;
                }

                await File.WriteAllTextAsync(csPath, generatedCode);

                AssetDatabase.Refresh();
            }
            catch (Exception e) {
                Debug.Log(e.Message);
            }
            finally {
                isGenerating = false;
            }
        }

        async void GenerateWithCurrentCode() {
            try {
                isGenerating = true;

                var (csPath, _) = pathTuple;

                if (!File.Exists(csPath)) {
                    Debug.LogError("Файл не найден: " + csPath);
                    return;
                }

                var currentCode = await File.ReadAllTextAsync(csPath);
                var messages    = WrapPromptWithCode(GetFileName(), promptField.Prompt, currentCode);
                Debug.LogError(promptField.Prompt);

                if (contextScriptsToolbar.Count > 0) {
                    messages.Add(new APIOpenAI.Message(role: "user",
                                    $"Here are additional files for context, NOT modify them:\n" +
                                    $"{contextScriptsToolbar.GetCombinedContextText()}"));
                }

                var generatedCode = await APIOpenAI.RequestCompletionAsync(messages, model);

                if (string.IsNullOrEmpty(generatedCode)) {
                    Debug.LogError("Ошибка генерации кода");
                    return;
                }

                await File.WriteAllTextAsync(csPath, generatedCode);

                AssetDatabase.Refresh();
            }
            catch (Exception e) {
                Debug.LogError($"{e.Message} while {nameof(GenerateWithCurrentCode)}:\n {e.StackTrace}");
            }
            finally {
                isGenerating = false;
            }
        }

        async void FillPromptFromCurrentCode() {
            try {
                isGenerating = true;

                var (csPath, _) = pathTuple;

                if (!File.Exists(csPath)) {
                    Debug.LogError("Файл не найден: " + csPath);
                    return;
                }

                var currentCode = await File.ReadAllTextAsync(csPath);

                const string systemMessage =
                                "You are an assistant for creating a short description of the functionality of EditorWindow.\n" +
                                "The description of the individual functionality MUST be on a separate line.\n" +
                                "The prompt MUST be focused on the functionality of this window.\n" +
                                "YOU MUST USE public override void OnGUI() with base.OnGUI() call into inheritor!!!\n" +
                                "Only provide the prompt text without additional explanations or markdown.\n";

                var userMessage = string.Empty;
                if (string.IsNullOrEmpty(promptField.Prompt)) {
                    userMessage += "Write a very short prompt describing main functions of this EditorWindow.\n";
                }
                else {
                    userMessage += "Improve the current description of the functionality of EditorWindow.\n";
                    userMessage += "Only change the obviously unnecessary or unclear parts of the current description.\n";
                    userMessage += "The prompt MUST be in the same language as the current description of the functionality.\n";
                    userMessage += $"Here is the current description of the functionality:\n{promptField.Prompt}\n";
                }
                userMessage += $"Here is the current code:\n{currentCode}";

                var messages = new List<APIOpenAI.Message> {
                                new(role: "system", content: systemMessage),
                                new(role: "user", content: userMessage),
                };

                if (contextScriptsToolbar.Count > 0)
                    messages.Add(new APIOpenAI.Message(role: "user",
                                    $"Here are additional files for context, NOT modify them:\n" +
                                    $"{contextScriptsToolbar.GetCombinedContextText()}"));

                var newPrompt = await APIOpenAI.RequestCompletionAsync(messages, model);

                if (string.IsNullOrEmpty(newPrompt)) {
                    Debug.LogError("Ошибка получения промта");
                    return;
                }

                Undo.RecordObject(this, "Prompt Change");

                //  todo promptField.Prompt = newPrompt;

                EditorUtility.SetDirty(this);

                Repaint();
            }
            catch (Exception e) {
                Debug.LogError($"{e.Message} while {nameof(FillPromptFromCurrentCode)}:\n {e.StackTrace}");
            }
            finally {
                isGenerating = false;
            }
        }

        string GetFileName() {
            return GetType().Name;
        }

        static List<APIOpenAI.Message> WrapPromptWithoutCode(string scriptName, string prompt) {
            var systemMessage =
                            $"Write a Unity C# EditorWindow named \"{scriptName}\".\n" +
                            $"- I need ready to compile EditorWindow script.\n" +
                            $"- Do not add any explanations.\n" +
                            $"- Do not use markdown in response.\n" +
                            $"- Inheritor MUST call base.OnGUI.\n" +
                            $"- Inheritor MUST HAVE public override void OnGUI() with base.OnGUI()!!!\n" +
                            $"- Inheritor MUST be inside namespace AITools.AIEditorWindowTool\n" +
                            $"- Inheritor MUST inherit from AIEditorWindow.\n" +
                            $"- Inheritor MUST call base.OnEnable if need override it.\n" +
                            $"- Inheritor MUST call base.OnDisable if need override it.\n" +
                            $"- Do not define AIEditorWindow, it is already defined.\n" +
                            $"- Must place ShowWindow method in [MenuItem(\"GPTGenerated/\" + nameof({scriptName}))]\n";

            var userMessage = $"It must do the following: {prompt}";

            return new() {
                            new() { role = "system", content = systemMessage },
                            new() { role = "user", content   = userMessage },
            };
        }

        static List<APIOpenAI.Message> WrapPromptWithCode(string scriptName, string prompt, string currentCode) {
            var systemMessage =
                            $"Write a Unity C# EditorWindow named \"{scriptName}\".\n" +
                            $"- I need ready to compile EditorWindow script.\n" +
                            $"- Do not add any explanations.\n" +
                            $"- Do not use markdown in response.\n" +
                            $"- Inheritor MUST call base.OnGUI.\n" +
                            $"- Inheritor MUST be inside namespace AITools.AIEditorWindowTool\n" +
                            $"- Inheritor MUST inherit from AIEditorWindow.\n" +
                            $"- Inheritor MUST call base.OnEnable if need override it.\n" +
                            $"- Inheritor MUST call base.OnDisable if need override it.\n" +
                            $"- Do not define AIEditorWindow, it is already defined.\n" +
                            $"- Must place ShowWindow method in [MenuItem(\"GPTGenerated/\" + nameof({scriptName}))]\n";

            var userMessage =
                            $"Consider this existing code:\n{currentCode}\n" +
                            $"It must do the following:\n{prompt}";

            return new() {
                            new(role: "system", content: systemMessage),
                            new(role: "user", content: userMessage),
            };
        }
    }
}