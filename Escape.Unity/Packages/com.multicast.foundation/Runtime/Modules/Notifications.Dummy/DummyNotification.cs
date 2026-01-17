namespace Multicast.Modules.Notifications.Dummy {
    using System;
    using Multicast.Notifications;

    internal class DummyNotification : IGameNotification {
        public int?      Id               { get; set; }
        public string    Title            { get; set; }
        public string    Body             { get; set; }
        public string    Subtitle         { get; set; }
        public string    Data             { get; set; }
        public string    Group            { get; set; }
        public int?      BadgeNumber      { get; set; }
        public bool      ShouldAutoCancel { get; set; }
        public DateTime? DeliveryTime     { get; set; }
        public bool      Scheduled        { get; set; }
        public string    SmallIcon        { get; set; }
        public string    LargeIcon        { get; set; }
    }
}