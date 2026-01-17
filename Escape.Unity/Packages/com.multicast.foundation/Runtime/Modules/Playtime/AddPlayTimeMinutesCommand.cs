namespace Multicast.Modules.Playtime {
    using GameProperties;
    using Multicast.Analytics;
    using Multicast.UserStats;
    using UserStats;

    internal readonly struct AddPlayTimeMinutesCommand : ICommand {
        public int PlayTime { get; }

        public AddPlayTimeMinutesCommand(int playTime) {
            this.PlayTime = playTime;
        }
    }

    internal class AddPlayTimeMinutesCommandHandler : ICommandHandler<AddPlayTimeMinutesCommand> {
        [Inject] private UdUserStatsRepo statsRepo;
        [Inject] private IAnalytics      analytics;

        public void Execute(CommandContext context, AddPlayTimeMinutesCommand command) {
            this.statsRepo.PlaytimeMinutes.Value += command.PlayTime;

            this.analytics.Send(new PlayTimeAnalyticsEvent(this.statsRepo.PlaytimeMinutes.Value));

            context.Execute(new SyncGamePropertiesCommand());
        }
    }
}