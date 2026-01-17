namespace Multicast.Purchasing {
    using System;
    using System.Collections.Generic;
    using DropSystem;
    using Multicast;

    [Serializable, RequireFieldsInit] public struct InitiatePurchaseCommand : ICommand<string> {
        public string     purchaseKey;
        public string     itemKey;
        public int        priceCents;
        public string     iapCurrencyCode;
        public double     iapCurrencyAmount;
        public List<Drop> drops;
    }

    [SkipInstallWithoutDependency(typeof(IPurchasing))]
    public class InitiatePurchaseCommandHandler : ICommandHandler<InitiatePurchaseCommand, string> {
        [Inject] private UdPurchasesRepo purchasesRepo;
        [Inject] private ITimeService    timeService;

        public string Execute(CommandContext context, InitiatePurchaseCommand command) {
            var guid = Guid.NewGuid().ToString();

            var purchaseData = this.purchasesRepo.Lookup.Create(guid);

            purchaseData.PurchaseKey.Value       = command.purchaseKey;
            purchaseData.ItemKey.Value           = command.itemKey;
            purchaseData.Status.Value            = UdPurchaseStatus.INITIATED;
            purchaseData.PurchaseDate.Value      = this.timeService.Now;
            purchaseData.PriceCents.Value        = command.priceCents;
            purchaseData.IapCurrencyCode.Value   = command.iapCurrencyCode;
            purchaseData.IapCurrencyAmount.Value = command.iapCurrencyAmount;

            foreach (var drop in command.drops) {
                purchaseData.Drops.Add(drop);
            }

            return guid;
        }
    }
}