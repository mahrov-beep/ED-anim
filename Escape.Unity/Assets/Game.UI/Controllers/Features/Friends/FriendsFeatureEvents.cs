namespace Game.UI.Controllers.Features.Friends {
    using Multicast;

    public static class FriendsFeatureEvents {
        public static readonly EventSource Open         = new();
        public static readonly EventSource OpenIncoming = new();
    }
}