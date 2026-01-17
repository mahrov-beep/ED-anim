namespace Multicast.Modules.Adjust {
    using System;
    using Cysharp.Threading.Tasks;
    using Install;

    [Obsolete("AdjustAttributionModule is obsolete. Just delete it", true)]
    public class Obsolete_AdjustAttributionModule : ScriptableModule {
        public override void Setup(ModuleSetup module) {
        }

        public override async UniTask Install(Resolver resolver) {
            throw new Exception("AdjustAttributionModule is obsolete. Just delete it");
        }
    }
}