namespace Multicast.Install {
    using Cysharp.Threading.Tasks;
    using JetBrains.Annotations;

    public static partial class ScriptableModuleFactory {
        [PublicAPI]
        public static FactoryBuilder<TResult> Factory<TResult>() {
            return new FactoryBuilder<TResult>();
        }

        public struct FactoryBuilder<TResult> {
            [PublicAPI]
            public IScriptableModule WithArgs<TArg1>(bool lazyInject = false) {
                return new FactoryInstallerModule<TArg1, TResult>(lazyInject);
            }

            [PublicAPI]
            public IScriptableModule WithArgs<TArg1, TArg2>(bool lazyInject = false) {
                return new FactoryInstallerModule<TArg1, TArg2, TResult>(lazyInject);
            }

            [PublicAPI]
            public IScriptableModule WithArgs<TArg1, TArg2, TArg3>(bool lazyInject = false) {
                return new FactoryInstallerModule<TArg1, TArg2, TArg3, TResult>(lazyInject);
            }
        }

        private class FactoryInstallerModule<T1, TResult> : IScriptableModule, INonLoggedScriptableModule {
            private readonly bool lazyInject;

            public string name => $"Factory<{typeof(TResult).Name} :: {typeof(T1)}>";

            public bool    IsPlatformSupported(string platform)        => true;
            public void    Setup(ScriptableModule.ModuleSetup module)  => module.ProvidesFactory<T1, TResult>();
            public UniTask Install(ScriptableModule.Resolver resolver) => resolver.RegisterFactory<T1, TResult>(this.lazyInject);

            public FactoryInstallerModule(bool lazyInject) {
                this.lazyInject = lazyInject;
            }

            public void PreInstall() {
            }

            public void PostInstall() {
            }
        }

        private class FactoryInstallerModule<T1, T2, TResult> : IScriptableModule, INonLoggedScriptableModule {
            private readonly bool lazyInject;

            public string name => $"Factory<{typeof(TResult).Name} :: {typeof(T1)}, {typeof(T2)}>";

            public bool    IsPlatformSupported(string platform)        => true;
            public void    Setup(ScriptableModule.ModuleSetup module)  => module.ProvidesFactory<T1, T2, TResult>();
            public UniTask Install(ScriptableModule.Resolver resolver) => resolver.RegisterFactory<T1, T2, TResult>(this.lazyInject);

            public FactoryInstallerModule(bool lazyInject) {
                this.lazyInject = lazyInject;
            }

            public void PreInstall() {
            }

            public void PostInstall() {
            }
        }

        private class FactoryInstallerModule<T1, T2, T3, TResult> : IScriptableModule, INonLoggedScriptableModule {
            private readonly bool lazyInject;

            public string name => $"Factory<{typeof(TResult).Name} :: {typeof(T1)}, {typeof(T2)}, {typeof(T3)}>";

            public bool    IsPlatformSupported(string platform)        => true;
            public void    Setup(ScriptableModule.ModuleSetup module)  => module.ProvidesFactory<T1, T2, T3, TResult>();
            public UniTask Install(ScriptableModule.Resolver resolver) => resolver.RegisterFactory<T1, T2, T3, TResult>(this.lazyInject);

            public FactoryInstallerModule(bool lazyInject) {
                this.lazyInject = lazyInject;
            }

            public void PreInstall() {
            }

            public void PostInstall() {
            }
        }
    }
}