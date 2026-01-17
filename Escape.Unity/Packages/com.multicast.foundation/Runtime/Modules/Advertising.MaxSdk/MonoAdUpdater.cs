#if APPLOVIN_MAX_SDK
namespace Multicast.Modules.Advertising.MaxSdk {
    using System.Collections;
    using UnityEngine;

    public class MonoAdUpdater : MonoBehaviour {
        public AdvertisingApplovinMaxSdk Advertising { get; set; }

        private const float MAX_AD_UPDATE_DELAY = 1F;

        private void Start() {
            this.StartCoroutine(this.UpdateAvailability());
            this.StartCoroutine(this.UpdateReward());
            this.StartCoroutine(this.UpdateInter());
        }

        private IEnumerator UpdateReward() {
            while (this.enabled) {
                if (!this.Advertising.IsRewardedAvailable) {
                    this.Advertising.CacheRewarded();
                    yield return new WaitForSecondsRealtime(Mathf.Pow(2, Mathf.Min(MAX_AD_UPDATE_DELAY, this.Advertising.RewardRetryAttempt)));
                }

                yield return null;
            }
        }

        private IEnumerator UpdateInter() {
            while (this.enabled) {
                if (!this.Advertising.IsInterstitialAvailable) {
                    this.Advertising.CacheInterstitial();
                    yield return new WaitForSecondsRealtime(Mathf.Pow(2, Mathf.Min(MAX_AD_UPDATE_DELAY, this.Advertising.InterRetryAttempt)));
                }

                yield return null;
            }
        }

        private IEnumerator UpdateAvailability() {
            while (this.enabled) {
                this.Advertising.RefreshAdAvailability();
                yield return new WaitForSecondsRealtime(1f);
            }
        }
    }
}
#endif