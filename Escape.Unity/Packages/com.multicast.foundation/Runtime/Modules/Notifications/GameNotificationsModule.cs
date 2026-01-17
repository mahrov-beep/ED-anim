namespace Multicast.Notifications {
    using System;
    using System.Linq;
    using Cheats;
    using Collections;
    using Cysharp.Threading.Tasks;
    using GameProperties;
    using Install;
    using Modules.Notifications;
    using Scellecs.Morpeh;
    using UniMob;
    using UnityEngine;

    [ScriptableModule(Category = ScriptableModuleCategory.GAME_CORE)]
    public class GameNotificationsModule : ScriptableModule {
        public override void Setup(ModuleSetup module) {
            module.Provides<IGameNotifications>();
        }

        public override async UniTask Install(Resolver resolver) {
            var lifetime       = await resolver.Get<Lifetime>();
            var gameProperties = await resolver.Get<GamePropertiesModel>();

            var manager = GameObject.FindObjectOfType<GameNotificationsManager>();

            if (manager == null) {
                Debug.LogError("No GameNotificationsManager object found in scene. Please check GameNotifications integration");
                return;
            }

            var localizationProvider = new GameNotificationsLocalizationProvider();

            var platformFactory  = await resolver.Get<IGameNotificationsPlatformFactory>();
            var notificationsDef = await resolver.Get<LookupCollection<GameNotificationDef>>();

            var channels = notificationsDef.Items
                .GroupBy(it => it.channel)
                .Select(it => it.Key)
                .Select(CreateChannel)
                .ToArray();

            manager.Initialize(platformFactory, channels);

            resolver.Register<IGameNotifications>().To(new GameNotifications(manager, notificationsDef, localizationProvider));

            var buttons = await resolver.Get<ICheatButtonsRegistry>();

            buttons.RegisterAction("Schedule All Notifications", () => ScheduleAllNotifications());

            Atom.Reaction(lifetime,
                () => gameProperties.Get(SettingsGameProperties.PushNotifications),
                v => GameNotificationsManager.NotificationsEnabled = v
            );

            GameNotificationChannel CreateChannel(string key) {
                var channelName = localizationProvider.LocalizeChannelName(key);
                var channelDesc = localizationProvider.LocalizeChannelDescription(key);
                return new GameNotificationChannel(key, channelName, channelDesc);
            }
        }

        private static void ScheduleAllNotifications() {
            var notifications     = App.Get<LookupCollection<GameNotificationDef>>();
            var gameNotifications = App.Notifications;

            var time = DateTime.Now;

            foreach (var notification in notifications.Items) {
                if (!notification.enabled) {
                    continue;
                }

                gameNotifications.CancelNotification(notification.key);
                gameNotifications.ScheduleNotification(notification.key, time);

                time = time.AddMinutes(1);
            }
        }
    }
}