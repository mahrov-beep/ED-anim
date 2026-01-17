namespace Multicast.Modules.Advertising.Dummy {
    using Cysharp.Threading.Tasks;
    using Multicast.Advertising;
    using Scellecs.Morpeh;

    public class DummyAdvertising : IAdvertising {
        private readonly World world;

        public bool IsInterstitialAvailable => true;
        public bool IsRewardedAvailable     => true;

        public DummyAdvertising(World world) {
            this.world = world;
        }

        public void Initialize() {
        }

        public UniTask<AdResult> ShowRewarded(string placement) {
            this.SendImpressionData(AdType.Rewarded);

            return UniTask.FromResult(AdResult.Completed("dummy", "dummy_ad_unit"));
        }

        public UniTask<AdResult> ShowInterstitial(string placement) {
            this.SendImpressionData(AdType.Interstitial);

            return UniTask.FromResult(AdResult.Completed("dummy", "dummy_ad_unit"));
        }

        public void CacheRewarded() {
        }

        public void CacheInterstitial() {
        }

        private void SendImpressionData(AdType type) {
            this.world.GetEvent<AdImpressionEvent>().NextFrame(new AdImpressionEvent {
                type      = type,
                placement = "dummy",
                revenue   = 0.1,
                adNetwork = "dummy",
                adUnit    = "dummy",
            });
        }
    }
}