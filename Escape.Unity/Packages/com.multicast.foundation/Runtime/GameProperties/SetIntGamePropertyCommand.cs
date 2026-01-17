namespace Multicast.GameProperties {
    using Multicast;

    public readonly struct SetIntGamePropertyCommand : ICommand {
        public IntGamePropertyName Name  { get; }
        public int                 Value { get; }

        public SetIntGamePropertyCommand(IntGamePropertyName name, int value) {
            this.Name  = name;
            this.Value = value;
        }
    }

    public class SetIntGamePropertyCommandHandler : ICommandHandler<SetIntGamePropertyCommand> {
        [Inject] private readonly UdGamePropertiesData data;

        public void Execute(CommandContext context, SetIntGamePropertyCommand command) {
            this.data.SetInt(command.Name, command.Value);
            context.Execute(new SyncGamePropertiesCommand());
        }
    }
}