namespace Multicast.Advertising {
    using Cysharp.Threading.Tasks;

    public interface IAdvertising {
        bool IsInterstitialAvailable { get; }
        bool IsRewardedAvailable     { get; }

        UniTask<AdResult> ShowRewarded(string placement);
        UniTask<AdResult> ShowInterstitial(string placement);

        void CacheRewarded();
        void CacheInterstitial();
    }
}