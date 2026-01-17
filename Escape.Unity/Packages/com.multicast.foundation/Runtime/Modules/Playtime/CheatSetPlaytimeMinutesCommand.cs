namespace Multicast.Modules.Playtime {
    using Multicast;
    using Multicast.UserStats;

    internal readonly struct CheatSetPlaytimeMinutesCommand : ICommand {
        public int Minutes { get; }

        public CheatSetPlaytimeMinutesCommand(int minutes) {
            this.Minutes = minutes;
        }
    }

    internal class CheatSetPlaytimeMinutesCommandHandler : ICommandHandler<CheatSetPlaytimeMinutesCommand> {
        [Inject] private UdUserStatsRepo userStatsRepo;

        public void Execute(CommandContext context, CheatSetPlaytimeMinutesCommand command) {
            this.userStatsRepo.PlaytimeMinutes.Value = command.Minutes;
        }
    }
}