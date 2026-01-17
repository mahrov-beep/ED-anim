#if APPMETRICA_SDK
namespace Multicast.Modules.AdAchievements.AppMetrica {
    using System;
    using Multicast.AdAchievements;
    using Multicast.Analytics;
    using UnityEngine;
    using Multicast.Modules.IapValidation;

    public class AppMetricaAdAchievementsAnalyticsAdapter : IAnalyticsAdapter {
        public string Name { get; } = "AdAchievements.AppMetrica";

        public void Send(BakedAnalyticsEvent evt) {
            switch (evt.SourceEvent) {
                case AdRevenueAnalyticsEvent adRevenue:
                    SendAdRevenue(adRevenue);
                    break;

                case EcpmProfileAnalyticsEvent ecpmProfileEvent:
                    SendEcpmProfile(ecpmProfileEvent);
                    break;

                case AdProfileEvent adProfileEvent:
                    SendAdProfile(adProfileEvent);
                    break;

                case IapRevenueAnalyticsEvent iapRevenueAnalyticsEvent:
                    SendIapRevenue(iapRevenueAnalyticsEvent);
                    break;
            }
        }

        private static void SendAdProfile(AdProfileEvent adProfileEvent) {
            var profile = new Io.AppMetrica.Profile.UserProfile()
                .Apply(Io.AppMetrica.Profile.Attribute.CustomBoolean("Is Active Subscription").WithValue(adProfileEvent.HasSubscription))
                .Apply(Io.AppMetrica.Profile.Attribute.CustomNumber("ECPM avg").WithValue(adProfileEvent.Ecpm))
                .Apply(Io.AppMetrica.Profile.Attribute.CustomNumber("Rewarded Impressions").WithValue(adProfileEvent.Impressions))
                .Apply(Io.AppMetrica.Profile.Attribute.CustomNumber("Rewarded eCPM").WithValue(adProfileEvent.Ecpm));

            Io.AppMetrica.AppMetrica.ReportUserProfile(profile);
        }

        private static void SendEcpmProfile(EcpmProfileAnalyticsEvent ecpmProfileEvent) {
            var profile = new Io.AppMetrica.Profile.UserProfile()
                .Apply(Io.AppMetrica.Profile.Attribute.CustomNumber("ECPM avg").WithValue(ecpmProfileEvent.Ecpm));

            Io.AppMetrica.AppMetrica.ReportUserProfile(profile);
        }

        private static void SendAdRevenue(AdRevenueAnalyticsEvent adRevenue) {
            var appmetricaAdRevenue = new Io.AppMetrica.AdRevenue(adRevenue.Revenue, "USD") {
                AdNetwork       = adRevenue.NetworkName,
                AdPlacementName = adRevenue.Placement,
                AdPlacementId   = adRevenue.CreativeIdentifier,
                AdUnitId        = adRevenue.AdUnitIdentifier,
            };

            Io.AppMetrica.AppMetrica.ReportAdRevenue(appmetricaAdRevenue);
        }

        private static void SendIapRevenue(IapRevenueAnalyticsEvent iapRevenue) {
            var revenue = new Io.AppMetrica.Revenue((long)(iapRevenue.Price * 1_000_000), iapRevenue.Currency) {
                ProductID = iapRevenue.StoreSpecificId
            };

            var yaReceipt = new Io.AppMetrica.Revenue.Receipt();
            var receipt   = JsonUtility.FromJson<Receipt>(iapRevenue.ValidatedReceipt);

#if UNITY_ANDROID
            var payloadAndroid = JsonUtility.FromJson<PayloadAndroid>(receipt.Payload);

            yaReceipt.Signature  = payloadAndroid.signature;
            yaReceipt.Data       = payloadAndroid.json;
            revenue.ReceiptValue = yaReceipt;
#elif UNITY_IOS
            yaReceipt.TransactionID = receipt.TransactionID;
            yaReceipt.Data          = receipt.Payload;
            revenue.ReceiptValue    = yaReceipt;
#endif

            Io.AppMetrica.AppMetrica.ReportRevenue(revenue);
            Io.AppMetrica.AppMetrica.SendEventsBuffer();
        }

        public void Flush() {
        }
    }

    [System.Serializable]
    internal struct Receipt {
        public string Store;
        public string TransactionID;
        public string Payload;
    }

    [System.Serializable]
    internal struct PayloadAndroid {
        public string json;
        public string signature;
    }
}
#endif