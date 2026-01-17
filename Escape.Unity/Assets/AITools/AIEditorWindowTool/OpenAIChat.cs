namespace AITools.AIEditorWindowTool {
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using OpenAI;

    public class OpenAIChat : AIEditorWindow {
        [System.Serializable]
        private class ChatMessage {
            public string role;
            public string content;

            public ChatMessage(string role, string content) {
                this.role    = role;
                this.content = content;
            }
        }

        private List<ChatMessage> chatHistory = new();
        private Vector2           scrollPos;
        private string            userInput = "";
        private bool              isRequesting;

        [MenuItem("GPTGenerated/" + nameof(OpenAIChat))]
        public static void ShowWindow() {
            var window = GetWindow<OpenAIChat>();
            window.titleContent = new GUIContent("OpenAIChat");
            window.Show();
        }

        public override void OnGUI() {
            base.OnGUI();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("OpenAI Chat (ChatGPT)", EditorStyles.boldLabel);

            using (new GUILayout.VerticalScope(GUILayout.ExpandHeight(true))) {
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandHeight(true));
                foreach (var msg in chatHistory) {
                    var    boxStyle = new GUIStyle(EditorStyles.textArea) { wordWrap = true };
                    EditorGUI.BeginDisabledGroup(true);
                    if (msg.role == "user") {
                        EditorGUILayout.LabelField("You:", EditorStyles.boldLabel);
                    }
                    else {
                        EditorGUILayout.LabelField("Assistant:", EditorStyles.boldLabel);
                    }

                    EditorGUILayout.TextArea(msg.content, boxStyle, GUILayout.ExpandHeight(false));
                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.Space(4);
                }
                EditorGUILayout.EndScrollView();

                EditorGUI.BeginDisabledGroup(isRequesting);
                EditorGUILayout.LabelField("Ваш вопрос:", EditorStyles.label);
                userInput = EditorGUILayout.TextArea(userInput, GUILayout.MinHeight(40));

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Send", GUILayout.MinWidth(75)) && !string.IsNullOrWhiteSpace(userInput) && !isRequesting) {
                    SendUserInput();
                }
                if (GUILayout.Button("Очистить чат", GUILayout.MinWidth(100))) {
                    ClearChat();
                }
                EditorGUILayout.EndHorizontal();
                EditorGUI.EndDisabledGroup();
            }
        }

        private void SendUserInput() {
            var promptText = userInput.Trim();
            chatHistory.Add(new ChatMessage("user", promptText));
            userInput    = "";
            isRequesting = true;
            RequestToOpenAI();
        }

        private void ClearChat() {
            chatHistory.Clear();
            userInput    = "";
            isRequesting = false;
            Repaint();
        }

        private async void RequestToOpenAI() {
            var messagesForApi = new List<APIOpenAI.Message>();
            foreach (var msg in chatHistory) {
                messagesForApi.Add(new APIOpenAI.Message(msg.role, msg.content));
            }

            var result = await APIOpenAI.RequestCompletionAsync(messagesForApi, EModels.GPT4o);

            if (!string.IsNullOrWhiteSpace(result)) {
                chatHistory.Add(new ChatMessage("assistant", result.Trim()));
            }
            isRequesting = false;
            Repaint();
        }
    }
}