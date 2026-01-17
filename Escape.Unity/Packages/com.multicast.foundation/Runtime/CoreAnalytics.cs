namespace Multicast {
    using System.Collections.Generic;

    internal class CoreAnalytics {
        public static void ReportEvent(string name, Dictionary<string, object> data) {
            ReportEvent(name, Json.Serialize(data));
        }

        public static void ReportEvent(string name, string json) {
#if APPMETRICA_SDK
            if (Io.AppMetrica.AppMetrica.IsActivated()) {
                Io.AppMetrica.AppMetrica.ReportEvent(name, json);
            }
#endif
        }
    }
}