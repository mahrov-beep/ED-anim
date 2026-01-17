namespace Multicast.Cheats {
    using System;
    using DropSystem;
    using JetBrains.Annotations;
    using Scellecs.Morpeh;

    public static class CheatButtonRegistryExtensions {
        [PublicAPI]
        public static void RegisterDrop(this ICheatButtonsRegistry registry, string name, Func<Drop> func) {
            registry.RegisterCommand(name, () => new AddDropCheatCommand(func()));
        }

        [PublicAPI]
        public static void RegisterCommand<T>(this ICheatButtonsRegistry registry, string name, Func<T> func) where T : struct, ICommand {
            registry.RegisterAction(name, () => App.Execute(func()));
        }
    }
}