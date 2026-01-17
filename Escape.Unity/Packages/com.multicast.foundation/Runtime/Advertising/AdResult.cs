namespace Multicast.Advertising {
    public abstract class AdResult {
        public static AdResult Completed(string adNetwork, string adUnitId) {
            return new AdCompleted(adNetwork, adUnitId);
        }

        public static AdResult Canceled(string adNetwork) {
            return new AdCanceled(adNetwork);
        }

        public static AdResult NotAvailable(string message) {
            return new AdNotAvailable(message);
        }

        public class AdCompleted : AdResult {
            public string AdNetwork { get; }
            public string ADUnitId  { get; }

            public AdCompleted(string adNetwork, string adUnitId) {
                this.AdNetwork = adNetwork;
                this.ADUnitId  = adUnitId;
            }
        }

        public class AdCanceled : AdResult {
            public string AdNetwork { get; }

            public AdCanceled(string adNetwork) {
                this.AdNetwork = adNetwork;
            }
        }

        public class AdNotAvailable : AdResult {
            public string Message { get; }

            public AdNotAvailable(string message = null) {
                this.Message = message;
            }
        }
    }
}