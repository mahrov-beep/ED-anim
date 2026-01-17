namespace Multicast {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;

    public static class EnumNames<T> where T : Enum {
        private static readonly Dictionary<string, T> NameToEnum;
        private static readonly Dictionary<T, string> EnumToName;

        static EnumNames() {
            var list = Enum.GetValues(typeof(T))
                .Cast<T>()
                .Select(it => new { name = Enum.GetName(typeof(T), it), value = it })
                .ToList();

            NameToEnum = list.ToDictionary(it => it.name, it => it.value);
            EnumToName = list.ToDictionary(it => it.value, it => it.name);
        }

        [PublicAPI]
        public static IEnumerable<KeyValuePair<string, T>> Values => NameToEnum;

        [PublicAPI]
        public static string GetName(T value) => EnumToName[value];

        [PublicAPI]
        public static T GetValue(string name) => NameToEnum[name];
    }
}