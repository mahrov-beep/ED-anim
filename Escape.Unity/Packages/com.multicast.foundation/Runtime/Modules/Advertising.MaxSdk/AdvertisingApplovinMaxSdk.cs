#if APPLOVIN_MAX_SDK
namespace Multicast.Modules.Advertising.MaxSdk {
    using System;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using GameProperties;
    using JetBrains.Annotations;
    using Multicast.Advertising;
    using Multicast.Analytics;
    using Scellecs.Morpeh;
    using UniMob;
    using UnityEngine;
    using MaxSdk = global::MaxSdk;
    using Object = UnityEngine.Object;

    public class AdvertisingApplovinMaxSdk : IAdvertising, ILifetimeScope {
        private readonly Atom<int> version;

        [Atom] public bool IsInterstitialAvailable {
            get {
                this.version.Get();

                if (this.IsAdShown()) {
                    return false;
                }

                if (!this.IsInitialized) {
                    return false;
                }

                if (!MaxSdk.IsInterstitialReady(this.CurrentInterAdUnit)) {
                    return false;
                }

                return true;
            }
        }

        [Atom] public bool IsRewardedAvailable {
            get {
                this.version.Get();

                if (this.IsAdShown()) {
                    return false;
                }

                if (!this.IsInitialized) {
                    return false;
                }

                var available = this.gameProperties.Get(MaxSdkGameProperties.RewardedAlwaysAvailable) ||
                                MaxSdk.IsRewardedAdReady(this.CurrentRewardedAdUnit);

                if (!available) {
                    return false;
                }

                return true;
            }
        }

        [Atom] public bool IsInitialized { get; set; }

        public int InterRetryAttempt  { get; private set; }
        public int RewardRetryAttempt { get; private set; }

        public Lifetime Lifetime => this.lifetime;

        private readonly Lifetime              lifetime;
        private readonly IAnalytics            analytics;
        private readonly GamePropertiesModel   gameProperties;
        private readonly MaxSdkAdConfiguration config;
        private readonly World                 world;

        private bool isInterstitialLoadingOrLoaded;
        private bool isRewardedLoadingOrLoaded;

        [CanBeNull] private volatile AdInfo currentInterstitial;
        [CanBeNull] private volatile AdInfo currentRewarded;

        public AdvertisingApplovinMaxSdk(Lifetime lifetime, IAnalytics analytics, GamePropertiesModel gameProperties, MaxSdkAdConfiguration config, World world) {
            this.lifetime       = lifetime;
            this.analytics      = analytics;
            this.gameProperties = gameProperties;
            this.config         = config;
            this.world          = world;
            this.version        = Atom.Value(lifetime, 0);
        }

        public string CurrentRewardedAdUnit => this.config.RewardedAdUnitOverride?.AdUnit ?? this.config.Rewarded;
        public string CurrentInterAdUnit    => this.config.Inter;

        public async UniTask InitializeAsync() {
            try {
                MaxSdk.SetSdkKey(this.config.SdkKey);
                MaxSdk.SetHasUserConsent(true);
                MaxSdk.SetDoNotSell(false);
                MaxSdk.InitializeSdk();

                while (!MaxSdk.IsInitialized()) {
                    await UniTask.NextFrame(this.lifetime);
                }

                this.IsInitialized = true;

                this.SubscribeCallbacks();
                this.RefreshAdAvailability();

                this.CacheRewarded();
                this.CacheInterstitial();

                this.CreateAdMonoUpdater();

                var context = SynchronizationContext.Current;
                MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += (s, data) =>
                    context.Post(_ => this.OnRewardedAdRevenuePaid(s, data), null);

                MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += (s, data) =>
                    context.Post(_ => this.analytics.Send(new RewardedAdHiddenAnalyticsEvent()), null);

                MaxSdkCallbacks.AppOpen.OnAdHiddenEvent += (s, data) =>
                    context.Post(_ => this.analytics.Send(new AppOpenAdHiddenAnalyticsEvent()), null);
            }
            catch (Exception e) {
                Debug.LogException(e);
            }
            finally {
                this.RefreshAdAvailability();
            }
        }

        private void CreateAdMonoUpdater() {
            var go = new GameObject(nameof(MonoAdUpdater));
            Object.DontDestroyOnLoad(go);
            var updater = go.AddComponent<MonoAdUpdater>();
            updater.Advertising = this;

            this.Lifetime.Register(() => Object.Destroy(go));
        }

        public async UniTask<AdResult> ShowRewarded(string placement) {
            if (this.currentRewarded != null) {
                return AdResult.NotAvailable("currentRewarded not null");
            }

            if (this.currentInterstitial != null) {
                return AdResult.NotAvailable("currentInterstitial not null");
            }

            if (!MaxSdk.IsRewardedAdReady(this.CurrentRewardedAdUnit)) {
                return AdResult.NotAvailable("NO_FILL");
            }

            this.currentRewarded = AdInfo.Create(placement);

            this.RefreshAdAvailability();

            await UniTask.NextFrame();

            try {
                MaxSdk.ShowRewardedAd(this.CurrentRewardedAdUnit, placement);

                while (true) {
                    await UniTask.NextFrame();

                    if (this.currentRewarded == null) {
                        return AdResult.NotAvailable("ERROR");
                    }

                    if (this.currentRewarded.error != null) {
                        return AdResult.NotAvailable(this.currentRewarded.error);
                    }

                    if (this.currentRewarded.closed) {
                        if (!this.currentRewarded.succeed) {
                            await UniTask.DelayFrame(5);
                        }

                        if (this.currentRewarded.succeed) {
                            return AdResult.Completed(this.currentRewarded.adNetwork, this.currentRewarded.adUnitId);
                        }

                        return AdResult.Canceled(this.currentRewarded.adNetwork);
                    }
                }
            }
            catch (Exception e) {
                Debug.LogException(e);
                return AdResult.NotAvailable("ERROR");
            }
            finally {
                this.currentRewarded = null;
                this.config.RewardedAdUnitOverride?.OnAdClosed();
                this.RefreshAdAvailability();
            }
        }

        public async UniTask<AdResult> ShowInterstitial(string placement) {
            if (this.currentRewarded != null) {
                return AdResult.NotAvailable("currentRewarded not null");
            }

            if (this.currentInterstitial != null) {
                return AdResult.NotAvailable("currentInterstitial not null");
            }

            if (!MaxSdk.IsInterstitialReady(this.CurrentInterAdUnit)) {
                return AdResult.NotAvailable("NO_FILL");
            }

            this.currentInterstitial = AdInfo.Create(placement);

            this.RefreshAdAvailability();

            await UniTask.NextFrame();

            try {
                MaxSdk.ShowInterstitial(this.CurrentInterAdUnit, placement);

                while (true) {
                    await UniTask.NextFrame();
                    if (this.currentInterstitial == null) {
                        return AdResult.NotAvailable("ERROR");
                    }

                    if (this.currentInterstitial.error != null) {
                        return AdResult.NotAvailable(this.currentInterstitial.error);
                    }

                    if (this.currentInterstitial.closed) {
                        return AdResult.Completed(this.currentInterstitial.adNetwork, this.currentInterstitial.adUnitId);
                    }
                }
            }
            catch (Exception e) {
                Debug.LogException(e);
                return AdResult.NotAvailable("ERROR");
            }
            finally {
                this.currentInterstitial = null;
                this.RefreshAdAvailability();
            }
        }

        public void CacheRewarded() {
            if (this.isRewardedLoadingOrLoaded) {
                return;
            }

            if (this.IsAdShown()) {
                return;
            }

            if (MaxSdk.IsRewardedAdReady(this.CurrentRewardedAdUnit)) {
                return;
            }

            this.isRewardedLoadingOrLoaded = true;
            MaxSdk.LoadRewardedAd(this.CurrentRewardedAdUnit);
        }

        public void CacheInterstitial() {
            if (this.isInterstitialLoadingOrLoaded) {
                return;
            }

            if (MaxSdk.IsInterstitialReady(this.CurrentInterAdUnit)) {
                return;
            }

            this.isInterstitialLoadingOrLoaded = true;
            MaxSdk.LoadInterstitial(this.CurrentInterAdUnit);
        }

        internal void RefreshAdAvailability() {
            this.version.Invalidate();
        }

        private bool IsAdShown() => this.currentInterstitial != null || this.currentRewarded != null;

        private void SubscribeCallbacks() {
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent        += this.OnInterstitialLoadedEvent;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent    += this.OnInterstitialLoadFailedEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent     += this.OnInterstitialDisplayedEvent;
            MaxSdkCallbacks.Interstitial.OnAdClickedEvent       += this.OnInterstitialClickedEvent;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent        += this.OnInterstitialHiddenEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += this.OnInterstitialAdFailedToDisplayEvent;

            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent         += this.OnRewardedAdLoadedEvent;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent     += this.OnRewardedAdLoadFailedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent      += this.OnRewardedAdDisplayedEvent;
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent        += this.OnRewardedAdClickedEvent;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent         += this.OnRewardedAdHiddenEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent  += this.OnRewardedAdFailedToDisplayEvent;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += this.OnRewardedAdReceivedRewardEvent;
        }

        #region Callbacks

        private void OnRewardedAdRevenuePaid(string s, MaxSdkBase.AdInfo data) {
            this.world.GetEvent<AdImpressionEvent>().NextFrame(new AdImpressionEvent {
                type      = AdType.Rewarded,
                placement = data.AdFormat,
                revenue   = data.Revenue,
                adNetwork = data.NetworkName,
                adUnit    = data.AdUnitIdentifier,
            });

            this.world.GetEvent<RevenuePaidEvent>().NextFrame(new RevenuePaidEvent {
                AdUnitIdentifier   = data.AdUnitIdentifier,
                AdFormat           = data.AdFormat,
                NetworkName        = data.NetworkName,
                NetworkPlacement   = data.NetworkPlacement,
                Placement          = data.Placement,
                CreativeIdentifier = data.CreativeIdentifier,
                Revenue            = data.Revenue,
            });
        }

        private void OnRewardedAdReceivedRewardEvent(string unitId, MaxSdkBase.Reward reward, MaxSdkBase.AdInfo info) {
            if (this.currentRewarded != null) {
                this.currentRewarded.succeed = true;
            }
        }

        private void OnRewardedAdFailedToDisplayEvent(string unitId, MaxSdkBase.ErrorInfo error, MaxSdkBase.AdInfo info) {
            if (this.currentRewarded != null) {
                this.currentRewarded.error = error.Message;
            }
        }

        private void OnRewardedAdHiddenEvent(string unitId, MaxSdkBase.AdInfo info) {
            if (this.currentRewarded != null) {
                this.currentRewarded.adNetwork = info.NetworkName;
                this.currentRewarded.adUnitId  = info.AdUnitIdentifier;
                this.currentRewarded.closed    = true;
            }

            this.isRewardedLoadingOrLoaded = false;
        }

        private void OnRewardedAdClickedEvent(string unitId, MaxSdkBase.AdInfo info) {
        }

        private void OnRewardedAdDisplayedEvent(string unitId, MaxSdkBase.AdInfo info) {
        }

        private void OnRewardedAdLoadFailedEvent(string unitId, MaxSdkBase.ErrorInfo error) {
            this.isRewardedLoadingOrLoaded = false;
            this.RefreshAdAvailability();
            this.RewardRetryAttempt++;
        }

        private void OnRewardedAdLoadedEvent(string unitId, MaxSdkBase.AdInfo info) {
            this.isRewardedLoadingOrLoaded = true;
            this.RefreshAdAvailability();
            this.RewardRetryAttempt = 0;
        }

        private void OnInterstitialAdFailedToDisplayEvent(string unitId, MaxSdkBase.ErrorInfo error, MaxSdkBase.AdInfo info) {
            if (this.currentInterstitial != null) {
                this.currentInterstitial.error = error.Message;
            }
        }

        private void OnInterstitialHiddenEvent(string unitId, MaxSdkBase.AdInfo info) {
            if (this.currentInterstitial != null) {
                this.currentInterstitial.adNetwork = info.NetworkName;
                this.currentInterstitial.adUnitId  = info.AdUnitIdentifier;
                this.currentInterstitial.closed    = true;
            }

            this.isInterstitialLoadingOrLoaded = false;
        }

        private void OnInterstitialClickedEvent(string unitId, MaxSdkBase.AdInfo info) {
        }

        private void OnInterstitialDisplayedEvent(string unitId, MaxSdkBase.AdInfo info) {
        }

        private void OnInterstitialLoadFailedEvent(string unitId, MaxSdkBase.ErrorInfo error) {
            this.isInterstitialLoadingOrLoaded = false;

            this.InterRetryAttempt++;
        }

        private void OnInterstitialLoadedEvent(string unitId, MaxSdkBase.AdInfo info) {
            this.isInterstitialLoadingOrLoaded = true;

            this.InterRetryAttempt = 0;
        }

        #endregion

        public void ShowDebugger() {
            MaxSdk.ShowMediationDebugger();
        }
    }
}
#endif