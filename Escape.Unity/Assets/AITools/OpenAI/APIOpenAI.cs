using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace AITools.OpenAI {
    using System.Collections.Generic;
    public enum EModels {
        None,
        GPT4o,
        GPT4oMini,
        O1,
        GPT4_5Preview,
        gpt_4_1
    }

    public static class APIOpenAI {
        private const           string     ApiUrl     = "https://api.openai.com/v1/chat/completions";
        private static readonly HttpClient HttpClient = new();

        private static string GetModelName(this EModels model) {
            return model switch {
                            EModels.GPT4o => "gpt-4o",
                            EModels.GPT4oMini => "gpt-4o-mini",
                            EModels.O1 => "o1",
                            EModels.GPT4_5Preview => "gpt-4.5-preview",
                            EModels.gpt_4_1 => "gpt-4.1",
                            _ => throw new ArgumentOutOfRangeException(nameof(model), model, null)
            };
        }

        public static async Task<string> RequestCompletionAsync(List<Message> messages, EModels model) {
            var apiKey = AIToolsSettings.instance.openAIAPIKey;

            if (string.IsNullOrEmpty(apiKey)) {
                Debug.LogError("API ключ не задан");
                return string.Empty;
            }

            if (model == EModels.None) {
                Debug.LogError($"{nameof(EModels)} is {nameof(EModels.None)}");
                return string.Empty;
            }

            var request = new {
                            model    = model.GetModelName(),
                            messages = messages.ToArray(),
            };

            Debug.Log($"OPENAI REQUEST:\n{JsonConvert.SerializeObject(request, Formatting.Indented)}");
            // EditorGUIUtility.systemCopyBuffer = JsonConvert.SerializeObject(request, Formatting.Indented);

            var jsonContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

            HttpClient.DefaultRequestHeaders.Clear();
            HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            try {
                var response = await HttpClient.PostAsync(ApiUrl, jsonContent);

                if (!response.IsSuccessStatusCode) {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    Debug.LogError($"Ошибка запроса: {response.StatusCode} - {response.ReasonPhrase}\nТело ответа: {errorBody}");
                    return string.Empty;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var data         = JsonConvert.DeserializeObject<Response>(jsonResponse);

                return data.choices[0].message.content;
            }
            catch (Exception e) {
                Debug.LogError($"Исключение при запросе: {e.Message}");
                return string.Empty;
            }
        }

        [Serializable]
        public class Response {
            public string   id;
            public Choice[] choices;
        }

        [Serializable]
        public class Choice {
            public int     index;
            public Message message;
        }

        [Serializable]
        public class Message {
            public string role;
            public string content;
            public Message() { }

            public Message(string role, string content) {
                this.role    = role;
                this.content = content;
            }
        }
    }
}