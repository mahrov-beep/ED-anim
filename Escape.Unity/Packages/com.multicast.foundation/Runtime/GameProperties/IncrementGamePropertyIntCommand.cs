namespace Multicast.GameProperties {
    using Multicast;

    public readonly struct IncrementGamePropertyIntCommand : ICommand {
        public IntGamePropertyName Name        { get; }
        public int                 AmountToAdd { get; }

        public IncrementGamePropertyIntCommand(IntGamePropertyName name, int amountToAdd) {
            this.Name        = name;
            this.AmountToAdd = amountToAdd;
        }
    }

    public class IncrementGamePropertyIntCommandHandler : ICommandHandler<IncrementGamePropertyIntCommand> {
        [Inject] private readonly UdGamePropertiesData data;

        public void Execute(CommandContext context, IncrementGamePropertyIntCommand command) {
            this.data.IncrementInt(command.Name, command.AmountToAdd);
            context.Execute(new SyncGamePropertiesCommand());
        }
    }
}