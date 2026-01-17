namespace Multicast.DropSystem {
    using Multicast;

    public readonly struct AddDropCheatCommand : ICommand {
        public Drop Drop { get; }

        public AddDropCheatCommand(Drop drop) {
            this.Drop = drop;
        }
    }

    public class AddDropCheatCommandHandler : ICommandHandler<AddDropCheatCommand> {
        public void Execute(CommandContext context, AddDropCheatCommand command) {
            context.Execute(new QueueDropCommand(command.Drop, DropSourceType.InGame, "cheat", "cheat_console"));
        }
    }
}