namespace Multicast.Modules.AdAchievements {
    using Multicast.Advertising;
    using Multicast.UserStats;

    public readonly struct AddImpressionDataCommand : ICommand {
        public AdImpressionEvent ImpressionData { get; }

        public AddImpressionDataCommand(AdImpressionEvent impressionData) {
            this.ImpressionData = impressionData;
        }
    }

    public class AddImpressionDataCommandHandler : ICommandHandler<AddImpressionDataCommand> {
        [Inject] private readonly UdUserStatsRepo userStats;

        public void Execute(CommandContext context, AddImpressionDataCommand command) {
            this.userStats.AdRevenue.Value += command.ImpressionData.revenue;
            this.userStats.AdImpressionCount.Value++;
        }
    }
}