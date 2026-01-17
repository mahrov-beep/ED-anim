namespace Multicast {
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Install;
    using JetBrains.Annotations;
    using UniMob.UI;
    using UniMob.UI.Widgets;

    public partial class App {
        internal static App Current { get; set; }

        internal bool AppUpdateRequested;

        private readonly string                    platform;
        private readonly GlobalKey<NavigatorState> rootNavigatorKey;
        private readonly GlobalKey<NavigatorState> systemNavigatorKey;
        private readonly SynchronizationContext    unitySynchronizationContext;
        private readonly EventHub<IAppEvent>       appEventHub;

        internal readonly ServicesContainer Container = new ServicesContainer();

        private Func<string> editorCloneKeyDelegate;

        [PublicAPI]
        public static string Platform => Current.platform;

        [PublicAPI]
        public static string EditorCloneKey => Current.editorCloneKeyDelegate?.Invoke() ?? string.Empty;

        [PublicAPI]
        public static EventHub<IAppEvent> Events => Current.appEventHub;

        internal App(string platform, GlobalKey<NavigatorState> rootNavigatorKey, GlobalKey<NavigatorState> systemNavigatorKey) {
            this.platform                    = platform;
            this.rootNavigatorKey            = rootNavigatorKey;
            this.systemNavigatorKey          = systemNavigatorKey;
            this.unitySynchronizationContext = SynchronizationContext.Current;
            this.appEventHub                 = new EventHub<IAppEvent>();
        }

        [PublicAPI]
        public static void EnsureIsOnUnityThread() {
            if (SynchronizationContext.Current != App.Current.unitySynchronizationContext) {
                throw new InvalidOperationException("Method must only be executed on unity thread");
            }
        }

        [PublicAPI]
        public static void ExecuteOnUnityThread(Action call) {
            App.Current.unitySynchronizationContext.Post(_ => call(), null);
        }

        [PublicAPI]
        public static T Get<T>() {
            return (T)Current.Container.Get(typeof(T));
        }

        [PublicAPI]
        public static void RequestAppUpdateFlow() {
            Current.AppUpdateRequested = true;
        }

        public NavigatorState GetNavigator(AppNavigatorType type) => type switch {
            AppNavigatorType.Root => this.rootNavigatorKey.CurrentState,
            AppNavigatorType.System => this.systemNavigatorKey.CurrentState,
            _ => throw new ArgumentOutOfRangeException(nameof(type)),
        };

        public static bool IsRootNavigatorOnMainScreen() {
            return Current.GetNavigator(AppNavigatorType.Root).Screens.Length == 1;
        }

        public static void SetEditorCloneKeyDelegate(Func<string> it) {
            Current.editorCloneKeyDelegate = it;
        }
    }

    public enum AppNavigatorType {
        Root,
        System,
    }
}