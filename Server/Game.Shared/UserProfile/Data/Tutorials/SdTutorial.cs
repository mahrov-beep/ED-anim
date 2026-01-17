namespace Game.Shared.UserProfile.Data.Tutorials {
    using Multicast.ServerData;

    public class SdTutorial : SdArrayObject {
        public string TutorialKey => this.GetSdObjectKey();

        public SdValue<bool> Completed { get; }

        public SdTutorial(SdArgs args) : base(args) {
            this.Completed = this.Child(0);
        }
    }
}