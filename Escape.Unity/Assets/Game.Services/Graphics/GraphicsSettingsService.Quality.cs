namespace Game.Services.Graphics {
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public partial class GraphicsSettingsService {
        private class QualitySettingImpl : IGraphicsSetting<int> {
            private const string PrefSelected    = "GraphicsPreset.Selected";
            private const string PrefRecommended = "GraphicsPreset.Recommended";

            private readonly List<GraphicsOption<int>>   options       = new();
            private readonly List<GraphicsQualityOption> legacyOptions = new();
            private readonly Dictionary<string, int>     dedup         = new(StringComparer.OrdinalIgnoreCase);
            private readonly IGraphicsSettingsModel      model;
            private readonly IGraphicsSettingStorage     storage;
            private readonly Action                      onQualityApplied;

            public QualitySettingImpl(IGraphicsSettingsModel model, IGraphicsSettingStorage storage, Action onQualityApplied) {
                this.model   = model;
                this.storage = storage;
                this.onQualityApplied = onQualityApplied;

                this.RefreshOptions();

                var recommended = this.LoadRecommendedQuality();
                var selected    = this.LoadSelectedQuality(recommended);

                this.model.RecommendedQualityIndex = recommended;
                this.model.SelectedQualityIndex    = selected;

                this.RefreshOptions(recommended);
                this.ApplyUnityQuality(selected);
            }

            public string Key => "quality";

            public List<GraphicsOption<int>> Options => this.options;
            public List<GraphicsQualityOption> LegacyOptions => this.legacyOptions;

            public int Current => this.model.SelectedQualityIndex;

            public int Recommended => this.model.RecommendedQualityIndex;

            public void Apply(int value, bool userInitiated = true) {
                var clampedIndex = this.ClampQualityIndex(value);

                if (this.model.SelectedQualityIndex == clampedIndex && userInitiated) {
                    return;
                }

                this.ApplyUnityQuality(clampedIndex);
                this.onQualityApplied?.Invoke();

                this.model.SelectedQualityIndex = clampedIndex;
                this.storage.SaveInt(PrefSelected, clampedIndex);
            }

            public void ReapplyCurrent() {
                this.ApplyUnityQuality(this.model.SelectedQualityIndex);
            }

            private void RefreshOptions(int recommendedIndex = -1) {
                var names = QualitySettings.names ?? Array.Empty<string>();

                this.dedup.Clear();
                this.options.Clear();
                this.legacyOptions.Clear();

                if (names.Length == 0) {
                    var option = new GraphicsOption<int>(0, "Default", recommendedIndex == 0);
                    this.options.Add(option);
                    this.legacyOptions.Add(new GraphicsQualityOption(0, "Default"));
                    return;
                }

                for (var i = 0; i < names.Length; i++) {
                    var baseName = string.IsNullOrWhiteSpace(names[i]) ? $"Level {i + 1}" : names[i].Trim();
                    if (this.dedup.TryGetValue(baseName, out var count)) {
                        this.dedup[baseName] = count + 1;
                        baseName             = $"{baseName} {count + 1}";
                    } else {
                        this.dedup[baseName] = 1;
                    }

                    var isRecommended = recommendedIndex >= 0 && recommendedIndex == i;
                    this.options.Add(new GraphicsOption<int>(i, baseName, isRecommended));
                    this.legacyOptions.Add(new GraphicsQualityOption(i, baseName));
                }
            }

            private int LoadRecommendedQuality() {
                var recommended = this.DetectRecommendedQuality();                
                this.storage.SaveInt(PrefRecommended, recommended);
                return recommended;
            }

            private int LoadSelectedQuality(int fallback) {
                var selected = this.ReadPersistedQuality(PrefSelected, fallback);
                this.storage.SaveInt(PrefSelected, selected);
                return selected;
            }

            private int ReadPersistedQuality(string key, int fallback) {
                var storedString = this.storage.ReadString(key, string.Empty);
                if (int.TryParse(storedString, out var storedIndex)) {
                    return this.ClampQualityIndex(storedIndex);
                }

                var resolved = this.ResolveQualityIndexByName(storedString);
                if (resolved >= 0) {
                    return resolved;
                }

                var storedInt = this.storage.ReadInt(key, fallback);
                return this.ClampQualityIndex(storedInt);
            }

            private int DetectRecommendedQuality() {
                var tier = GraphicsSettingsService.DetectRecommendedTier();

                var desiredName = tier switch {
                    RecommendedGraphicsTier.Low    => "Low",
                    RecommendedGraphicsTier.Medium => "Medium",
                    RecommendedGraphicsTier.High   => "High",
                    RecommendedGraphicsTier.Ultra  => "Ultra",
                    _                              => "Medium"
                };

                var resolved = this.ResolveQualityIndexByName(desiredName);
                
                if (resolved >= 0) {
                    return resolved;
                }

                return this.MapTierToIndex((int)tier);
            }

            private void ApplyUnityQuality(int qualityIndex) {
                var index = this.ClampQualityIndex(qualityIndex);
                if (this.options.Count == 0) {
                    return;
                }

                var current = QualitySettings.GetQualityLevel();
                if (current == index) {
                    return;
                }

                QualitySettings.SetQualityLevel(index, true);
            }

            private int ResolveQualityIndexByName(string name) {
                if (string.IsNullOrWhiteSpace(name)) {
                    return -1;
                }

                for (var i = 0; i < this.options.Count; i++) {
                    if (string.Equals(this.options[i].Name, name, StringComparison.OrdinalIgnoreCase)) {
                        return i;
                    }
                }

                return -1;
            }

            private int ClampQualityIndex(int value) {
                if (this.options.Count == 0) {
                    return 0;
                }

                return Mathf.Clamp(value, 0, this.options.Count - 1);
            }

            private int MapTierToIndex(int tier) {
                if (this.options.Count == 0) {
                    return 0;
                }

                var normalized = Mathf.Clamp01(tier / 3f);
                var mapped     = Mathf.RoundToInt(normalized * (this.options.Count - 1));
                return Mathf.Clamp(mapped, 0, this.options.Count - 1);
            }
        }
    }
}
