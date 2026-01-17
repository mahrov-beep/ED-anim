namespace Multicast.Analytics {
    using System.Collections.Generic;
    using UnityEngine;

    public static class AnalyticsExtensions {
        public static IEnumerable<AnalyticsArg> EnumerateArgs(this ref BakedAnalyticsEvent evt) {
            static AnalyticsArg ArgSelector(AnalyticsArg it) => it;
            return AnalyticsArgTraverse.Linear(evt.Args, ArgSelector);
        }

        public static Dictionary<string, object> ToAppMetricaDictionary(this ref BakedAnalyticsEvent evt) {
            var dict = new Dictionary<string, object>();

            foreach (var arg in evt.Args) {
                if (dict.ContainsKey(arg.Key)) {
                    Debug.LogError($"Analytics: Duplicate arg {arg.Key} for {evt.Name}");
                    dict[arg.Key] = BuildArg(evt.Name, arg);
                }
                else {
                    dict.Add(arg.Key, BuildArg(evt.Name, arg));
                }
            }

            return dict;
        }

        private static object BuildArg(string evt, AnalyticsArg arg) {
            if (arg.InnerArgs == null || arg.InnerArgs.Count == 0) {
                return arg.Type switch {
                    AnalyticsArg.ArgType.Bool => arg.BoolValue ? "yes" : "no",
                    AnalyticsArg.ArgType.Int => arg.ObjectValue,
                    AnalyticsArg.ArgType.Long => arg.ObjectValue,
                    AnalyticsArg.ArgType.String => arg.ObjectValue,
                    _ => arg.StringValue,
                };
            }

            if (arg.InnerArgs.Count == 1) {
                return new Dictionary<string, object> {
                    [arg.Value] = BuildArg(evt, arg.InnerArgs[0]),
                };
            }

            var dict = new Dictionary<string, object>();

            foreach (var subArg in arg.InnerArgs) {
                if (dict.ContainsKey(subArg.Key)) {
                    Debug.LogError($"Analytics: Duplicate arg {subArg.Key} for {evt}");
                    dict[subArg.Key] = BuildArg(evt, subArg);
                }
                else {
                    dict.Add(subArg.Key, BuildArg(evt, subArg));
                }
            }

            return new Dictionary<string, object> {[arg.Value] = dict};
        }
    }
}