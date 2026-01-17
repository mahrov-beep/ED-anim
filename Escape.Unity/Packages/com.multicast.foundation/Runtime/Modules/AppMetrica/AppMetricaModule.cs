namespace Multicast.Modules.AppMetrica {
    using System;
    using Cysharp.Threading.Tasks;
    using Multicast.Analytics;
    using Install;
#if APPMETRICA_SDK
    using Io.AppMetrica;
#endif
    using Sirenix.OdinInspector;
    using UnityEngine;

    public class AppMetricaModule : ScriptableModule,
        IScriptableModuleWithPriority, ISubModuleProvider {
        [SerializeField, Required] private string apiKey;

        [SerializeField, InlineProperty, HideLabel]
        private AppMetricaSdkConfiguration appMetricaSdkConfiguration;

        public int Priority => ScriptableModulePriority.EARLY - 10;

        public override void Setup(ModuleSetup module) {
#if APPMETRICA_SDK
            module.Provides<SdkInitializationMarkers.AppMetrica>();
#endif
        }

        public override async UniTask Install(Resolver resolver) {
#if APPMETRICA_SDK
            var analyticsRegistration = await resolver.Get<IAnalyticsRegistration>();

            AppMetrica.Activate(new AppMetricaConfig(apiKey: this.apiKey) {
                RevenueAutoTrackingEnabled = false,
            });

            Debug.Log("AppMetrica.Activate()");

            analyticsRegistration.RegisterAdapter(new AppMetricaAnalyticsAdapter(this.appMetricaSdkConfiguration));

            resolver.Register<SdkInitializationMarkers.AppMetrica>().To(new SdkInitializationMarkers.AppMetrica());

            try {
                AppMetrica.PutErrorEnvironmentValue("systemMemorySize", SystemInfo.systemMemorySize.ToString());
                AppMetrica.PutErrorEnvironmentValue("maxTextureSize", SystemInfo.maxTextureSize.ToString());

                AppMetrica.PutErrorEnvironmentValue("processorType", SystemInfo.processorType);
                AppMetrica.PutErrorEnvironmentValue("processorCount", SystemInfo.processorCount.ToString());
                AppMetrica.PutErrorEnvironmentValue("processorFrequency", SystemInfo.processorFrequency.ToString());

                AppMetrica.PutErrorEnvironmentValue("graphicsDeviceName", SystemInfo.graphicsDeviceName);
                AppMetrica.PutErrorEnvironmentValue("graphicsDeviceVendor", SystemInfo.graphicsDeviceVendor);
                AppMetrica.PutErrorEnvironmentValue("graphicsDeviceVersion", SystemInfo.graphicsDeviceVersion);
                AppMetrica.PutErrorEnvironmentValue("graphicsDeviceType", SystemInfo.graphicsDeviceType.ToString());
                AppMetrica.PutErrorEnvironmentValue("graphicsShaderLevel", SystemInfo.graphicsShaderLevel.ToString());
                AppMetrica.PutErrorEnvironmentValue("graphicsMemorySize", SystemInfo.graphicsMemorySize.ToString());

                AppMetrica.PutErrorEnvironmentValue("hasDynamicUniformArrayIndexingInFragmentShaders", SystemInfo.hasDynamicUniformArrayIndexingInFragmentShaders.ToString());
                AppMetrica.PutErrorEnvironmentValue("npotSupport", SystemInfo.npotSupport.ToString());
                AppMetrica.PutErrorEnvironmentValue("supportsInstancing", SystemInfo.supportsInstancing.ToString());
            }
            catch (Exception e) {
                Debug.LogException(e);
            }
#else
            Debug.LogError($"Project does not contains APPMETRICA_SDK define. Add it or remove {this.name}");
#endif
        }

        public IScriptableModule[] BuildSubModules() {
            return new IScriptableModule[] {
                new AppMetricaAdjustAttributionModule(),
            };
        }
    }
}