namespace Multicast.Purchasing {
    using System;
    using Multicast;

    [Serializable, RequireFieldsInit] public struct CancelPurchaseCommand : ICommand {
        public string purchaseGuid;
    }

    [SkipInstallWithoutDependency(typeof(IPurchasing))]
    public class CancelPurchaseCommandHandler : ICommandHandler<CancelPurchaseCommand> {
        [Inject] private readonly UdPurchasesRepo purchasesRepo;

        public void Execute(CommandContext context, CancelPurchaseCommand command) {
            var purchaseData = this.purchasesRepo.Get(command.purchaseGuid);

            purchaseData.Status.Value = UdPurchaseStatus.CANCELLED;
        }
    }
}