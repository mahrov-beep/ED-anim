namespace Game.Domain.Commands.Purchases {
    using Models.Purchases;
    using Multicast;

    public readonly struct SetStoreItemHasBeenSeenCommand : ICommand {
        public string StoreItemKey { get; }

        public SetStoreItemHasBeenSeenCommand(string storeItemKey) {
            this.StoreItemKey = storeItemKey;
        }
    }

    public class SetStoreItemHasBeenSeenCommandHandler : ICommandHandler<SetStoreItemHasBeenSeenCommand> {
        [Inject] private readonly StoreItemsModel storeItemsModel;

        public void Execute(CommandContext context, SetStoreItemHasBeenSeenCommand command) {
            var storeItem = this.storeItemsModel.Get(command.StoreItemKey);

            storeItem.Data.HasBeenSeen.Value = true;
        }
    }
}