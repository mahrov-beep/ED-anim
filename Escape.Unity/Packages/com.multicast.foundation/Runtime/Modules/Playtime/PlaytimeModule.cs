namespace Multicast.Modules.Playtime {
    using System;
    using Cheats;
    using Cysharp.Threading.Tasks;
    using GameProperties;
    using Install;
    using Morpeh;
    using Multicast.UserStats;
    using Scellecs.Morpeh;
    using UniMob;
    using UnityEngine;

    public class PlaytimeModule : IScriptableModule {
        public string name => "Playtime";

        public bool IsPlatformSupported(string platform) => true;

        public void Setup(ScriptableModule.ModuleSetup module) {
        }

        public void PreInstall() {
        }

        public async UniTask Install(ScriptableModule.Resolver resolver) {
            var worldRegistration = await resolver.Get<IWorldRegistration>();
            var cheatProperties   = await resolver.Get<ICheatGamePropertiesRegistry>();
            var userStats         = await resolver.Get<UdUserStatsRepo>();
            var gameProperties    = await resolver.Get<GamePropertiesModel>();
            var appLifetime       = await resolver.Get<Lifetime>();
            var timeService       = await resolver.Get<ITimeService>();

            worldRegistration.RegisterInstaller(InstallPlaytimeSystems);

            cheatProperties.Register("playtime",
                () => userStats.PlaytimeMinutes.Value,
                v => App.Execute(new CheatSetPlaytimeMinutesCommand(v))
            );

            gameProperties.RegisterAutoSyncedProperty(appLifetime,
                AppGameProperties.Ints.Playtime,
                () => userStats.PlaytimeMinutes.Value);

            gameProperties.RegisterAutoSyncedProperty(appLifetime,
                AppGameProperties.Ints.HoursSinceInstall,
                () => {
                    var firstOpenTime = userStats.FirstOpenTime.Value;

                    if (firstOpenTime.Equals(default)) {
                        return 0;
                    }

                    var totalHours = (float) Math.Min((timeService.Now - userStats.FirstOpenTime.Value).TotalHours, float.MaxValue);
                    return Mathf.CeilToInt(totalHours);
                });
        }

        public void PostInstall() {
        }

        private static void InstallPlaytimeSystems(SystemsGroup systems) {
            systems.AddExistingSystem<PlayTimeSystem>();
        }
    }
}