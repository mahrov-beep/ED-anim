namespace Game.Shared.UserProfile.Data.Quests {
    using Multicast.Numerics;
    using Multicast.ServerData;

    public class SdQuest : SdArrayObject {
        public string Key => this.GetSdObjectKey();
        
        public SdEnumValue<SdQuestStates> State  { get; }
        public SdValue<Reward>            Reward { get; }

        public SdQuest(SdArgs args) : base(args) {
            this.State  = new SdEnumValue<SdQuestStates>(this.Child(0), SdQuestStates.Locked);
            this.Reward = this.Child(1);
        }
    }

    public enum SdQuestStates : byte {
        Locked    = 0,
        Revealed  = 1,
        Completed = 2,
    }
}