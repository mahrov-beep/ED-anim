namespace Multicast.Install {
    using System;
    using System.Linq;
    using System.Reflection;

    public static class AppResolverExtensions {
        public static bool HasSkipAttribute(Type type) {
            return type.GetCustomAttributes<SkipInstallWithoutDependencyAttribute>().Any();
        }

        public static bool ShouldSkip(ScriptableModule.Resolver resolver, Type type) {
            var skipInstall = type.GetCustomAttributes<SkipInstallWithoutDependencyAttribute>();

            foreach (var attr in skipInstall) {
                if (!resolver.HasProviderFor(attr.Type)) {
                    return true;
                }
            }

            return false;
        }

        public static bool ShouldSkip(ScriptableModule.ModuleSetup module, Type type) {
            var skipInstall = type.GetCustomAttributes<SkipInstallWithoutDependencyAttribute>();

            foreach (var attr in skipInstall) {
                if (!module.HasProviderFor(attr.Type)) {
                    return true;
                }
            }

            return false;
        }
    }
}