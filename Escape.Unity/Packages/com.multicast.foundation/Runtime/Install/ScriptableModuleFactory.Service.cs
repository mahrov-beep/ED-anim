namespace Multicast.Install {
    using Cysharp.Threading.Tasks;
    using JetBrains.Annotations;

    public static partial class ScriptableModuleFactory {
        [PublicAPI]
        public static IScriptableModule Service<T>(T instance) {
            return new InstanceServiceInstallerModule<T>(instance);
        }

        [PublicAPI]
        public static IScriptableModule Service<T, TImpl>() where TImpl : class, T {
            return new ServiceInstallerModule<T, TImpl>();
        }

        private class InstanceServiceInstallerModule<T> : IScriptableModule, INonLoggedScriptableModule {
            private readonly T instance;

            public InstanceServiceInstallerModule(T instance) => this.instance = instance;

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

        private class ServiceInstallerModule<T, TImpl> : IScriptableModule, INonLoggedScriptableModule
            where TImpl : class, T {
            public string name                                       => typeof(T).Name;
            public bool   IsPlatformSupported(string platform)       => true;
            public void   Setup(ScriptableModule.ModuleSetup module) => module.Provides<T>();

            public UniTask Install(ScriptableModule.Resolver resolver) => resolver.Register<T>().ToAsync<TImpl>();

            public void PreInstall() {
            }

            public void PostInstall() {
            }
        }
    }
}