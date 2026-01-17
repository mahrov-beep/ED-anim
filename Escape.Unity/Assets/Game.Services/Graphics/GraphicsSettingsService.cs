namespace Game.Services.Graphics {
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public partial class GraphicsSettingsService : IDisposable {
        private readonly IGraphicsSettingStorage storage;
        private readonly QualitySettingImpl  qualitySetting;
        private readonly ShadowsSettingImpl  shadowsSetting;
        private readonly TexturesSettingImpl texturesSetting;
        private readonly LightingSettingImpl lightingSetting;
        private bool                         sceneReapplyHookRegistered;
        private bool                         quitHookRegistered;
        private bool                         disposed;
        private CancellationTokenSource      deferredReapplyCts;

        public GraphicsSettingsService(
            IGraphicsSettingsModel model,
            IGraphicsSettingStorage storage = null,
            GraphicsSettingsConfig graphicsConfig = null) {
            this.storage = storage ?? new PlayerPrefsGraphicsSettingStorage();

            var resolvedShadowConfig   = this.ResolveShadowConfig(graphicsConfig?.Shadows);
            var resolvedLightingConfig = this.ResolveLightingConfig(graphicsConfig?.Lighting);
            var resolvedTextureConfig  = this.ResolveTextureConfig(graphicsConfig?.Textures);

            this.qualitySetting  = new QualitySettingImpl(model, this.storage, this.ScheduleDeferredReapply);
            this.shadowsSetting  = new ShadowsSettingImpl(model, resolvedShadowConfig, this.storage);
            this.texturesSetting = new TexturesSettingImpl(model, resolvedTextureConfig, this.storage);
            this.lightingSetting = new LightingSettingImpl(model, resolvedLightingConfig, this.storage);

            this.EnsureSceneReapplyHook();
            this.EnsureApplicationQuitHook();
        }

        public IGraphicsSetting<int> QualitySetting => this.qualitySetting;
        public IGraphicsSetting<GraphicsShadowQuality> ShadowsSetting => this.shadowsSetting;
        public IGraphicsSetting<GraphicsTextureQuality> TexturesSetting => this.texturesSetting;
        public IGraphicsSetting<GraphicsLightingMode> LightingSetting => this.lightingSetting;

        public List<GraphicsQualityOption> QualityOptions => this.qualitySetting.LegacyOptions;
        public int CurrentQualityIndex => this.qualitySetting.Current;
        public int RecommendedQualityIndex => this.qualitySetting.Recommended;

        public void ApplyQualityLevel(int qualityIndex, bool userInitiated = true) {
            this.qualitySetting.Apply(qualityIndex, userInitiated);
        }

        public void ReapplyCurrentPreset() {
            this.qualitySetting.ReapplyCurrent();
        }

        public void ClearCachedLightmaps() {
            this.shadowsSetting.ClearLightmapCache();
        }

        public void Dispose() => this.DisposeInternal();

        private void EnsureSceneReapplyHook() {
            if (this.sceneReapplyHookRegistered) {
                return;
            }

            SceneManager.activeSceneChanged += this.OnActiveSceneChanged;
            this.sceneReapplyHookRegistered = true;
        }

        private void EnsureApplicationQuitHook() {
            if (this.quitHookRegistered) {
                return;
            }

            Application.quitting += this.OnApplicationQuit;
            this.quitHookRegistered = true;
        }

        private void OnActiveSceneChanged(Scene previous, Scene next) {
            this.shadowsSetting.ClearLightmapCache();
            this.ReapplyAllSettings();
        }

        private void OnApplicationQuit() {
            this.DisposeInternal();
        }

        private void ScheduleDeferredReapply() {
            this.deferredReapplyCts?.Cancel();
            this.deferredReapplyCts?.Dispose();
            var cts = new CancellationTokenSource();
            this.deferredReapplyCts = cts;
            var token = cts.Token;

            UniTask.Void(async () => {
                try {
                    await UniTask.DelayFrame(5, PlayerLoopTiming.PostLateUpdate, token);
                    if (!token.IsCancellationRequested) {                        
                        this.ReapplyAllWithoutQualitySettings();
                        this.ReleaseDeferredReapplyCts(cts);
                    }
                } catch (OperationCanceledException) {
                    this.ReleaseDeferredReapplyCts(cts);
                }
            });
        }

        private void ReapplyAllSettings() {
            this.qualitySetting.ReapplyCurrent();
            this.shadowsSetting.Apply(this.shadowsSetting.Current, userInitiated: false);
            this.texturesSetting.Apply(this.texturesSetting.Current, userInitiated: false);
            this.lightingSetting.Apply(this.lightingSetting.Current, userInitiated: false);
        }

        private void ReapplyAllWithoutQualitySettings() {            
            this.shadowsSetting.Apply(this.shadowsSetting.Current, userInitiated: false);
            this.texturesSetting.Apply(this.texturesSetting.Current, userInitiated: false);
            this.lightingSetting.Apply(this.lightingSetting.Current, userInitiated: false);
        }

        private ShadowSettingsConfig ResolveShadowConfig(ShadowSettingsConfig shadowConfig) {
            return shadowConfig ?? ScriptableObject.CreateInstance<ShadowSettingsConfig>();
        }

        private LightingSettingsConfig ResolveLightingConfig(LightingSettingsConfig lightingConfig) {
            return lightingConfig ?? ScriptableObject.CreateInstance<LightingSettingsConfig>();
        }

        private TextureSettingsConfig ResolveTextureConfig(TextureSettingsConfig textureConfig = null) {
            return textureConfig ?? ScriptableObject.CreateInstance<TextureSettingsConfig>();
        }

        private void DisposeInternal() {
            if (this.disposed) {
                return;
            }

            if (this.sceneReapplyHookRegistered) {
                SceneManager.activeSceneChanged -= this.OnActiveSceneChanged;
                this.sceneReapplyHookRegistered = false;
            }

            if (this.quitHookRegistered) {
                Application.quitting -= this.OnApplicationQuit;
                this.quitHookRegistered = false;
            }

            if (this.deferredReapplyCts != null) {
                this.deferredReapplyCts.Cancel();
                this.ReleaseDeferredReapplyCts(this.deferredReapplyCts);
            }

            this.shadowsSetting.ClearLightmapCache();

            this.disposed = true;
        }

        private void ReleaseDeferredReapplyCts(CancellationTokenSource cts) {
            if (cts == null) {
                return;
            }

            if (ReferenceEquals(this.deferredReapplyCts, cts)) {
                this.deferredReapplyCts.Dispose();
                this.deferredReapplyCts = null;
            } else {
                cts.Dispose();
            }
        }

        private enum RecommendedGraphicsTier {
            Low,
            Medium,
            High,
            Ultra
        }

        private static RecommendedGraphicsTier DetectRecommendedTier() {
            var systemMemoryMB = SystemInfo.systemMemorySize; // MB
            if (systemMemoryMB > 0) {
                var systemMemoryGB = Mathf.RoundToInt(systemMemoryMB / 1024f);
                if (systemMemoryGB >= 8) {
                    return RecommendedGraphicsTier.Ultra;
                }

                if (systemMemoryGB >= 6) {
                    return RecommendedGraphicsTier.High;
                }

                if (systemMemoryGB >= 4) {
                    return RecommendedGraphicsTier.Medium;
                }

                return RecommendedGraphicsTier.Low;
            }

            var gpuMemoryMB = SystemInfo.graphicsMemorySize; // MB
            if (gpuMemoryMB >= 2000) {
                return RecommendedGraphicsTier.Ultra;
            }

            if (gpuMemoryMB >= 1200) {
                return RecommendedGraphicsTier.High;
            }

            if (gpuMemoryMB >= 800) {
                return RecommendedGraphicsTier.Medium;
            }

            return RecommendedGraphicsTier.Low;
        }
    }
}
