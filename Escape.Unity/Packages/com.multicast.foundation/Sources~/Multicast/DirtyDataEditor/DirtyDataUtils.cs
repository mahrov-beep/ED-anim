namespace Multicast.DirtyDataEditor {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    internal static class DirtyDataUtils {
        public static List<Assembly> DependantAssemblies { get; } = new List<Assembly>();

        static DirtyDataUtils() {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()) {
                if (asm.GetReferencedAssemblies().Any(refAsm => refAsm.Name == "Multicast.Foundation" || refAsm.Name == "Multicast")) {
                    DependantAssemblies.Add(asm);
                }
            }

            DependantAssemblies.Add(typeof(DirtyDataUtils).Assembly);
        }

        public static long GetHash(ReadOnlySpan<char> str) {
            var hash = 2166136261;

            foreach (var c in str) {
                hash ^= c;
                hash *= 16777619;
            }

            return hash;
        }

        public static string GetNiceName(this Type type) {
            if (!type.IsGenericType) {
                return type.Name;
            }

            var badName  = type.Name;
            var typeName = badName.Substring(0, badName.IndexOf("`", StringComparison.Ordinal));
            return typeName + "<" + string.Join(",", type.GetGenericArguments().Select(GetNiceName)) + ">";
        }
    }
}