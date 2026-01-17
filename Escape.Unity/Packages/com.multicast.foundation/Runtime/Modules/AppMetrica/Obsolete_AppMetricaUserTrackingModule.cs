namespace Multicast.Modules.AppMetrica.UserTracking {
    using System;
    using Cysharp.Threading.Tasks;
    using Install;

    [Obsolete("AppMetricaUserTrackingModule is obsolete. Just delete it", true)]
    public class Obsolete_AppMetricaUserTrackingModule : ScriptableModule {
        public override void Setup(ModuleSetup module) {
        }

        public override async UniTask Install(Resolver resolver) {
            throw new Exception("AppMetricaUserTrackingModule is obsolete. Just delete it");
        }
    }
}