using UnityEngine;
using UnityEditor;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using AITools.OpenAI;

namespace AITools.AIEditorWindowTool {
    public class AISpriteGenerator : AIEditorWindow {
        private string imagePrompt = "";
        private string imageSize = "1024x1024";
        private string imageQuality = "high";
        private bool transparentBg = true;
        private DefaultAsset saveFolder;
        private Texture2D generatedTexture;
        private Vector2 scrollPosition;
        private bool generatingImage;

        private const string OpenAIImageGenUrl = "https://api.openai.com/v1/images/generations";

        [MenuItem("GPTGenerated/" + nameof(AISpriteGenerator))]
        public static void ShowWindow() {
            var window = GetWindow<AISpriteGenerator>();
            window.titleContent = new GUIContent("AISpriteGenerator");
            window.Show();
        }

        protected override void OnEnable() {
            base.OnEnable();
        }

        protected override void OnDisable() {
            base.OnDisable();
        }

        public override void OnGUI() {
            base.OnGUI();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("AI Sprite Generator (GPT-Image-1)", EditorStyles.boldLabel);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            EditorGUILayout.LabelField("Image Prompt:", EditorStyles.label);
            imagePrompt = EditorGUILayout.TextArea(imagePrompt, GUILayout.Height(60));

            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField("Image Parameters", EditorStyles.boldLabel);
            imageSize = EditorGUILayout.Popup("Size", GetSizeIndex(imageSize), new[] { "1024x1024", "1024x1536", "1536x1024" }) switch {
                0 => "1024x1024",
                1 => "1024x1536",
                2 => "1536x1024",
                _ => "1024x1024"
            };
            imageQuality = EditorGUILayout.Popup("Quality", GetQualityIndex(imageQuality), new[] { "low", "medium", "high" }) switch {
                0 => "low",
                1 => "medium",
                2 => "high",
                _ => "high"
            };
            transparentBg = EditorGUILayout.Toggle("Transparent Background", transparentBg);

            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField("Save Folder:", EditorStyles.boldLabel);
            saveFolder = (DefaultAsset)EditorGUILayout.ObjectField("Folder", saveFolder, typeof(DefaultAsset), false);

            EditorGUILayout.Space(10);
            EditorGUI.BeginDisabledGroup(generatingImage);
            if (GUILayout.Button("Generate Sprite", GUILayout.Height(35))) {
                if (string.IsNullOrWhiteSpace(imagePrompt)) {
                    Debug.LogError("Image Prompt cannot be empty!");
                } else if (saveFolder == null) {
                    Debug.LogError("Please select a valid save folder!");
                } else {
                    RequestImage();
                }
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(10);

            if (generatedTexture != null) {
                EditorGUILayout.LabelField("Generated Sprite Preview:", EditorStyles.boldLabel);
                GUILayout.Label(generatedTexture, GUILayout.Width(256), GUILayout.Height(256));

                if (GUILayout.Button("Save Sprite to Folder")) {
                    SaveGeneratedSprite();
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private int GetQualityIndex(string quality) {
            return quality switch {
                "low" => 0,
                "medium" => 1,
                "high" => 2,
                _ => 2
            };
        }

        private int GetSizeIndex(string size) {
            return size switch {
                "1024x1024" => 0,
                "1024x1536" => 1,
                "1536x1024" => 2,
                _ => 0
            };
        }

        private async void RequestImage() {
            generatingImage = true;

            var apiKey = AIToolsSettings.instance.openAIAPIKey;

            if (string.IsNullOrEmpty(apiKey)) {
                Debug.LogError("OpenAI API key not set!");
                generatingImage = false;
                return;
            }

            var requestBody = new {
                model = "gpt-image-1",
                prompt = imagePrompt,
                size = imageSize,
                quality = imageQuality,
                background = transparentBg ? "transparent" : "auto"
            };

            var jsonContent = new StringContent(JsonConvert.SerializeObject(requestBody), System.Text.Encoding.UTF8, "application/json");
            
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            try {
                var response = await httpClient.PostAsync(OpenAIImageGenUrl, jsonContent);

                if (!response.IsSuccessStatusCode) {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    Debug.LogError($"API Error: {response.StatusCode} - {response.ReasonPhrase}\n{errorContent}");
                    generatingImage = false;
                    return;
                }

                var json = await response.Content.ReadAsStringAsync();
                var imageResponse = JsonConvert.DeserializeObject<ImageGenResponse>(json);

                if (imageResponse.data != null && imageResponse.data.Length > 0) {
                    var imageData = System.Convert.FromBase64String(imageResponse.data[0].b64_json);
                    generatedTexture = new Texture2D(2, 2);
                    generatedTexture.LoadImage(imageData);
                }
                else {
                    Debug.LogError("No image data returned from OpenAI.");
                }
            }
            catch (System.Exception ex) {
                Debug.LogError("Exception during OpenAI Request: " + ex.Message);
            }

            generatingImage = false;
            Repaint();
        }

        private void SaveGeneratedSprite() {
            if (generatedTexture == null || saveFolder == null) {
                Debug.LogError("Generated texture or save folder not set correctly.");
                return;
            }

            var folderPath = AssetDatabase.GetAssetPath(saveFolder);
            if (!Directory.Exists(folderPath)) {
                Debug.LogError("Invalid save folder path.");
                return;
            }

            var fileName = $"AISprite_{System.DateTime.Now:yyyyMMdd_HHmmss}.png";
            var fullPath = Path.Combine(folderPath, fileName);
            var pngData = generatedTexture.EncodeToPNG();

            if (pngData == null) {
                Debug.LogError("Failed to encode texture to PNG.");
                return;
            }

            File.WriteAllBytes(fullPath, pngData);
            Debug.Log($"Saved Sprite at: {fullPath}");

            AssetDatabase.Refresh();
        }

        private class ImageGenResponse {
            public ImageData[] data;
        }

        private class ImageData {
            public string b64_json;
        }
    }
}