namespace Multicast.CoreAchievements {
    using System;
    using JetBrains.Annotations;
    using Multicast;
    using Analytics;
    using Collections;
    using GameProperties;
    using UniMob;

    public class CoreAchievementsModel : Model {
        [Inject] private readonly LookupCollection<CoreAchievementDef> achievementDefs;
        [Inject] private readonly GamePropertiesModel                  gamePropertiesModel;
        [Inject] private readonly IAnalytics                           analytics;

        public CoreAchievementsModel(Lifetime lifetime) : base(lifetime) {
        }

        public override void Initialize() {
            base.Initialize();

            foreach (var achievementDef in this.achievementDefs.Items) {
                bool IsAchievementDone() => this.gamePropertiesModel.Data.TryGetInt(achievementDef.property, out var v) && v >= achievementDef.value;

                void OnAchievementDone(bool done) {
                    if (!done) {
                        return;
                    }

                    this.analytics.Send(new CoreAchievementDoneAnalyticsEvent {
                        Achievement = achievementDef.key,
                    });
                }

                var boolPropertyName = (BoolGamePropertyName) achievementDef.key;

                this.gamePropertiesModel.RegisterAutoSyncedProperty(this.Lifetime, boolPropertyName, IsAchievementDone);

                Atom.Reaction(this.Lifetime, () => this.gamePropertiesModel.Get(boolPropertyName),
                    OnAchievementDone, fireImmediately: false);
            }
        }

        [PublicAPI]
        public string GetProperty(string key) {
            foreach (var achDef in this.achievementDefs.Items) {
                if (achDef.key == key) {
                    return achDef.property;
                }
            }

            throw new InvalidOperationException($"CoreAchievement not exists");
        }

        [PublicAPI]
        public int GetRequiredValue(string key) {
            foreach (var achDef in this.achievementDefs.Items) {
                if (achDef.key == key) {
                    return achDef.value;
                }
            }

            throw new InvalidOperationException($"CoreAchievement not exists");
        }
    }
}