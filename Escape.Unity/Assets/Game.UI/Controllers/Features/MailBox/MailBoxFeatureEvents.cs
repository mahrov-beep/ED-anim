namespace Game.UI.Controllers.Features.MailBox {
    using Multicast;

    public static class MailBoxFeatureEvents {
        public static readonly EventSource              Open    = new();
        public static readonly EventSource<CollectArgs> Collect = new();

        [RequireFieldsInit]
        public struct CollectArgs {
            public string MessageGuid;
        }
    }
}