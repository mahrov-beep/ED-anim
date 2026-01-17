namespace Multicast.Boot {
    using System.Linq;
    using Cysharp.Threading.Tasks;
    using Diagnostics;
    using External.MessagePack;
    using GameProperties;
    using Modules.Morpeh;
    using Multicast;
    using Scellecs.Morpeh;
    using Steps;
    using UniMob;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using UnityEngine;

    [RequireFieldsInit]
    public struct BootloaderControllerArgs : IFlowControllerArgs {
        public PreloaderUI PreloaderUI;

        public Transform PreloaderRoot;
        public ViewPanel SystemViewPanel;
        public ViewPanel RootViewPanel;

        public string[] GameAssemblies;
        public string   EditorPlatformOverride;
    }

    public class BootloaderController : FlowController<BootloaderControllerArgs> {
        private const string DEBUG_TIMER_BOOT = "boot";

        [RuntimeInitializeOnLoadMethod]
        private static void Setup() {
            ControllersShared.RegisterController<BootloaderControllerArgs, BootloaderController>();
        }

        protected override async UniTask Activate(Context context) {
            await UniTask.DelayFrame(5);

            await context.RunForResult(new SetupStackTraceControllerArgs());
            await context.RunChild(new UnhandledErrorControllerArgs());

            using (DebugTimer.Create(DEBUG_TIMER_BOOT, "initialize_addressables")) {
                await context.RunForResult(new InitAddressablesControllerArgs());
            }

            await UniTask.Yield();

            using (DebugTimer.Create(DEBUG_TIMER_BOOT, "load_services_scene")) {
                await context.RunForResult(new LoadServicesSceneControllerArgs());
            }

            await UniTask.Yield();

            using (DebugTimer.Create(DEBUG_TIMER_BOOT, "init_dirty_data")) {
                await context.RunForResult(new InitDirtyDataControllerArgs());
            }

            // Create modules lifetime before view
            // in order to be able to access models from view's Dispose method
            var modulesLifetimeController = this.Lifetime.CreateNested();

            // Create UI
            await UniTask.Yield();

            var rootNavigator   = new GlobalKey<NavigatorState>();
            var systemNavigator = new GlobalKey<NavigatorState>();

            using (DebugTimer.Create(DEBUG_TIMER_BOOT, "run_unimob_ui_app")) {
                await context.RunChild(new UniMobUiControllerArgs {
                    RootViewPanel   = this.Args.RootViewPanel,
                    SystemViewPanel = this.Args.SystemViewPanel,
                    RootKey         = rootNavigator,
                    SystemKey       = systemNavigator,
                });
            }

            var platform = await context.RunForResult(new GetPlatformControllerArgs {
                EditorPlatformOverride = this.Args.EditorPlatformOverride,
            }, default(string));

            App.Current = new App(platform, rootNavigator, systemNavigator);
            this.Lifetime.Register(() => App.Current = null);

            await context.RunForResult(new DebugLogAppInfoControllerArgs());

            using (DebugTimer.Create(DEBUG_TIMER_BOOT, "configure_localization")) {
                await context.RunForResult(new ConfigureLocalizationControllerArgs());
            }

            // move game UI to foreground (and preloader to background)
            this.Args.PreloaderRoot.transform.SetSiblingIndex(0);
            this.Args.RootViewPanel.transform.SetSiblingIndex(1);
            this.Args.SystemViewPanel.transform.SetSiblingIndex(2);

            // Load and Install modules
            await UniTask.Yield();

            var world                    = World.Default;
            var systemsGroupRegistration = new SystemsGroupRegistration();

            using (DebugTimer.Create(DEBUG_TIMER_BOOT, "install_modules")) {
                await context.RunForResult(new InstallModulesControllerArgs {
                    GameAssemblies           = this.Args.GameAssemblies,
                    ModulesLifetime          = modulesLifetimeController.Lifetime,
                    World                    = world,
                    SystemsGroupRegistration = systemsGroupRegistration,
                    PreloaderUI              = this.Args.PreloaderUI,
                });
            }

            using (DebugTimer.Create(DEBUG_TIMER_BOOT, "init_app_container")) {
                var models = App.Current.Container.OfType<Model>().ToList();

                foreach (var model in models) {
                    model.Initialize();
                }
            }

            using (DebugTimer.Create(DEBUG_TIMER_BOOT, "preload_ui")) {
                await context.RunChild(new UniMobUiAddressablesPreloadControllerArgs {
                    AddressablesLabel = AppConstants.AddressableLabels.UI,
                    LoadOnDemand      = true,
                });
            }

            // Execute startup command

            App.Execute(new ConfigureUserDataForModelsCommand());
            App.Execute(new SyncGamePropertiesCommand());

            if (ControllersShared.IsControllerRegisteredForArgs<AppBootControllerArgs>()) {
                await context.RunChild(new AppBootControllerArgs());
            }

            // move preloader to foreground to hide game UI
            this.Args.RootViewPanel.transform.SetSiblingIndex(0);
            this.Args.PreloaderRoot.transform.SetSiblingIndex(1);
            this.Args.SystemViewPanel.transform.SetSiblingIndex(2);

            if (ControllersShared.IsControllerRegisteredForArgs<AppMainControllerArgs>()) {
                await context.RunChild(new AppMainControllerArgs());
            }

            using (DebugTimer.Create(DEBUG_TIMER_BOOT, "install_world")) {
                InstallRegistrationsToWorld(this.Lifetime, world, systemsGroupRegistration);
            }

            await context.RunChild(new BackButtonControllerArgs());

            GameObject.DontDestroyOnLoad(new GameObject("Foundation.AppEvents", typeof(AppEvents)));

            // Destroy loader
            await this.Args.PreloaderUI.AnimateHide();

            App.RequestAppUpdateFlow();
        }

        private static void InstallRegistrationsToWorld(Lifetime lifetime, World world, SystemsGroupRegistration systemsGroupRegistration) {
            var order = 10000;
            foreach (var installer in systemsGroupRegistration.ActionInstallers) {
                var systemsGroupOrder = ++order;
                var systemsGroup      = world.CreateSystemsGroup();

                installer.Invoke(systemsGroup);

                lifetime.Bracket(
                    () => world.AddSystemsGroup(systemsGroupOrder, systemsGroup),
                    () => world.RemoveSystemsGroup(systemsGroup)
                );
            }
        }
    }
}