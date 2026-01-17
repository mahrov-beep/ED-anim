namespace Game.Shared.UserProfile.Data.MailBox {
    using Multicast.ServerData;

    public class SdMailBox : SdObject {
        public SdDict<SdMailBoxMessage> Messages { get; }

        public SdMailBox(SdArgs args) : base(args) {
            this.Messages = new SdDict<SdMailBoxMessage>(this.Child(0), a => new SdMailBoxMessage(a));
        }
    }
}