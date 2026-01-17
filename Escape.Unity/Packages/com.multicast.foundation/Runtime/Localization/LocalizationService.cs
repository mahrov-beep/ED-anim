namespace Multicast.Localization {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CodeWriter.ViewBinding;
    using UniMob;
    using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor;

#endif

    public static class LocalizationService {
        private const string LANG_KEY = "KEY";
        private const string LANG_EN  = "EN";

        public static readonly IReadOnlyDictionary<SystemLanguage, string> LanguageCodes = new Dictionary<SystemLanguage, string> {
            [(SystemLanguage) (-1)]   = "KEY",
            [SystemLanguage.English]  = "EN",
            [SystemLanguage.Russian]  = "RU",
            [SystemLanguage.German]   = "DE",
            [SystemLanguage.French]   = "FR",
            [SystemLanguage.Japanese] = "JA",
            [SystemLanguage.Korean]   = "KO",
            [SystemLanguage.Spanish]  = "ES",
        };

        private static readonly MutableAtom<string> SelectedLangAtom = Atom.Computed(Lifetime.Eternal,
            () => PlayerPrefs.GetString("Multicast.Lang", string.Empty),
            v => PlayerPrefs.SetString("Multicast.Lang", v)
        );

        private static Dictionary<string, LocalizationTable[]> tables;
        private static SpanHashMap<LocalizationTextLink>       search;
        private static SpanHashMap<LocalizationKeyReplacement> replacements;

        public static IReadOnlyList<string> Languages { get; private set; }

        public static string SelectedLang {
            get => SelectedLangAtom.Value;
            set => SelectedLangAtom.Value = value;
        }

#if UNITY_EDITOR
        public static SystemLanguage EditorLanguage {
            get => (SystemLanguage) SessionState.GetInt("EditorLocalizationSetup_Lang", (int) SystemLanguage.English);
            set => SessionState.SetInt("EditorLocalizationSetup_Lang", (int) value);
        }
#endif

        private static string CurrentLanguage {
            get {
#if UNITY_EDITOR
                if (!Application.isPlaying) {
                    return GetCodeBySystemLanguage(EditorLanguage);
                }
#endif
                return SelectedLang;
            }
        }

        public static void Configure(ICache<LocalizationTable> cache, string[] paths) {
            var assets    = Array.ConvertAll(paths, path => cache.Get(path));
            var keyAssets = new List<LocalizationTable>();
            var languages = new HashSet<string>();
            var textCount = 0;

            Array.Sort(assets, (a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));

            foreach (var asset in assets) {
                languages.Add(asset.Lang);

                if (asset.Lang == LANG_KEY) {
                    keyAssets.Add(asset);
                    textCount += asset.Values.Length;
                }
            }

            tables = new Dictionary<string, LocalizationTable[]>(languages.Count);
            search = new SpanHashMap<LocalizationTextLink>((int) (textCount * 1.2f));

            replacements = null;

            for (var tableIndex = 0; tableIndex < keyAssets.Count; tableIndex++) {
                var keys = keyAssets[tableIndex].Values;

                for (var valueIndex = 0; valueIndex < keys.Length; valueIndex++) {
                    search.Add(keys[valueIndex], new LocalizationTextLink {
                        tableIndex = tableIndex,
                        valueIndex = valueIndex,
                    });
                }
            }

            foreach (var lang in languages) {
                tables.Add(lang, new LocalizationTable[keyAssets.Count]);
            }

            foreach (var asset in assets) {
                var tableIndex = keyAssets.FindIndex(keyAsset => keyAsset.Page == asset.Page);
                tables[asset.Lang][tableIndex] = asset;
            }

            var includeKeyLang = Application.isEditor || Debug.isDebugBuild;
            if (!includeKeyLang) {
                languages.Remove(LANG_KEY);
            }

            Languages = languages.ToList();

            if (string.IsNullOrEmpty(SelectedLang)) {
                var lang = GetDefaultSystemCode();
                SelectedLang = languages.Contains(lang) ? lang : LANG_EN;
            }

            BindingsLocalization.SetCallback(Localize);
        }

        public static void AddKeyReplacement(string oldKey, string newKey) {
            replacements ??= new SpanHashMap<LocalizationKeyReplacement>(10);

            if (replacements.TryGetValue(oldKey, out _)) {
                Debug.LogError($"Localization key replacement for {oldKey} already exist");
            }

            replacements.Add(oldKey, new LocalizationKeyReplacement {
                newKey = newKey,
            });
        }

        private static string Localize(ref ValueTextBuilder b) {
            var key = b.AsSpan();
            
            if (TryLocalize(key, CurrentLanguage, out var currentLangTranslation)) {
                return currentLangTranslation;
            }

            if (TryLocalize(key, LANG_EN, out var defaultLangTranslation)) {
                return defaultLangTranslation;
            }

            var str = b.ToString();
            Debug.LogError($"Localization key '{str}' not exists");
            return str;
        }

        private static bool TryLocalize(ReadOnlySpan<char> key, string lang, out string result) {
            if (replacements != null && replacements.TryGetValue(key, out var replacement)) {
                key = replacement.newKey.AsSpan();
            }

            if (tables.TryGetValue(lang, out var langTables) &&
                search.TryGetValue(key, out var link) &&
                link.tableIndex < langTables.Length &&
                langTables[link.tableIndex] is var langTable &&
                langTable != null &&
                langTable.Values[link.valueIndex] is var translation &&
                !string.IsNullOrEmpty(translation)) {
                result = translation;
                return true;
            }

            result = default;
            return false;
        }

        public static string GetCodeBySystemLanguage(SystemLanguage systemLanguage) {
            return LanguageCodes.TryGetValue(systemLanguage, out var code)
                ? code
                : LANG_EN;
        }

        public static string GetDefaultSystemCode() {
            return GetCodeBySystemLanguage(Application.systemLanguage);
        }

        [Serializable]
        private struct LocalizationTextLink {
            public int tableIndex;
            public int valueIndex;
        }
        
        [Serializable]
        private struct LocalizationKeyReplacement {
            public string newKey;
        }

        [Serializable]
        private struct SpanHashMapSlot {
            public string key;
            public int    next;
        }

        private sealed class SpanHashMap<T> where T : struct {
            private readonly int   capacity;
            private readonly int[] buckets;
            private readonly T[]   data;

            private readonly SpanHashMapSlot[] slots;

            private int lastIndex;

            public SpanHashMap(int capacity) {
                this.lastIndex = 0;
                this.capacity  = capacity;
                this.buckets   = new int[this.capacity];
                this.slots     = new SpanHashMapSlot[this.capacity];
                this.data      = new T[this.capacity];
            }

            public void Add(string key, in T value) {
                var hash = key.Length % this.capacity;

                var slotIndex = this.lastIndex;
                ++this.lastIndex;

                ref var newSlot = ref this.slots[slotIndex];

                newSlot.key  = key;
                newSlot.next = this.buckets[hash] - 1;

                this.data[slotIndex] = value;
                this.buckets[hash]   = slotIndex + 1;
            }

            public bool TryGetValue(ReadOnlySpan<char> key, out T value) {
                var hash = key.Length % this.capacity;

                int next;
                for (var i = this.buckets[hash] - 1; i >= 0; i = next) {
                    ref var slot = ref this.slots[i];

                    if (key.SequenceEqual(slot.key)) {
                        value = this.data[i];
                        return true;
                    }

                    next = slot.next;
                }

                value = default;
                return false;
            }
        }
    }
}