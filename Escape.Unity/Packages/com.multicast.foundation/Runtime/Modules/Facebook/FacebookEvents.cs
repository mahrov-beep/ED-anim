#if FACEBOOK_SDK

namespace Multicast.Modules.Facebook {
    using System.Threading.Tasks;
    using global::Facebook.Unity;
    using UnityEngine;

    internal class FacebookEvents : MonoBehaviour {
        private static FacebookEvents instance;

        private static readonly TaskCompletionSource<object> ActivationTcs = new TaskCompletionSource<object>();

        public static Task ActivateAsync() {
            if (instance) {
                return ActivationTcs.Task;
            }

            var go = new GameObject(nameof(FacebookEvents), typeof(FacebookEvents));
            instance = go.GetComponent<FacebookEvents>();
            DontDestroyOnLoad(instance);

            return ActivationTcs.Task;
        }

        private void Awake() {
            if (FB.IsInitialized) {
                this.OnFbInitialized();
            }
            else {
                FB.Init(this.OnFbInitialized);
            }
        }

        // Unity will call OnApplicationPause(false) when an app is resumed from the background
        private void OnApplicationPause(bool pauseStatus) {
            // Check the pauseStatus to see if we are in the foreground
            // or background
            if (pauseStatus) {
                return;
            }

            if (FB.IsInitialized) {
                FB.ActivateApp();
            }
            else {
                FB.Init(FB.ActivateApp);
            }
        }

        private void OnFbInitialized() {
            FB.ActivateApp();

            ActivationTcs.TrySetResult(null);
        }
    }
}

#endif