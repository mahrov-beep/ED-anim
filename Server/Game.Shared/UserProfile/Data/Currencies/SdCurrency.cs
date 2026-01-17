namespace Game.Shared.UserProfile.Data.Currencies {
    using Multicast.ServerData;

    public class SdCurrency : SdArrayObject {
        public SdValue<int> Amount { get; }

        public SdCurrency(SdArgs args) : base(args) {
            this.Amount = this.Child(0);
        }
    }
}