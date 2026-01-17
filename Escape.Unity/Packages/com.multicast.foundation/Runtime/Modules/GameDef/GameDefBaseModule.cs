namespace Multicast.Modules.GameDef {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Collections;
    using Cysharp.Threading.Tasks;
    using Install;
    using UnityEngine;

    public abstract class GameDefBaseModule<TDef> : ScriptableModule, IScriptableModuleWithPriority {
        [SerializeField] private string configsAddressableGroup = "Configs";

        private TDef gameDef;

        private List<FieldInfo> autoProvidedFields;

        private AddressableCache<TextAsset> cache;
        private UniTask                     cachePreloadTask;

        public int Priority { get; } = ScriptableModulePriority.LATE;

        public override void Setup(ModuleSetup module) {
            this.autoProvidedFields = this.GetAutoProvidedFields();

            module.Provides<TDef>();
            module.ProvidesDynamic(type => this.autoProvidedFields.Any(it => it.FieldType == type));
        }

        public override void PreInstall() {
            base.PreInstall();

            this.cache            = new AddressableCache<UnityEngine.TextAsset>();
            this.cachePreloadTask = this.cache.Preload(this.configsAddressableGroup);
        }

        public override async UniTask Install(Resolver resolver) {
            await this.cachePreloadTask;

            this.gameDef = this.CreateDef(this.cache.Select(it => new Multicast.TextAsset(it.text)));

            resolver.Register<TDef>().To(this.gameDef);
            resolver.RegisterDynamic(this.ResolveAutoProvidedField);
        }

        private object ResolveAutoProvidedField(Type type) {
            foreach (var fieldInfo in this.autoProvidedFields) {
                if (fieldInfo.FieldType == type) {
                    return fieldInfo.GetValue(this.gameDef);
                }
            }

            return null;
        }

        protected abstract TDef CreateDef(IEnumerableCache<Multicast.TextAsset> cache);

        private List<FieldInfo> GetAutoProvidedFields() {
            return typeof(TDef).GetFields(BindingFlags.Public | BindingFlags.Instance).ToList();
        }
    }
}