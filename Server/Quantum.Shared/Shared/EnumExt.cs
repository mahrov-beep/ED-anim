using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public static class EnumExt {
    public static int ToInt<T>(this T value) where T : Enum => Convert.ToInt32(value);

    public static T[] MembersArrayExcluded<T>(T toExclude) where T : Enum {
        return Enum.GetValues(typeof(T))
            .Cast<T>()
            .Where(it => !it.Equals(toExclude))
            .ToArray();
    }

    public static int CountMembers<T>() where T : Enum {
        return Enum.GetValues(typeof(T)).Length;
    }
}

public static class EnumExt<T> where T : Enum {
    private static readonly Dictionary<string, T[]> ValuesStartsWithPrefix = new();

    [Conditional("UNITY_EDITOR")]
    public static void ValidateNameStartsWith(T value, string prefix) {
        if (!ValuesStartsWithPrefix.TryGetValue(prefix, out var valid)) {
            ValuesStartsWithPrefix[prefix] = valid = GetEnumValuesWithNamePrefix(prefix);
        }

        if (Array.IndexOf(valid, value) == -1) {
            Console.Error.WriteLine($"Enum {typeof(T).Name} value '{value.ToString()}' must start with '{prefix}'");
        }
    }

    private static T[] GetEnumValuesWithNamePrefix(string prefix) {
        return Enum.GetValues(typeof(T))
            .Cast<T>()
            .Select(v => (name: Enum.GetName(typeof(T), v), value: v))
            .Where(it => it.name.StartsWith(prefix))
            .Select(it => it.value)
            .ToArray();
    }
}