namespace Multicast.Purchasing {
    using System;
    using Multicast;

    [Serializable, RequireFieldsInit] public struct FailPurchaseCommand : ICommand {
        public string purchaseGuid;
        public string failMessage;
    }

    [SkipInstallWithoutDependency(typeof(IPurchasing))]
    public class FailPurchaseCommandHandler : ICommandHandler<FailPurchaseCommand> {
        [Inject] private readonly UdPurchasesRepo purchasesRepo;

        public void Execute(CommandContext context, FailPurchaseCommand command) {
            var purchaseData = this.purchasesRepo.Get(command.purchaseGuid);

            purchaseData.Status.Value      = UdPurchaseStatus.FAILED;
            purchaseData.FailMessage.Value = command.failMessage;
        }
    }
}