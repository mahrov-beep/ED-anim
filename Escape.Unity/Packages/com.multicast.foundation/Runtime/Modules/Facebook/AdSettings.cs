#if FACEBOOK_SDK

using System.Runtime.InteropServices;
using UnityEngine;

namespace AudienceNetwork {
    internal static class AdSettings {
#if UNITY_IOS && !UNITY_EDITOR && APPLOVIN_MAX_SDK
        [DllImport("__Internal")]
        private static extern void FBAdSettingsBridgeSetAdvertiserTrackingEnabled(bool advertiserTrackingEnabled);

        public static void SetAdvertiserTrackingEnabled(bool advertiserTrackingEnabled) {
            FBAdSettingsBridgeSetAdvertiserTrackingEnabled(advertiserTrackingEnabled);
            Debug.Log($"AudienceNetwork.AdSettings.SetAdvertiserTrackingEnabled({advertiserTrackingEnabled})");
        }
#else
        public static void SetAdvertiserTrackingEnabled(bool advertiserTrackingEnabled) {
        }
#endif
    }
}

#endif