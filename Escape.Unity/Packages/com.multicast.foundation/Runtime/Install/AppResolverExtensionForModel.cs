namespace Multicast.Install {
    using Cysharp.Threading.Tasks;
    using JetBrains.Annotations;
    using UniMob;
    using UserData;

    public static class AppResolverExtensionForModel {
        [PublicAPI]
        public static void ProvidesKeyedModel<TDef, TData, TModel, TCollectionModel>(this ScriptableModule.ModuleSetup module)
            where TDef : Def
            where TData : class, IDataObject
            where TModel : Model<TDef, TData>
            where TCollectionModel : KeyedModelBase<TDef, TData, TModel> {
            module.ProvidesFactory<Lifetime, TDef, TData, TModel>();
            module.Provides<TCollectionModel>();
        }

        [PublicAPI]
        public static async UniTask RegisterKeyedModel<TDef, TData, TModel, TCollectionModel>(this ScriptableModule.Resolver resolver, bool lazyInject = false)
            where TDef : Def
            where TData : class, IDataObject
            where TModel : Model<TDef, TData>
            where TCollectionModel : KeyedModelBase<TDef, TData, TModel> {
            await resolver.RegisterFactory<Lifetime, TDef, TData, TModel>(lazyInject);
            await resolver.Register<TCollectionModel>().ToAsync<TCollectionModel>(lazyInject);
        }
    }
}