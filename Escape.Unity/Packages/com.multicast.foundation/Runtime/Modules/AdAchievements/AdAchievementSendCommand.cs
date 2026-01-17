namespace Multicast.Modules.AdAchievements {
    using Multicast;

    public readonly struct AdAchievementSendCommand : ICommand {
        public string AdAchievementKey { get; }

        public AdAchievementSendCommand(string adAchievementKey) {
            this.AdAchievementKey = adAchievementKey;
        }
    }

    [SkipInstallWithoutDependency(typeof(AdAchievementsModel))]
    public class AdAchievementSendCommandHandler : ICommandHandler<AdAchievementSendCommand> {
        [Inject] private readonly UdAdAchievementsRepo adAchievementsData;

        public void Execute(CommandContext context, AdAchievementSendCommand command) {
            var achievementData = this.adAchievementsData.Get(command.AdAchievementKey);

            achievementData.WasSent.Value = true;
        }
    }
}