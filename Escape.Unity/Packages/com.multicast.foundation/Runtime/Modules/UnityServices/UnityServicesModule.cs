namespace Multicast.Modules.UnityServices {
    using System;
    using System.Threading.Tasks;
    using Cysharp.Threading.Tasks;
    using FeatureToggles;
    using Install;
    using Multicast.Analytics;
    using Sirenix.OdinInspector;
    using UnityEngine;
    using Utilities;
    using Diagnostics;
#if UNITY_SERVICES_CORE
    using global::Unity.Services.Core;
#endif
#if UNITY_SERVICES_AUTH
    using global::Unity.Services.Authentication;
#endif
#if UNITY_REMOTE_CONFIG
    using global::Unity.Services.RemoteConfig;

#endif

    public class UnityServicesModule : ScriptableModule,
        IScriptableModuleWithPriority {
        private const string DEBUG_TIMER_CATEGORY = "unity_services";

        [SerializeField, Required] private string environmentId;

        [SerializeField] private bool isFeatureVariantProvider = true;

        private Task processTask;

#if UNITY_REMOTE_CONFIG
        private TaskCompletionSource<ConfigResponse> configFetchTcs;
#endif

        public int Priority => ScriptableModulePriority.LATE;

        public override void Setup(ModuleSetup module) {
            if (this.isFeatureVariantProvider) {
                module.Provides<IFeatureToggleVariantProvider>();
            }
        }

        public override void PreInstall() {
            base.PreInstall();

#if UNITY_REMOTE_CONFIG
            this.configFetchTcs = new TaskCompletionSource<ConfigResponse>();
#endif

            this.processTask = this.RunInBackground();
        }

        public override async UniTask Install(Resolver resolver) {
            var analytics = await resolver.Get<IAnalytics>();

            try {
                await this.processTask;

#if UNITY_REMOTE_CONFIG
                var (hasConfig, config) = await UniTask.WhenAny(
                    this.configFetchTcs.Task.AsUniTask(),
                    UniTask.Delay(TimeSpan.FromSeconds(5))
                );

                analytics.Send("unity_remote_config",
                    new AnalyticsArg("status", hasConfig ? $"{config.requestOrigin}_{config.status}" : "timeout")
                );
#endif
            }
            catch (Exception ex) {
                analytics.Send("unity_remote_config",
                    new AnalyticsArg("status", "exception") {
                        new AnalyticsArg("type", ex?.GetType().Name) {
                            new AnalyticsArg("message", ex?.Message),
                        },
                    }
                );
            }

#if UNITY_REMOTE_CONFIG
            if (this.isFeatureVariantProvider) {
                await resolver.Register<IFeatureVariantProvider>().ToAsync<UnityRemoteConfigFeatureVariantProvider>();
            }
#endif
        }

        private async Task RunInBackground() {
            await UniTask.Delay(TimeSpan.FromSeconds(0.2f));

            await WithRetry(this.InitializeUnityServiceAsync, maxTries: 3, retryDelay: TimeSpan.FromSeconds(1));

#if UNITY_REMOTE_CONFIG
            await WithRetry(this.FetchRemoteConfigAsync, maxTries: 3, retryDelay: TimeSpan.FromSeconds(1));
#endif
        }

        private async UniTask InitializeUnityServiceAsync() {
            using (DebugTimer.Create(DEBUG_TIMER_CATEGORY, "check_internet")) {
                if (!await InternetUtils.CheckForInternetConnectionAsync()) {
                    return;
                }
            }

#if UNITY_SERVICES_CORE
            using (DebugTimer.Create(DEBUG_TIMER_CATEGORY, "initialize")) {
                await UnityServices.InitializeAsync();
            }
#endif

#if UNITY_SERVICES_AUTH
            if (!AuthenticationService.Instance.IsSignedIn) {
                using (DebugTimer.Create(DEBUG_TIMER_CATEGORY, "sign_in_anon")) {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                }
            }
#endif
        }

#if UNITY_REMOTE_CONFIG
        private async UniTask FetchRemoteConfigAsync() {
            using var _ = DebugTimer.Create(DEBUG_TIMER_CATEGORY, "fetch_config");

            RemoteConfigService.Instance.SetEnvironmentID(this.environmentId);

            RemoteConfigService.Instance.FetchCompleted += it => this.configFetchTcs.TrySetResult(it);
            RemoteConfigService.Instance.FetchConfigs(new userAttributes(), new appAttributes());

            await UniTask.NextFrame();
        }
#endif

        public static async UniTask WithRetry(Func<UniTask> call, int maxTries, TimeSpan retryDelay) {
            while (maxTries-- > 0) {
                try {
                    await call.Invoke();
                    return;
                }
                catch {
                    if (maxTries <= 0) {
                        throw;
                    }
                }

                await UniTask.Delay(retryDelay);
            }
        }

        [Serializable]
        public struct userAttributes {
        }

        [Serializable]
        public struct appAttributes {
        }
    }
}