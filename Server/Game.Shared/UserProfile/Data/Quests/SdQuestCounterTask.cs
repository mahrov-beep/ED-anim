namespace Game.Shared.UserProfile.Data.Quests {
    using Multicast.ServerData;

    public class SdQuestCounterTask : SdArrayObject {
        public string Key => this.GetSdObjectKey();

        public SdValue<int> Counter { get; }

        public SdQuestCounterTask(SdArgs args) : base(args) {
            this.Counter = new SdValue<int>(this.Child(0), -1);
        }
    }
}