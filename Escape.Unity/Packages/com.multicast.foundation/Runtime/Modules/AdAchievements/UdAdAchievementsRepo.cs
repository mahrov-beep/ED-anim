namespace Multicast.Modules.AdAchievements {
    using Multicast.UserData;

    public class UdAdAchievementsRepo : UdRepo<AdAchievementData> {
        public UdAdAchievementsRepo(UdArgs args) : base(args, a => new AdAchievementData(a)) {
        }
    }
}