#if (UNITY_EDITOR || UNITY_ANDROID) && UNITY_MOBILE_NOTIFICATIONS
#define UNITY_MOBILE_NOTIFICATIONS_ANDROID
#endif

#if (UNITY_EDITOR || UNITY_IOS) && UNITY_MOBILE_NOTIFICATIONS
#define UNITY_MOBILE_NOTIFICATIONS_IOS
#endif

namespace Multicast.Notifications {
    using System;
    using Analytics;
    using Cysharp.Threading.Tasks;
    using UnityEngine;
#if UNITY_MOBILE_NOTIFICATIONS_ANDROID
    using global::Unity.Notifications.Android;
#endif
#if UNITY_MOBILE_NOTIFICATIONS_IOS
    using global::Unity.Notifications.iOS;

#endif

    [Serializable, RequireFieldsInit]
    public struct RequestNotificationsPermissionControllerArgs : IResultControllerArgs {
    }

    public class RequestNotificationsPermissionController : ResultController<RequestNotificationsPermissionControllerArgs> {
        [Inject] private IAnalytics analytics;

        protected override async UniTask Execute(Context context) {
#if UNITY_MOBILE_NOTIFICATIONS_ANDROID
            if (Application.platform == RuntimePlatform.Android) {
                var request = new PermissionRequest();

                await UniTask.WaitWhile(() => request.Status == PermissionStatus.RequestPending);

                this.analytics.Send("push_notifications_status_android",
                    new AnalyticsArg("status", request.Status.ToString())
                );
            }
#endif

#if UNITY_MOBILE_NOTIFICATIONS_IOS
            if (Application.platform == RuntimePlatform.IPhonePlayer) {
                const AuthorizationOption authorizationOption = AuthorizationOption.Alert |
                                                                AuthorizationOption.Badge;

                using var req = new AuthorizationRequest(authorizationOption, true);

                await UniTask.WaitWhile(() => !req.IsFinished);

                this.analytics.Send("push_notifications_status_ios",
                    new AnalyticsArg("finished", req.IsFinished),
                    new AnalyticsArg("granted", req.Granted),
                    new AnalyticsArg("error", req.Error)
                );
            }
#endif
        }
    }
}