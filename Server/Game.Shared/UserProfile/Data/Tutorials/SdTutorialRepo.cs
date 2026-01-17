namespace Game.Shared.UserProfile.Data.Tutorials {
    using Multicast.ServerData;

    public class SdTutorialRepo : SdRepo<SdTutorial> {
        public SdTutorialRepo(SdArgs args) : base(args, a => new SdTutorial(a)) {
        }

        public bool IsCompleted(string tutorialKey) {
            return this.Lookup.TryGetValue(tutorialKey, out var sdTutorial) && sdTutorial.Completed.Value;
        }

        public void SetCompleted(string tutorialKey) {
            var sdTutorial = this.Lookup.GetOrCreate(tutorialKey, out _);

            sdTutorial.Completed.Value = true;
        }
    }
}