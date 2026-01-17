namespace Multicast.Purchasing {
    using System.Linq;
    using DropSystem;
    using JetBrains.Annotations;
    using Numerics;
    using UserData;

    public class UdPurchasesRepo : UdRepo<UdPurchase> {
        public UdPurchasesRepo(UdArgs args) : base(args, a => new UdPurchase(a)) {
        }

        [PublicAPI] public long TotalPurchasesCount() => this.Lookup
            .Count(purchase => purchase.Status.Value == UdPurchaseStatus.COMPLETED);

        [PublicAPI] public double TotalUsdSpent() => this.Lookup
            .Where(purchase => purchase.Status.Value == UdPurchaseStatus.COMPLETED)
            .Select(purchase => purchase.PriceCents.Value)
            .Sum() / 100.0;

        [PublicAPI] public int GetPurchasesCountByPurchaseKey(string purchaseKey) => this.Lookup
            .Where(purchase => purchase.Status.Value == UdPurchaseStatus.COMPLETED)
            .Count(purchase => purchase.PurchaseKey.Value == purchaseKey);

        [PublicAPI] public int GetPurchasesCountByItemKey(string itemKey) => this.Lookup
            .Where(purchase => purchase.Status.Value == UdPurchaseStatus.COMPLETED)
            .Count(purchase => purchase.ItemKey.Value == itemKey);
    }

    public class UdPurchase : UdObject {
        public UdValue<string>   PurchaseKey  { get; }
        public UdValue<string>   ItemKey      { get; }
        public UdListValue<Drop> Drops        { get; }
        public UdValue<string>   Status       { get; }
        public UdValue<GameTime> PurchaseDate { get; }

        public UdValue<int> PriceCents { get; }

        public UdValue<string> IapCurrencyCode   { get; }
        public UdValue<double> IapCurrencyAmount { get; }

        public UdValue<string> TransactionId { get; }
        public UdValue<string> FailMessage   { get; }

        public UdPurchase(UdArgs args) : base(args) {
            this.PurchaseKey       = this.Child("purchase_key");
            this.ItemKey           = this.Child("item_key");
            this.Drops             = this.Child("drops");
            this.Status            = this.Child("status");
            this.PurchaseDate      = this.Child("date");
            this.PriceCents        = this.Child("price");
            this.IapCurrencyCode   = this.Child("iap_code");
            this.IapCurrencyAmount = this.Child("iap_amount");
            this.TransactionId     = this.Child("tid");
            this.FailMessage       = this.Child("fail_message");
        }
    }

    public static class UdPurchaseStatus {
        public const string INITIATED = "Initiated";
        public const string CANCELLED = "Cancelled";
        public const string COMPLETED = "Completed";
        public const string FAILED    = "Failed";
    }
}