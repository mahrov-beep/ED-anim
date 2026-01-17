namespace Multicast.Modules.Morpeh {
    using System;
    using System.Reflection;
    using Cysharp.Threading.Tasks;
    using Install;
    using Scellecs.Morpeh;
    using UnityEngine;

    public class MorpehModule : IScriptableModule {
        private readonly Type  eventType      = typeof(Event<>);
        private readonly Type  requestType    = typeof(Request<>);
        private readonly Type  aspectType     = typeof(AspectFactory<>);
        private readonly Type  stashType      = typeof(Stash<>);
        private readonly Type  systemBaseType = typeof(SystemBase);
        private readonly World world;

        private readonly IWorldRegistration worldRegistration;

        public string name { get; } = "Morpeh";

        public bool IsPlatformSupported(string platform) => true;

        public MorpehModule(World world, IWorldRegistration worldRegistration) {
            this.world             = world;
            this.worldRegistration = worldRegistration;
        }

        public void Setup(ScriptableModule.ModuleSetup module) {
            module.Provides<World>();
            module.Provides<IWorldRegistration>();
            module.ProvidesDynamic(type => type.IsGenericType && type.GetGenericTypeDefinition() == this.eventType);
            module.ProvidesDynamic(type => type.IsGenericType && type.GetGenericTypeDefinition() == this.requestType);
            module.ProvidesDynamic(type => type.IsGenericType && type.GetGenericTypeDefinition() == this.aspectType);
            module.ProvidesDynamic(type => type.IsGenericType && type.GetGenericTypeDefinition() == this.stashType);
            module.ProvidesDynamic(type => this.systemBaseType.IsAssignableFrom(type));
        }

        public UniTask Install(ScriptableModule.Resolver resolver) {
            resolver.Register<World>().To(this.world);
            resolver.Register<IWorldRegistration>().To(this.worldRegistration);
            resolver.RegisterDynamic(this.GetEvent);
            resolver.RegisterDynamic(this.GetRequest);
            resolver.RegisterDynamic(this.GetAspect);
            resolver.RegisterDynamic(this.GetStash);
            resolver.RegisterDynamic(this.GetSystem);

            return UniTask.CompletedTask;
        }

        public void PreInstall() {
        }

        public void PostInstall() {
        }

        private object GetEvent(Type type) {
            if (!type.IsGenericType || type.GetGenericTypeDefinition() != this.eventType) {
                return null;
            }

            var eventDataType = type.GetGenericArguments()[0];
            return this.world.GetReflectionEvent(eventDataType);
        }

        private object GetRequest(Type type) {
            if (!type.IsGenericType || type.GetGenericTypeDefinition() != this.requestType) {
                return null;
            }

            var requestDataType = type.GetGenericArguments()[0];
            return this.world.GetReflectionRequest(requestDataType);
        }

        private object GetAspect(Type type) {
            if (!type.IsGenericType || type.GetGenericTypeDefinition() != this.aspectType) {
                return null;
            }

            var aspectFactory = (IBoxedAspectFactory) Activator.CreateInstance(type);
            this.InjectStashesAndAspectsIntoAspect(aspectFactory);
            return aspectFactory;
        }

        private object GetStash(Type type) {
            if (!type.IsGenericType || type.GetGenericTypeDefinition() != this.stashType) {
                return null;
            }

            var componentType = type.GetGenericArguments()[0];
            return this.world.GetReflectionStash(componentType);
        }

        private object GetSystem(Type type) {
            if (!this.systemBaseType.IsAssignableFrom(type)) {
                return null;
            }

            return this.world.GetExistingSystem(type);
        }

        private void InjectStashesAndAspectsIntoAspect(IBoxedAspectFactory factory) {
            var flags  = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var fields = factory.AspectType.GetFields(flags);

            var instance = factory.ValueBoxed;

            foreach (var fi in fields) {
                if (!fi.IsDefined(typeof(InjectAttribute), false)) {
                    continue;
                }

                var stash = this.GetStash(fi.FieldType);
                if (stash != null) {
                    fi.SetValue(instance, stash);
                    continue;
                }

                var aspect = this.GetAspect(fi.FieldType);
                if (aspect != null) {
                    fi.SetValue(instance, aspect);
                    continue;
                }

                Debug.LogError($"Failed to inject '{fi.Name}' into '{factory.AspectType}': only stashes and aspects can be injected into aspects");
            }

            instance.OnGetAspectFactory(this.world);

            factory.ValueBoxed = instance;
        }
    }
}