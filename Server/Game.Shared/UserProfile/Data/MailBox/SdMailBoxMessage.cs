namespace Game.Shared.UserProfile.Data.MailBox {
    using Multicast.Numerics;
    using Multicast.ServerData;

    public class SdMailBoxMessage : SdArrayObject {
        public string MessageGuid => this.GetSdObjectKey();

        public SdEnumValue<SdMailBoxMessageTypes> Type { get; }

        public SdValue<Reward>   Reward      { get; }
        public SdValue<GameTime> ReceiveDate { get; }
        public SdValue<bool>     Claimed     { get; }
        public SdValue<bool>     Viewed      { get; }

        public SdMailBoxMessage(SdArgs args) : base(args) {
            this.Type        = this.Child(0);
            this.Reward      = this.Child(1);
            this.ReceiveDate = this.Child(2);
            this.Claimed     = this.Child(3);
            this.Viewed      = this.Child(4);
        }
    }

    public enum SdMailBoxMessageTypes {
        QuestReward,
        LootBoxReward,
        BetaTestReward,
    }
}