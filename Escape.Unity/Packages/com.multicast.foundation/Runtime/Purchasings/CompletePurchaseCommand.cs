namespace Multicast.Purchasing {
    using System;
    using Collections;
    using DropSystem;
    using Multicast;

    [Serializable, RequireFieldsInit] public struct CompletePurchaseCommand : ICommand {
        public string purchaseGuid;
        public string transactionId;
    }

    [SkipInstallWithoutDependency(typeof(IPurchasing))]
    public class CompletePurchaseCommandHandler : ICommandHandler<CompletePurchaseCommand> {
        [Inject] private readonly UdPurchasesRepo purchasesRepo;

        public void Execute(CommandContext context, CompletePurchaseCommand command) {
            var purchaseData = this.purchasesRepo.Get(command.purchaseGuid);

            purchaseData.Status.Value        = UdPurchaseStatus.COMPLETED;
            purchaseData.TransactionId.Value = command.transactionId;

            foreach (var drop in purchaseData.Drops) {
                context.Execute(new QueueDropCommand(drop, DropSourceType.Iap, "purchase", purchaseData.PurchaseKey.Value));
            }
        }
    }
}