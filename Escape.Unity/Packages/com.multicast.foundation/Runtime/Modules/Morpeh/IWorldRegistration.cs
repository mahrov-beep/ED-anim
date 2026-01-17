namespace Multicast.Modules.Morpeh {
    using System;
    using JetBrains.Annotations;
    using Scellecs.Morpeh;

    public interface IWorldRegistration {
        [PublicAPI]
        void RegisterInstaller(Action<SystemsGroup> install);
    }
}