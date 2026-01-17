namespace Multicast {
    using System;
    using JetBrains.Annotations;
    using UnityEngine;

    internal sealed class AppEvents : MonoBehaviour {
        private void Update() {
            App.Events.Raise(ApplicationUpdateEvent.Instance);
        }

        private void OnApplicationFocus(bool hasFocus) {
            App.Events.Raise(new ApplicationFocusEvent {
                HasFocus = hasFocus,
            });
        }

        private void OnApplicationPause(bool pauseStatus) {
            App.Events.Raise(new ApplicationPauseEvent {
                Paused = pauseStatus,
            });
        }

        private void OnApplicationQuit() {
            App.Events.Raise(new ApplicationQuitEvent());
        }
    }

    [RequireFieldsInit]
    public sealed class ApplicationFocusEvent : IAppEvent {
        public bool HasFocus;
    }

    [RequireFieldsInit]
    public sealed class ApplicationPauseEvent : IAppEvent {
        public bool Paused;
    }

    [RequireFieldsInit]
    public sealed class ApplicationQuitEvent : IAppEvent {
    }

    [RequireFieldsInit]
    public sealed class ApplicationUpdateEvent : IAppEvent {
        internal static readonly ApplicationUpdateEvent Instance = new ApplicationUpdateEvent();
    }
}