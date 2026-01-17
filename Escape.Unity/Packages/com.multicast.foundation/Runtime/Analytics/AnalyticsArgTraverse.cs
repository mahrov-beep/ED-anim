namespace Multicast.Analytics {
    using System;
    using System.Collections.Generic;
    using JetBrains.Annotations;

    public class AnalyticsArgTraverse {
        public static IEnumerable<TDest> Linear<TDest>([NotNull] AnalyticsArgCollection args, Func<AnalyticsArg, TDest> mapper) {
            if (args == null) {
                throw new ArgumentNullException(nameof(args));
            }

            foreach (var arg in args) {
                foreach (var dest in Linear(arg, mapper)) {
                    yield return dest;
                }
            }
        }

        public static IEnumerable<TDest> Linear<TDest>([NotNull] List<AnalyticsArg> args, Func<AnalyticsArg, TDest> mapper) {
            if (args == null) {
                throw new ArgumentNullException(nameof(args));
            }

            foreach (var arg in args) {
                foreach (var dest in Linear(arg, mapper)) {
                    yield return dest;
                }
            }
        }

        public static IEnumerable<TDest> Linear<TDest>(AnalyticsArg arg, Func<AnalyticsArg, TDest> mapper) {
            yield return mapper.Invoke(arg);

            if (arg.InnerArgs != null) {
                foreach (var dest in Linear(arg.InnerArgs, mapper)) {
                    yield return dest;
                }
            }
        }
    }
}