namespace Game.Services.Graphics {
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.AddressableAssets;
    using UnityEngine.ResourceManagement.AsyncOperations;
    
    public class LightmapPresetService {
        private readonly Dictionary<string, Preset> presets = new(StringComparer.OrdinalIgnoreCase);

        private LightmapData[] originalLightmaps;
        private LightmapsMode  originalMode;
        private bool           originalCached;

        public void CacheOriginal() {
            if (this.originalCached) {
                return;
            }

            this.originalLightmaps = this.CloneLightmaps(LightmapSettings.lightmaps);
            this.originalMode      = LightmapSettings.lightmapsMode;
            this.originalCached    = true;
        }
        
        public void SaveCurrent(string key) {
            if (string.IsNullOrWhiteSpace(key)) {
                throw new ArgumentException("Key must not be empty", nameof(key));
            }

            this.CacheOriginal();
            this.presets[key] = new Preset(this.CloneLightmaps(LightmapSettings.lightmaps), LightmapSettings.lightmapsMode);
        }
        
        public void SaveOriginal(string key) {
            if (!this.originalCached) {
                this.CacheOriginal();
            }

            if (string.IsNullOrWhiteSpace(key)) {
                throw new ArgumentException("Key must not be empty", nameof(key));
            }

            this.presets[key] = new Preset(this.CloneLightmaps(this.originalLightmaps), this.originalMode);
        }

        public bool Apply(string key) {
            if (!this.presets.TryGetValue(key, out var preset)) {
                Debug.LogWarning($"[LightmapPreset] Preset '{key}' not found.");
                return false;
            }

            this.ApplyPreset(preset);
            Debug.Log($"[LightmapPreset] Applied '{key}' (maps={preset.Lightmaps?.Length ?? 0}, mode={preset.Mode}).");
            return true;
        }
        
        public async UniTask<bool> LoadFromAddressablesAsync(string address, string presetKey = null, bool apply = false, CancellationToken cancellation = default) {
            if (string.IsNullOrWhiteSpace(address)) {
                Debug.LogWarning("[LightmapPreset] Address is empty.");
                return false;
            }

            presetKey ??= address;

            try {
                var preset = await this.LoadPresetInternalAsync(() => Addressables.LoadAssetAsync<LightmapPresetAsset>(address), cancellation);
                if (preset == null) {
                    Debug.LogWarning($"[LightmapPreset] Addressables asset '{address}' not found.");
                    return false;
                }

                this.presets[presetKey] = preset.Value;

                if (apply) {
                    this.ApplyPreset(preset.Value);
                    Debug.Log($"[LightmapPreset] Loaded+applied '{presetKey}' from '{address}'.");
                } else {
                    Debug.Log($"[LightmapPreset] Loaded '{presetKey}' from '{address}'.");
                }

                return true;
            } catch (OperationCanceledException) {
                Debug.LogWarning($"[LightmapPreset] Load cancelled for '{address}'.");
                return false;
            } catch (Exception e) {
                Debug.LogWarning($"[LightmapPreset] Failed to load '{address}': {e}");
                return false;
            }
        }
        
        public async UniTask<bool> LoadFromConfigAsync(LightmapPresetConfig config, string applyKey = null, bool applyFirstIfNoKey = false, CancellationToken cancellation = default) {
            if (config == null || config.presets == null || config.presets.Count == 0) {
                Debug.LogWarning("[LightmapPreset] Config is empty.");
                return false;
            }

            Preset? presetToApply = null;

            foreach (var entry in config.presets) {
                if (entry == null || entry.asset == null) {
                    continue;
                }

                var key = string.IsNullOrWhiteSpace(entry.key) ? entry.asset.AssetGUID : entry.key;
                if (string.IsNullOrWhiteSpace(key)) {
                    continue;
                }

                try {
                    var preset = await this.LoadPresetInternalAsync(() => entry.asset.LoadAssetAsync<LightmapPresetAsset>(), cancellation);
                    if (preset == null) {
                        Debug.LogWarning($"[LightmapPreset] Asset for '{key}' is null.");
                        continue;
                    }

                    this.presets[key] = preset.Value;

                    if (presetToApply == null && !string.IsNullOrWhiteSpace(applyKey) && string.Equals(applyKey, key, StringComparison.OrdinalIgnoreCase)) {
                        presetToApply = preset.Value;
                    }

                } catch (OperationCanceledException) {
                    Debug.LogWarning("[LightmapPreset] Load config cancelled.");
                    return false;
                } catch (Exception e) {
                    Debug.LogWarning($"[LightmapPreset] Failed to load preset '{key}': {e}");
                }
            }

            if (presetToApply == null && applyFirstIfNoKey) {
                foreach (var kv in this.presets) {
                    presetToApply = kv.Value;
                    break;
                }
            }

            if (presetToApply != null) {
                this.ApplyPreset(presetToApply.Value);
                Debug.Log("[LightmapPreset] Applied preset from config.");
            }

            return true;
        }

        private async UniTask<Preset?> LoadPresetInternalAsync(Func<AsyncOperationHandle<LightmapPresetAsset>> loader, CancellationToken cancellation) {
            AsyncOperationHandle<LightmapPresetAsset> handle = default;
            try {
                handle = loader();
                var asset = await handle.Task.AsUniTask();
                cancellation.ThrowIfCancellationRequested();
                if (asset == null) {
                    return null;
                }

                return this.BuildPresetFromAsset(asset);
            } finally {
                if (handle.IsValid()) {
                    Addressables.Release(handle);
                }
            }
        }

        public bool RestoreOriginal() {
            if (!this.originalCached) {
                Debug.LogWarning("[LightmapPreset] Original lightmaps not cached, nothing to restore.");
                return false;
            }

            this.ApplyPreset(new Preset(this.CloneLightmaps(this.originalLightmaps), this.originalMode));
            Debug.Log("[LightmapPreset] Restored original lightmaps.");
            return true;
        }

        public bool Remove(string key) {
            return this.presets.Remove(key);
        }

        public void Clear() {
            this.presets.Clear();
        }

        private void ApplyPreset(Preset preset) {
            LightmapSettings.lightmaps     = preset.Lightmaps ?? Array.Empty<LightmapData>();
            LightmapSettings.lightmapsMode = preset.Mode;
        }

        private Preset BuildPresetFromAsset(LightmapPresetAsset asset) {
            if (asset == null) {
                return new Preset(Array.Empty<LightmapData>(), LightmapsMode.NonDirectional);
            }

            var max = Math.Max(asset.colorMaps?.Length ?? 0, Math.Max(asset.dirMaps?.Length ?? 0, asset.shadowMasks?.Length ?? 0));
            var maps = new LightmapData[max];
            for (var i = 0; i < max; i++) {
                maps[i] = new LightmapData {
                    lightmapColor = asset.colorMaps != null && i < asset.colorMaps.Length ? asset.colorMaps[i] : null,
                    lightmapDir   = asset.dirMaps != null && i < asset.dirMaps.Length ? asset.dirMaps[i] : null,
                    shadowMask    = asset.shadowMasks != null && i < asset.shadowMasks.Length ? asset.shadowMasks[i] : null,
                };
            }

            return new Preset(maps, asset.mode);
        }

        private LightmapData[] CloneLightmaps(LightmapData[] source) {
            if (source == null || source.Length == 0) {
                return Array.Empty<LightmapData>();
            }

            var copy = new LightmapData[source.Length];
            for (var i = 0; i < source.Length; i++) {
                var src = source[i];
                if (src == null) {
                    copy[i] = new LightmapData();
                    continue;
                }

                copy[i] = new LightmapData {
                    lightmapColor = src.lightmapColor,
                    lightmapDir   = src.lightmapDir,
                    shadowMask    = src.shadowMask,
                };
            }

            return copy;
        }

        private readonly struct Preset {
            public Preset(LightmapData[] lightmaps, LightmapsMode mode) {
                this.Lightmaps = lightmaps;
                this.Mode      = mode;
            }

            public LightmapData[] Lightmaps { get; }
            public LightmapsMode  Mode      { get; }
        }
    }
}
