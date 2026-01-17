namespace Multicast.Cheats {
    using System;
    using JetBrains.Annotations;

    public interface ICheatButtonsRegistry {
        [PublicAPI]
        public void RegisterAction(string name, Action action);
    }
}