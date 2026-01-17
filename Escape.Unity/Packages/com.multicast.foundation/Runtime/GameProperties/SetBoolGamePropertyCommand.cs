namespace Multicast.GameProperties {
    using Multicast;

    public readonly struct SetBoolGamePropertyCommand : ICommand {
        public BoolGamePropertyName Name  { get; }
        public bool                 Value { get; }

        public SetBoolGamePropertyCommand(BoolGamePropertyName name, bool value) {
            this.Name  = name;
            this.Value = value;
        }
    }

    public class SetBoolGamePropertyCommandHandler : ICommandHandler<SetBoolGamePropertyCommand> {
        [Inject] private readonly UdGamePropertiesData data;

        public void Execute(CommandContext context, SetBoolGamePropertyCommand command) {
            this.data.SetBool(command.Name, command.Value);
            context.Execute(new SyncGamePropertiesCommand());
        }
    }
}