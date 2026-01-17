namespace Multicast.Modules.UserTracking {
    using System;
    using Cysharp.Threading.Tasks;
    using Install;

    [Obsolete("AlwaysAllowUserTrackingModule is obsolete. Just delete it", true)]
    internal class Obsolete_AlwaysAllowUserTrackingModule : ScriptableModule {
        public override void Setup(ModuleSetup module) {
            module.Provides<IUserTrackingService>();
        }

        public override UniTask Install(Resolver resolver) {
            throw new Exception("AlwaysAllowUserTrackingModule is obsolete. Just delete it");
        }
    }
}