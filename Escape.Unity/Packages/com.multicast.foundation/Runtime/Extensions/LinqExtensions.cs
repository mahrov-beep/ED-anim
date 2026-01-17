using System;
using System.Collections.Generic;

public static class LinqExtensions {
    public static T SingleOrDefaultIfMultiple<T>(this IEnumerable<T> source) {
        var hasResult = false;
        var result    = default(T);

        if (source is IList<T> list) {
            for (int i = 0, count = list.Count; i < count; i++) {
                var current = list[i];

                if (hasResult) {
                    return default;
                }

                hasResult = true;
                result    = current;
            }
        }
        else {
            foreach (var current in source) {
                if (hasResult) {
                    return default;
                }

                hasResult = true;
                result    = current;
            }
        }

        return result;
    }

    public static T SingleOrDefaultIfMultiple<T>(this IEnumerable<T> source, Func<T, bool> predicate) {
        var hasResult = false;
        var result    = default(T);

        if (source is IList<T> list) {
            for (int i = 0, count = list.Count; i < count; i++) {
                var current = list[i];

                if (!predicate.Invoke(current)) {
                    continue;
                }

                if (hasResult) {
                    return default;
                }

                hasResult = true;
                result    = current;
            }
        }
        else {
            foreach (var current in source) {
                if (!predicate.Invoke(current)) {
                    continue;
                }

                if (hasResult) {
                    return default;
                }

                hasResult = true;
                result    = current;
            }
        }

        return result;
    }
}