namespace Multicast.Server {
#if UNITY_EDITOR
    using UnityEditor;

    public static class ServerMenu {
        private const string USE_LOCAL_SERVER_MENU = "Server/Use Local Server";

        public static bool UseLocalServer {
            get => SessionState.GetBool("Server.UseLocalServer", false);
            set => SessionState.SetBool("Server.UseLocalServer", value);
        }

        [MenuItem(USE_LOCAL_SERVER_MENU, true, 99)]
        public static bool CanToggleUseLocalServer() {
            Menu.SetChecked(USE_LOCAL_SERVER_MENU, UseLocalServer);
            return true;
        }

        [MenuItem(USE_LOCAL_SERVER_MENU, false, 99)]
        public static void ToggleUseLocalServer() {
            UseLocalServer = !UseLocalServer;
        }
    }
#else
    public static class ServerMenu {
        public static bool UseLocalServer => false;
    }
#endif
}