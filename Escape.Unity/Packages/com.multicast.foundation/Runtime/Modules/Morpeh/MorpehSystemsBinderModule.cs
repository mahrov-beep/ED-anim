namespace Multicast.Modules.Morpeh {
    using System;
    using System.Linq;
    using System.Reflection;
    using Cysharp.Threading.Tasks;
    using Install;
    using Scellecs.Morpeh;

    public class MorpehSystemsBinderModule : IScriptableModule,
        INonLoggedScriptableModule,
        IScriptableModuleWithPriority,
        ISubModuleProvider {
        private readonly Assembly[] assemblies;
        private readonly World      world;

        public string name { get; } = "MorpehSystemsBinderModule";

        public bool IsPlatformSupported(string platform) => true;

        public int Priority => ScriptableModulePriority.LATE;

        public MorpehSystemsBinderModule(Assembly[] assemblies, World world) {
            this.assemblies = assemblies;
            this.world      = world;
        }

        public void Setup(ScriptableModule.ModuleSetup module) {
        }

        public void PreInstall() {
        }

        public async UniTask Install(ScriptableModule.Resolver resolver) {
        }

        public void PostInstall() {
        }

        public IScriptableModule[] BuildSubModules() {
            var baseSystemType = typeof(SystemBase);

            return this.assemblies
                .SelectMany(asm => asm.GetTypes())
                .Where(type => type.IsClass && !type.IsAbstract && baseSystemType.IsAssignableFrom(type))
                .Select(type => (IScriptableModule) new SystemInstallerModule(this.world, type))
                .ToArray();
        }

        private class SystemInstallerModule : IScriptableModule, INonLoggedScriptableModule {
            private readonly World world;
            private readonly Type  systemType;
            private readonly bool  checkSkip;

            public SystemInstallerModule(World world, Type systemType) {
                this.world      = world;
                this.systemType = systemType;
                this.checkSkip  = AppResolverExtensions.HasSkipAttribute(systemType);
            }

            public string name                                 => this.systemType.Name;
            public bool   IsPlatformSupported(string platform) => true;

            public void Setup(ScriptableModule.ModuleSetup module) {
                if (this.checkSkip && AppResolverExtensions.ShouldSkip(module, this.systemType)) {
                    return;
                }

                module.Provides(this.systemType);
            }

            public void PreInstall() {
            }

            public async UniTask Install(ScriptableModule.Resolver resolver) {
                if (this.checkSkip && AppResolverExtensions.ShouldSkip(resolver, this.systemType)) {
                    return;
                }

                var instance = await resolver.CreateInstanceInternalAsync(this.systemType);
                resolver.RegisterInternal(this.systemType, instance);
                this.world.RegisterSystem((SystemBase) instance);
            }

            public void PostInstall() {
            }
        }
    }
}