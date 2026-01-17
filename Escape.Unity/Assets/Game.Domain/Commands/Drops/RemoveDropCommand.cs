namespace Game.Domain.Commands {
    using Multicast;
    using UserData;

    public readonly struct RemoveDropCommand : ICommand {
        public string DropGuid { get; }

        public RemoveDropCommand(string dropGuid) {
            this.DropGuid = dropGuid;
        }
    }

    public class RemoveDropCommandHandler : ICommandHandler<RemoveDropCommand> {
        [Inject] private readonly GameData gameData;

        public void Execute(CommandContext context, RemoveDropCommand command) {
            this.gameData.Drops.Dequeue(command.DropGuid);
        }
    }
}