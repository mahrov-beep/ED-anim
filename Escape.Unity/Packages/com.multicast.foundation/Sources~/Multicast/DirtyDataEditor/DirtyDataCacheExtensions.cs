namespace Multicast.DirtyDataEditor {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Collections;

    public static class DirtyDataCacheExtensions {
        private static Dictionary<string, Regex> rgxCache = new Dictionary<string, Regex>();

        public static T GetSingle<T>(this ICache<TextAsset> cache, string path)
            where T : class {
            var asset = cache.Get(path);
            var text = asset.Text;

            var sw = Stopwatch.StartNew();
            var item = DirtyDataParser.Parse<T>(text);

            MulticastLog.Debug?.Log(nameof(DirtyDataCacheExtensions), $"Parse {path} ({typeof(T).Name}) in {sw.ElapsedMilliseconds} ms");

            return item;
        }

        [Obsolete("Use IEnumerableCache overload", error: true)]
        public static LookupCollection<T> GetLookup<T>(this ICache<TextAsset> cache, string path)
            where T : Def {
            var asset = cache.Get(path);
            var text = asset.Text;

            var sw = Stopwatch.StartNew();
            var list = DirtyDataParser.ParseList<T>(text);

            MulticastLog.Debug?.Log(nameof(DirtyDataCacheExtensions), $"Parse {path} (List<{typeof(T).Name}>) in {sw.ElapsedMilliseconds} ms");

            return new LookupCollection<T>(list);
        }

        [Obsolete("Use IEnumerableCache overload", error: true)]
        public static LookupCollection<T> GetLookup<T>(this ICache<TextAsset> cache, string[] paths)
            where T : Def {
            var list = new List<T>();

            var sw = Stopwatch.StartNew();

            foreach (var path in paths) {
                var asset = cache.Get(path);
                var text = asset.Text;

                sw.Restart();

                DirtyDataParser.ParseListAppend(list, text);

                MulticastLog.Debug?.Log(nameof(DirtyDataCacheExtensions), $"Parse {path} (List<{typeof(T).Name}>) in {sw.ElapsedMilliseconds} ms");
            }

            return new LookupCollection<T>(list);
        }

        public static LookupCollection<T> GetLookup<T>(this IEnumerableCache<TextAsset> cache, string pathPrefix)
            where T : Def {
            var list = new List<T>();

            var sw = Stopwatch.StartNew();

            var paths = cache.EnumeratePaths().Where(Match(pathPrefix)).ToList();
            paths.Sort((a, b) => string.Compare(a, b, StringComparison.Ordinal));

            foreach (var path in paths) {
                var asset = cache.Get(path);
                var text = asset.Text;

                sw.Restart();

                DirtyDataParser.ParseListAppend(list, text);

                MulticastLog.Debug?.Log(nameof(DirtyDataCacheExtensions), $"Parse {path} (List<{typeof(T).Name}>) in {sw.ElapsedMilliseconds} ms ({list.Count} items)");
            }

            return new LookupCollection<T>(list);
        }

        public static Func<string, bool> Match(string path) {
            return Matcher;

            bool Matcher(string it) {
                if (path.Contains("*")) {
                    var rgx = GetOrCreateMatchRegex(path);
                    if (rgx.IsMatch(it)) {
                        return true;
                    }
                }

                if (!it.StartsWith(path, StringComparison.InvariantCulture)) {
                    return false;
                }

                return it.Length == path.Length || it.Length > path.Length && it[path.Length] == '$';
            }
        }

        private static Regex GetOrCreateMatchRegex(string path) {
            if (rgxCache.ContainsKey(path)) {
                return rgxCache[path];
            }

            var prefix = Regex.Escape(path).Replace("\\*", @"([a-zA-Z0-9]*)");
            return rgxCache[path] = new Regex(prefix + @"($|[\$])");
        }
    }
}