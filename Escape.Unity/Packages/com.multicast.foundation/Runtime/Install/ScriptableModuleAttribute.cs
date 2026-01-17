namespace Multicast.Install {
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public class ScriptableModuleAttribute : Attribute {
        public string Category { get; set; }
    }

    public static class ScriptableModuleCategory {
        public const string GAME_CORE = "Game Core";
        public const string GAME_FEATURE = "Game Features";
    }
}