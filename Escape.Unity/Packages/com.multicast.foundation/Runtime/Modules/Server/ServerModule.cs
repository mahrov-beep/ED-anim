namespace Modules.Server {
    using System;
    using Cysharp.Threading.Tasks;
    using Multicast.Install;
    using Multicast.Server;
    using Sirenix.OdinInspector;
    using UnityEngine;

    [ScriptableModule(Category = ScriptableModuleCategory.GAME_CORE)]
    public class ServerModule : ScriptableModule {
        [SerializeField]
        [InlineProperty, HideLabel, Header("Server Settings")]
        private Settings settings;
        
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

        public override UniTask Install(Resolver resolver) {
            var usedSettings = ServerMenu.UseLocalServer ? this.localSettings : this.settings;
            resolver.Register<IServerSettings>().To(new MulticastServerSettings(usedSettings));
            return UniTask.CompletedTask;
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