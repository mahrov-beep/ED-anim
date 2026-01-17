namespace Game.UI.Modules {
    using System;
    using Cysharp.Threading.Tasks;
    using Multicast;
    using Multicast.Collections;
    using Multicast.Install;
    using Quantum;
    using Shared;

    public class QuantumDefSetupModule : ScriptableModule {
        public override void Setup(ModuleSetup module) {
        }

        public override async UniTask Install(Resolver resolver) => Setup(await resolver.Get<GameDef>());

        private static void Setup(GameDef gameDef) {
            Setup(out ItemAsset.DefCache, gameDef.Items, it => it.ItemKey);
        }

        private static void Setup<TKey, TDef, TDefImpl>(
            out QuantumDefCache<TKey, TDef> cache, LookupCollection<TDefImpl> lookup, Func<TKey, string> keySelector)
            where TKey : AssetObject
            where TDef : class
            where TDefImpl : Def, TDef {
            cache = new QuantumDefCache<TKey, TDef>(it => lookup.Get(keySelector(it)), capacity: lookup.Items.Count);
        }
    }
}