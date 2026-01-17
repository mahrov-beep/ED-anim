namespace Multicast.Modules.IapValidation.RevenueCat {
    using Multicast;

    public readonly struct RevenueCatAddValidationPurchaseCommand : ICommand {
        public string StoreSpecificId { get; }
        public string Receipt         { get; }

        public RevenueCatAddValidationPurchaseCommand(string storeSpecificId, string receipt) {
            this.StoreSpecificId = storeSpecificId;
            this.Receipt         = receipt;
        }
    }

    [SkipInstallWithoutDependency(typeof(SdkInitializationMarkers.RevenueCat))]
    public class RevenueCatAddValidationPurchaseCommandHandler : ICommandHandler<RevenueCatAddValidationPurchaseCommand> {
        [Inject] private readonly UdRevenueCatValidationRepo validation;

        public void Execute(CommandContext context, RevenueCatAddValidationPurchaseCommand command) {
            this.validation.AddPurchase(command.StoreSpecificId, command.Receipt);
        }
    }
}