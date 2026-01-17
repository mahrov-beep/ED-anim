namespace Game.Shared.UserProfile.Data.Quests {
    using Multicast.ServerData;

    public class SdQuestDonateItemTask : SdArrayObject {
        public SdEnumValue<SdQuestDonateItemTaskStates> State { get; }

        public SdQuestDonateItemTask(SdArgs args) : base(args) {
            this.State = new SdEnumValue<SdQuestDonateItemTaskStates>(this.Child(0), SdQuestDonateItemTaskStates.Locked);
        }
    }

    public enum SdQuestDonateItemTaskStates {
        Locked    = 0,
        Revealed  = 1,
        Completed = 2,
    }
}