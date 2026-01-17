namespace Modules.Server {
    using System;
    using Cysharp.Threading.Tasks;
    using Multicast;
    using Multicast.Install;
    using Multicast.Server;
    using Sirenix.OdinInspector;
    using UnityEngine;
    using UnityEngine.Networking;

    [ScriptableModule(Category = ScriptableModuleCategory.GAME_CORE)]
    public class EscapeServerModule : ScriptableModule {
        [PropertySpace(10, 10)]
        [SerializeField]
        private int version;

        [PropertySpace(10, 10)]
        [SerializeField]
        private string versionServerUrl;

        [SerializeField]
        [InlineProperty, HideLabel, Header("Staging Settings")]
        private Settings stagingSettings;

        [SerializeField]
        [InlineProperty, HideLabel, Header("Production Settings")]
        private Settings productionSettings;

        [SerializeField]
        [InlineProperty, HideLabel, Header("Local Server Settings")]
        private Settings localSettings;

        [ShowInInspector, ReadOnly]
        private bool LocalServerInUse => ServerMenu.UseLocalServer;

        [Serializable]
        public class Settings {
            public string serverUrl = "http://localhost:5024";
        }

        public override void Setup(ModuleSetup module) {
            module.Provides<IServerSettings>();
        }

        public override async UniTask Install(Resolver resolver) {
            var usedSettings = await this.GetServerSettings();
            resolver.Register<IServerSettings>().To(new MulticastServerSettings(usedSettings));
        }

        private async UniTask<Settings> GetServerSettings() {
            if (ServerMenu.UseLocalServer) {
                return this.localSettings;
            }

            while (true) {
                try {
                    var versionRequest = UnityWebRequest.Get(this.versionServerUrl);
                    versionRequest.timeout = 2;
                    var versionResponse = await versionRequest.SendWebRequest();
                    var versionString   = versionResponse.downloadHandler.text;

                    // какой-то мусор вместо версии на сервере, показываем сообщение-заглушку
                    if (!int.TryParse(versionString, out var actualVersion)) {
                        var retry = await NativeDialog.OkCancel("Server unavailable", "Game servers at maintenance. Please try again later", "Retry", "Quit");
                        if (retry) {
                            await UniTask.Delay(TimeSpan.FromSeconds(1));
                            continue;
                        }

                        await QuitGame();
                        continue;
                    }

                    // локальная версия устарела, нужно обновиться
                    if (this.version < actualVersion) {
                        await NativeDialog.Confirm("Update available", "Download the new version of the game from the store to continue playing", "Quit");
                        await QuitGame();
                        continue;
                    }

                    // локальная версия слишком новая, подключаемся к бета серверу
                    if (this.version > actualVersion) {
#if UNITY_STANDALONE || UNITY_EDITOR
                        Debug.Log($"[EscapeServerModule] Beta build detected (local={this.version}, server={actualVersion}). Auto-connecting to staging server.");
                        return this.stagingSettings;
#else
                        var ok = await NativeDialog.OkCancel("Beta testing",
                            "You have a beta build of the game installed. " +
                            "Do you want to connect to the beta server? \n\n" +
                            "The beta server may be UNSTABLE. Your game progress will be DELETED after testing is complete.",
                            "Continue on BETA", "Cancel");

                        if (ok) {
                            return this.stagingSettings;
                        }

                        await NativeDialog.Confirm("Update required", "Download the actual version of the game from the store to continue playing", "Quit");
                        await QuitGame();
                        continue;
#endif
                    }

                    if (this.version == actualVersion) {
                        return this.productionSettings;
                    }
                }
                catch (Exception ex) {
                    // не получилось подключиться к серверу, возможно проблемы с интернетом
                    Debug.LogException(ex);
                    var retry = await NativeDialog.OkCancel("Server unavailable", "Unable to connect to the game server. Check your internet connection", "Retry", "Quit");
                    if (retry) {
                        await UniTask.Delay(TimeSpan.FromSeconds(1));
                        continue;
                    }

                    await QuitGame();
                    continue;
                }
            }
        }

        private static async UniTask QuitGame() {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
            var tcs = new UniTaskCompletionSource();
            await tcs.Task;
        }

        private class MulticastServerSettings : IServerSettings {
            private readonly Settings settings;

            public MulticastServerSettings(Settings settings) {
                this.settings = settings;
            }

            public string ServerUrl => this.settings.serverUrl;
        }
    }
}