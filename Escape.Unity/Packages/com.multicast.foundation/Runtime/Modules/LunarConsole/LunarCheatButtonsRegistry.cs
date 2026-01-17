namespace Multicast.Modules.LunarConsole {
    using System;
    using Cheats;
    using JetBrains.Annotations;
#if LUNAR_CONSOLE
    using LunarConsolePlugin;
#endif
    using UniMob;
    using UnityEngine;

    internal class LunarCheatButtonsRegistry : ICheatButtonsRegistry {
        private readonly Lifetime lifetime;

        public LunarCheatButtonsRegistry(Lifetime lifetime) {
            this.lifetime = lifetime;
        }

        [PublicAPI]
        public void RegisterAction(string name, Action action) {
#if LUNAR_CONSOLE
            if (!LunarConsole.isConsoleEnabled) {
                return;
            }

            this.lifetime.Bracket(
                () => LunarConsole.RegisterAction(name, action),
                () => LunarConsole.UnregisterAction(name)
            );
#else
            Debug.LogError("Failed to register cheat action: Lunar Console not exist in project. Suggestion: add LUNAR_CONSOLE define");
#endif
        }
    }
}