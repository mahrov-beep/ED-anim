namespace Multicast.Install {
    using System;
    using Cysharp.Threading.Tasks;
    using JetBrains.Annotations;

    public partial class ScriptableModuleFactory {
        [PublicAPI]
        public static IScriptableModule Asset<T>([NotNull] T instance) {
            if (instance == null) {
                throw new ArgumentNullException(nameof(instance));
            }

            return new InstanceAssetInstallerModule<T>(instance);
        }

        private class InstanceAssetInstallerModule<T> : IScriptableModule, INonLoggedScriptableModule {
            private readonly T instance;

            public InstanceAssetInstallerModule(T instance) => this.instance = instance;

            public string name                                       => typeof(T).Name;
            public bool   IsPlatformSupported(string platform)       => true;
            public void   Setup(ScriptableModule.ModuleSetup module) => module.Provides<T>();

            public UniTask Install(ScriptableModule.Resolver resolver) {
                resolver.Register<T>().To(this.instance);
                return UniTask.CompletedTask;
            }

            public void PreInstall() {
            }

            public void PostInstall() {
            }
        }
    }
}