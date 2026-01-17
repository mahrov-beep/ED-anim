namespace Multicast.Install {
    public static class ScriptableModulePriority {
        public const int EARLY  = -100;
        public const int NORMAL = 0;
        public const int LATE   = 100;

        public static int GetPriority(IScriptableModule module) {
            return module is IScriptableModuleWithPriority withPriority
                ? withPriority.Priority
                : NORMAL;
        }
    }
}