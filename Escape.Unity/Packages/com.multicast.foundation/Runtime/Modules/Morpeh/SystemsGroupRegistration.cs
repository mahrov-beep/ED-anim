namespace Multicast.Modules.Morpeh {
    using System;
    using System.Collections.Generic;
    using Scellecs.Morpeh;

    internal class SystemsGroupRegistration : IWorldRegistration {
        public List<Action<SystemsGroup>> ActionInstallers { get; } = new();

        public void RegisterInstaller(Action<SystemsGroup> install) {
            this.ActionInstallers.Add(install);
        }
    }
}