namespace Multicast.Install {
    using Cysharp.Threading.Tasks;
    using JetBrains.Annotations;
    using UserData;

    public static partial class ScriptableModuleFactory {
        [PublicAPI]
        public static IScriptableModule Model<TModel>(bool lazyInject = false)
            where TModel : Model {
            return new ModelInstallerModule<TModel>(lazyInject);
        }

        [PublicAPI]
        public static IScriptableModule KeyedModel<TDef, TData, TModel, TCollectionModel>(bool lazyInject = false)
            where TDef : Def
            where TData : class, IDataObject
            where TModel : Model<TDef, TData>
            where TCollectionModel : KeyedModelBase<TDef, TData, TModel> {
            return new KeyedModelInstallerModule<TDef, TData, TModel, TCollectionModel>(lazyInject);
        }

        private class ModelInstallerModule<TModel> : IScriptableModule, INonLoggedScriptableModule
            where TModel : Model {
            private readonly bool lazyInject;

            public string  name                                        => typeof(TModel).Name;
            public bool    IsPlatformSupported(string platform)        => true;
            public void    Setup(ScriptableModule.ModuleSetup module)  => module.Provides<TModel>();
            public UniTask Install(ScriptableModule.Resolver resolver) => resolver.Register<TModel>().ToAsync<TModel>(this.lazyInject);

            public ModelInstallerModule(bool lazyInject) {
                this.lazyInject = lazyInject;
            }

            public void PreInstall() {
            }

            public void PostInstall() {
            }
        }

        private class KeyedModelInstallerModule<TDef, TData, TModel, TCollectionModel> : IScriptableModule, INonLoggedScriptableModule
            where TDef : Def
            where TData : class, IDataObject
            where TModel : Model<TDef, TData>
            where TCollectionModel : KeyedModelBase<TDef, TData, TModel> {
            private readonly bool lazyInject;

            public string  name                                        => typeof(TModel).Name;
            public bool    IsPlatformSupported(string platform)        => true;
            public void    Setup(ScriptableModule.ModuleSetup module)  => module.ProvidesKeyedModel<TDef, TData, TModel, TCollectionModel>();
            public UniTask Install(ScriptableModule.Resolver resolver) => resolver.RegisterKeyedModel<TDef, TData, TModel, TCollectionModel>(this.lazyInject);

            public KeyedModelInstallerModule(bool lazyInject) {
                this.lazyInject = lazyInject;
            }

            public void PreInstall() {
            }

            public void PostInstall() {
            }
        }
    }
}