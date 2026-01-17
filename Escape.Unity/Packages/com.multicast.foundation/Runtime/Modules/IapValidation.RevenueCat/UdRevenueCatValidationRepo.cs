namespace Multicast.Modules.IapValidation {
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Multicast.UserData;
    using UnityEngine;

    public class UdRevenueCatValidationRepo : UdObject {
        private readonly UdDict<UdReceiptData> receipts;
        private readonly UdValue<string>       validatedTrialReceipt;
        private readonly UdValue<long>         lastSubscriptionSentTicks;

        public UdRevenueCatValidationRepo(UdArgs args) : base(args) {
            this.receipts = new UdDict<UdReceiptData>(this.Child("receipts"), a => new UdReceiptData(a));

            this.validatedTrialReceipt     = this.Child("validated_trial_receipt");
            this.lastSubscriptionSentTicks = this.Child("last_subscription_sent_ticks");
        }

        public bool HasPurchase(string storeSpecificId) {
            if (this.receipts.TryGetValue(storeSpecificId, out var item)) {
                return item.Receipts.Count > 0;
            }

            return false;
        }

        public void AddPurchase(string storeSpecificId, string receipt) {
            if (!this.receipts.TryGetValue(storeSpecificId, out var receiptData)) {
                receiptData = this.receipts.Create(storeSpecificId);
            }

            receiptData.Receipts.Add(receipt);
        }

        public void RemovePurchase(string storeSpecificId, string receipt) {
            if (this.receipts.TryGetValue(storeSpecificId, out var receiptData)) {
                receiptData.Receipts.Remove(receipt);
            }
        }

        public int ReceiptsCount => this.receipts.Sum(x => x.Receipts.Count);

        public string FirstByIdentifier(string storeSpecificId) {
            return this.receipts.Get(storeSpecificId).Receipts.First();
        }

        public string ValidatedTrialReceipts {
            get => this.validatedTrialReceipt.Value;
            set => this.validatedTrialReceipt.Value = value;
        }

        public long LastSubscriptionSentTicks {
            get => this.lastSubscriptionSentTicks.Value;
            set => this.lastSubscriptionSentTicks.Value = value;
        }
    }
}