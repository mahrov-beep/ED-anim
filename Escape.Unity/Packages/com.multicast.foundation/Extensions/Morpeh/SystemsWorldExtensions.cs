namespace Scellecs.Morpeh {
    using System;
    using System.Collections.Generic;
    using JetBrains.Annotations;

    public static class SystemsWorldExtensions {
        private static readonly Dictionary<World, SystemsRegistry> Registries = new Dictionary<World, SystemsRegistry>();

        public static void RegisterSystem(this World world, SystemBase systemBase) {
            var registry = GetSystemRegistry(world);
            registry.RegisterSystem(systemBase);
        }

        [PublicAPI]
        public static TSystem GetExistingSystem<TSystem>(this World world)
            where TSystem : SystemBase {
            var registry = GetSystemRegistry(world);
            return (TSystem) registry.GetExistingSystem(typeof(TSystem));
        }

        [PublicAPI]
        public static SystemBase GetExistingSystem(this World world, Type type) {
            var registry = GetSystemRegistry(world);
            return registry.GetExistingSystem(type);
        }

        [PublicAPI]
        public static void AddExistingSystem<TSystem>(this SystemsGroup systemsGroup)
            where TSystem : SystemBase {
            var world  = systemsGroup.world;
            var system = world.GetExistingSystem<TSystem>();
            systemsGroup.AddSystem(system);
        }

        internal static SystemsRegistry GetSystemRegistry(this World world) {
            if (!Registries.TryGetValue(world, out var registry)) {
                registry = new SystemsRegistry(world);
                Registries.Add(world, registry);
            }

            return registry;
        }
    }

    internal class SystemsRegistry {
        private readonly World                        world;
        private readonly Dictionary<Type, SystemBase> systems = new Dictionary<Type, SystemBase>();

        internal SystemsRegistry(World world) {
            this.world = world;
        }

        public void RegisterSystem([NotNull] SystemBase systemBase) {
            if (systemBase == null) {
                throw new ArgumentNullException(nameof(systemBase));
            }

            this.systems.Add(systemBase.GetType(), systemBase);
        }

        public SystemBase GetExistingSystem(Type type) {
            if (this.systems.TryGetValue(type, out var system)) {
                return system;
            }

            throw new InvalidOperationException($"System of type '{type.Name}' not exist in world '{this.world.GetFriendlyName()}'");
        }
    }
}