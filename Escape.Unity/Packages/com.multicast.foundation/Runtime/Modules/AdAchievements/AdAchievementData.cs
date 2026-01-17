namespace Multicast.Modules.AdAchievements {
    using Multicast.UserData;

    public class AdAchievementData : UdObject {
        public UdValue<bool> WasSent { get; }

        public AdAchievementData(UdArgs args) : base(args) {
            this.WasSent = this.Child("was_sent");
        }
    }
}