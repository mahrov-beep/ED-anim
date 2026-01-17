namespace Multicast.Boot.Steps {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Collections;
    using Cysharp.Threading.Tasks;
    using GameProperties;
    using Install;
    using Modules;
    using Modules.Analytics;
    using Modules.Boosts;
    using Modules.CommandHandlers;
    using Modules.LunarConsole;
    using Modules.Morpeh;
    using Modules.Playtime;
    using Modules.TIme;
    using Modules.UiDynamicContext;
    using Modules.UserStats;
    using Modules.UserTracking;
    using Multicast;
    using Scellecs.Morpeh;
    using UniMob;
    using UniMob.UI;
    using UnityEngine;

    [RequireFieldsInit]
    internal struct InstallModulesControllerArgs : IResultControllerArgs {
        public World                    World;
        public SystemsGroupRegistration SystemsGroupRegistration;
        public string[]                 GameAssemblies;
        public Lifetime                 ModulesLifetime;
        public PreloaderUI              PreloaderUI;
    }

    internal class InstallModulesController : ResultController<InstallModulesControllerArgs> {
        [RuntimeInitializeOnLoadMethod]
        private static void Setup() {
            ControllersShared.RegisterController<InstallModulesControllerArgs, InstallModulesController>();
        }

        protected override async UniTask Execute(Context context) {
            var gameModulesCache = new AddressableCache<ScriptableModule>();

            await gameModulesCache.Preload(AppConstants.AddressableLabels.MODULES);

            var gameModules = gameModulesCache.EnumerateCachedPaths()
                .Select(it => gameModulesCache.Get(it))
                .ToList();

            var gameAssemblies = this.GetGameAssemblies();
            Array.Resize(ref gameAssemblies, gameAssemblies.Length + 1);
            gameAssemblies[^1] = typeof(BootLoader).Assembly;

            if (Debug.unityLogger.IsLogTypeAllowed(LogType.Log)) {
                foreach (var gameAssembly in gameAssemblies) {
                    Debug.Log($"Registered game assembly: {gameAssembly.FullName}");
                }
            }

            var modules = new List<IScriptableModule>();

            modules.Add(ScriptableModuleFactory.Service(this.Args.ModulesLifetime));
            modules.Add(ScriptableModuleFactory.Service<AppSharedFormulaContext, AppSharedFormulaContext>());
            modules.Add(ScriptableModuleFactory.Service<AppSharedNumberFormulaContext, AppSharedNumberFormulaContext>());
            modules.Add(new UserTrackingModule());
            modules.Add(new MorpehModule(this.Args.World, this.Args.SystemsGroupRegistration));
            modules.Add(new UiDynamicContextModule());
            modules.Add(new AnalyticsModule());
            modules.Add(new DebugLogAnalyticsModule());
            modules.Add(new GamePropertiesModule());
            modules.Add(new PlaytimeModule());
            modules.Add(new LunarConsoleModule());
            modules.Add(new FrameRateJitterModule());
            modules.Add(new BoostsModule());
            modules.Add(new TimeModule());
            modules.Add(new UserStatsModule());
            modules.AddRange(gameModules);
            modules.Add(new MorpehSystemsBinderModule(gameAssemblies, this.Args.World));
            modules.Add(new HandlersModule(gameAssemblies, StateProvider.Shared));

            await UniTask.Yield();

            using (ModelSafety.EnterModulesInstallPhase()) {
                await ScriptableModuleInstaller
                    .Create(modules, App.Platform, new Progress<float>(this.Args.PreloaderUI.UpdateProgress), App.Current.Container)
                    .SetupAndInstall();
            }
        }

        protected Assembly[] GetGameAssemblies() {
            var assemblies = new List<Assembly>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                if (this.Args.GameAssemblies.Contains(assembly.GetName().Name)) {
                    assemblies.Add(assembly);
                }
            }

            return assemblies.ToArray();
        }
    }
}