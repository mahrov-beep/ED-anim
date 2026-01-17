namespace Multicast.Modules.UserData {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Cysharp.Threading.Tasks;
    using Multicast;
    using Install;
    using Morpeh;
    using Multicast.Analytics;
    using Multicast.UserData;
    using Scellecs.Morpeh;
    using UnityEngine;

    public abstract partial class UserDataModuleBase<TUserData> : ScriptableModule, IScriptableModuleWithPriority
        where TUserData : UdObject {
        private UdRoot<TUserData>  userData;
        private List<PropertyInfo> autoProvidedProperties;

        public int Priority { get; } = ScriptableModulePriority.EARLY;

        public override void Setup(ModuleSetup module) {
            this.autoProvidedProperties = GetAutoProvidedProperties();

            module.Provides<TUserData>();
            module.Provides<IUserDataService>();
            module.ProvidesDynamic(type => this.autoProvidedProperties.Any(it => it.PropertyType == type));
        }

        public sealed override async UniTask Install(Resolver resolver) {
            var analytics         = await resolver.Get<IAnalytics>();
            var worldRegistration = await resolver.Get<IWorldRegistration>();

            worldRegistration.RegisterInstaller(this.InstallSystems);

#if UNITY_EDITOR || USER_DATA_SHOW_SELECTOR_UI
            if (UserDataUI.ShowUserDataSelectorOnce) {
                UserDataUI.ShowUserDataSelectorOnce = false;

                var featuresUi = new UserDataUI();
                await featuresUi.ShowUserDataSelectorUI();
            }
#endif

            var service = new UserDataService<TUserData>(analytics, this.CreateUserData);
            await service.LoadOrCreateUserData();

            resolver.Register<IUserDataService>().To(service);

            this.userData = service.UserData;

            resolver.Register<TUserData>().To(this.userData.Value);
            resolver.RegisterDynamic(this.ResolveAutoProvidedProperty);

            CreateUserDataEventsObject(service);

            this.Install(resolver, this.userData.Value);
        }

        private object ResolveAutoProvidedProperty(Type type) {
            foreach (var property in this.autoProvidedProperties) {
                if (property.PropertyType == type) {
                    return property.GetValue(this.userData.Value);
                }
            }

            return null;
        }

        private static void CreateUserDataEventsObject(UserDataService<TUserData> service) {
            var userDataEvents = new GameObject(nameof(UserDataEvents)).AddComponent<UserDataEvents>();
            userDataEvents.OnApplicationPaused = () => HandleApplicationPaused(service);
            DontDestroyOnLoad(userDataEvents);
        }

        private static void HandleApplicationPaused(UserDataService<TUserData> service) {
            if (!service.UserData.TryGetActiveTransaction(out _)) {
                service.SaveUserData();
            }
        }

        protected abstract TUserData CreateUserData(UdArgs args);

        protected virtual void Install(Resolver resolver, TUserData data) {
        }

        private static List<PropertyInfo> GetAutoProvidedProperties() {
            return typeof(TUserData)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(it => typeof(UdObject).IsAssignableFrom(it.PropertyType))
                .ToList();
        }

        private void InstallSystems(SystemsGroup systems) {
            systems.AddExistingSystem<UserDataCheatSystem>();
        }
    }
}