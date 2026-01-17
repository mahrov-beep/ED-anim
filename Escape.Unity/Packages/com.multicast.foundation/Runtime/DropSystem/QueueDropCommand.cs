namespace Multicast.DropSystem {
    using Multicast;

    public readonly struct QueueDropCommand : ICommand {
        public Drop           Drop       { get; }
        public DropSourceType SourceType { get; }
        public string         SourceName { get; }
        public string         SourceKey  { get; }

        public QueueDropCommand(Drop drop, DropSourceType sourceType, string sourceName, string sourceKey) {
            this.Drop       = drop;
            this.SourceType = sourceType;
            this.SourceName = sourceName;
            this.SourceKey  = sourceKey;
        }
    }

    public class QueueDropCommandHandler : ICommandHandler<QueueDropCommand> {
        [Inject] private readonly UdDropRepo dropRepo;

        public void Execute(CommandContext context, QueueDropCommand command) {
            this.dropRepo.CreateInternal(command.Drop, command.SourceType, command.SourceName, command.SourceKey);

            context.RequestUserDataSave();
        }
    }
}