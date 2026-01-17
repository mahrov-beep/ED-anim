namespace GreenButtonGames.Analytics.DebugLog {
    using System.Text;
    using Multicast.Analytics;
    using Multicast.GameProperties;
    using Multicast.Modules.Analytics;
    using UnityEngine;

    public class DebugLogAnalyticsAdapter : IAnalyticsAdapter {
        private readonly GamePropertiesModel properties;
        private readonly StringBuilder       sb = new StringBuilder();

        public string Name { get; } = "DebugLog";

        public DebugLogAnalyticsAdapter(GamePropertiesModel properties) {
            this.properties = properties;
        }

        public void Send(BakedAnalyticsEvent evt) {
            var shouldLog = Debug.unityLogger.IsLogTypeAllowed(LogType.Log);

            if (!shouldLog) {
                return;
            }

            this.sb.Clear();
            this.sb.Append("Analytics: ").AppendLine(evt.Name);
            foreach (var element in evt.EnumerateArgs()) {
                this.sb.Append(" - ").Append(element.Key).Append(" = ").AppendLine(element.Value);
            }

            if (Application.isEditor) {
                this.sb.AppendLine();
                this.sb.AppendLine(evt.SourceEvent.GetType().FullName);
            }

            Debug.Log(this.sb.ToString());
        }

        public void Flush() {
        }
    }
}