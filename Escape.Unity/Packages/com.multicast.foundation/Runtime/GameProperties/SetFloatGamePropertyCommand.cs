namespace Multicast.GameProperties {
    using Multicast;

    public readonly struct SetFloatGamePropertyCommand : ICommand {
        public FloatGamePropertyName Name  { get; }
        public float                 Value { get; }

        public SetFloatGamePropertyCommand(FloatGamePropertyName name, float value) {
            this.Name  = name;
            this.Value = value;
        }
    }

    public class SetFloatGamePropertyCommandHandler : ICommandHandler<SetFloatGamePropertyCommand> {
        [Inject] private readonly UdGamePropertiesData data;

        public void Execute(CommandContext context, SetFloatGamePropertyCommand command) {
            this.data.SetFloat(command.Name, command.Value);
            context.Execute(new SyncGamePropertiesCommand());
        }
    }
}