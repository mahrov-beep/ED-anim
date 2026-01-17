namespace Multicast.Modules.IapValidation.RevenueCat {
    using Multicast;

    public readonly struct RevenueCatRemoveValidationPurchaseCommand : ICommand {
        public string StoreSpecificId { get; }
        public string Receipt         { get; }

        public RevenueCatRemoveValidationPurchaseCommand(string storeSpecificId, string receipt) {
            this.StoreSpecificId = storeSpecificId;
            this.Receipt         = receipt;
        }
    }

    [SkipInstallWithoutDependency(typeof(SdkInitializationMarkers.RevenueCat))]
    public class RevenueCatRemoveValidationPurchaseCommandHandler : ICommandHandler<RevenueCatRemoveValidationPurchaseCommand> {
        [Inject] private readonly UdRevenueCatValidationRepo validation;

        public void Execute(CommandContext context, RevenueCatRemoveValidationPurchaseCommand command) {
            this.validation.RemovePurchase(command.StoreSpecificId, command.Receipt);
        }
    }
}