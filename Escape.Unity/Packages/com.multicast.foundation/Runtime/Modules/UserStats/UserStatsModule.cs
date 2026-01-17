namespace Multicast.Modules.UserStats {
    using Cysharp.Threading.Tasks;
    using Install;
    using Multicast.UserStats;
    using UnityEngine;

    public class UserStatsModule : IScriptableModule {
        public string name => "UserStats";

        public bool IsPlatformSupported(string platform) => true;

        public void Setup(ScriptableModule.ModuleSetup module) {
        }

        public async UniTask Install(ScriptableModule.Resolver resolver) {
            var userStatsData   = await resolver.Get<UdUserStatsRepo>();
            var userDataService = await resolver.Get<IUserDataService>();
            var timeService     = await resolver.Get<ITimeService>();

            var version = Application.version;

            using (userDataService.Root.BeginTransactionScope("UserStatsModule.Install")) {
                userStatsData.SessionsCount.Value += 1;

                if (string.IsNullOrEmpty(userStatsData.InstallGameVersion.Value)) {
                    userStatsData.InstallGameVersion.Value = version;
                }

                if (userStatsData.LastPlayedDaysCountUpdateDate.Value.Date < timeService.Now.Date) {
                    userStatsData.PlayedDaysCount.Value               += 1;
                    userStatsData.LastPlayedDaysCountUpdateDate.Value =  timeService.Now;
                }

                if (userStatsData.PlaytimeSeconds.Value <= 0 ||
                    userStatsData.FirstOpenTime.Value.Equals(default)) {
                    userStatsData.FirstOpenTime.Value = timeService.Now;
                }
            }
        }

        public void PreInstall() {
        }

        public void PostInstall() {
        }
    }
}