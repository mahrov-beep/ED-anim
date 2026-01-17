namespace Multicast.Modules.Playtime {
    using Multicast;
    using Multicast.Analytics;
    using Multicast.UserStats;
    using Scellecs.Morpeh;
    using UserStats;

    public class PlayTimeSystem : SystemBase {
        [Inject] private UdUserStatsRepo statsData;

        public override void OnAwake() {
        }

        public override void OnUpdate(float deltaTime) {
            this.statsData.PlaytimeElapsed.Value += deltaTime;

            const float interval = 1f;
            if (this.statsData.PlaytimeElapsed.Value < interval) {
                return;
            }

            this.statsData.PlaytimeElapsed.Value -= interval;
            this.statsData.PlaytimeSeconds.Value += 1;

            if (this.statsData.PlaytimeSeconds.Value % 60 != 0) {
                return;
            }

            App.Execute(new AddPlayTimeMinutesCommand(1));
        }
    }
}