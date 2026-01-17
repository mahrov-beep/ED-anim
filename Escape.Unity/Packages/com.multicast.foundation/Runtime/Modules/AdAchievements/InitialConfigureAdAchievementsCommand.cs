namespace Multicast.Modules.AdAchievements {
    using Multicast;
    using Collections;

    public readonly struct InitialConfigureAdAchievementsCommand : ICommand {
    }

    [SkipInstallWithoutDependency(typeof(UdAdAchievementsRepo))]
    public class InitialConfigureAdAchievementsCommandHandler : ICommandHandler<InitialConfigureAdAchievementsCommand> {
        [Inject] private readonly LookupCollection<AdAchievementDef> adAchievementsDefs;
        [Inject] private readonly UdAdAchievementsRepo               adAchievementsData;

        public void Execute(CommandContext context, InitialConfigureAdAchievementsCommand command) {
            foreach (var adAchievementDef in this.adAchievementsDefs.Items) {
                if (this.adAchievementsData.Lookup.ContainsKey(adAchievementDef.key)) {
                    continue;
                }

                var adAchievementData = this.adAchievementsData.Lookup.Create(adAchievementDef.key);

                adAchievementData.WasSent.Value = false;
            }
        }
    }
}