namespace Multicast.Install {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Cysharp.Threading.Tasks;
    using Diagnostics;
    using UnityEngine;

    public class ScriptableModuleInstaller {
        private readonly IProgress<float>        progress;
        private readonly List<IScriptableModule> modules;

        private readonly ServicesContainer containter;

        private readonly List<IScriptableModule> pendingModules;
        private readonly List<IScriptableModule> installingModules;
        private readonly List<IScriptableModule> installedModules;

        internal readonly Dictionary<Type, IScriptableModule>       providedTypeToModule;
        internal readonly Dictionary<IScriptableModule, List<Type>> moduleToProvidedType;

        private readonly List<(Func<Type, bool>, IScriptableModule)> dynamicTypeProviders;

        private readonly List<Type> registeredTypes;

        private readonly List<Func<UniTask>> lazyActions;

        private ScriptableModuleInstaller(IEnumerable<IScriptableModule> modules, IProgress<float> progress, ServicesContainer container) {
            this.progress             = progress;
            this.modules              = modules.ToList();
            this.containter           = container;
            this.pendingModules       = new List<IScriptableModule>();
            this.installingModules    = new List<IScriptableModule>();
            this.installedModules     = new List<IScriptableModule>();
            this.providedTypeToModule = new Dictionary<Type, IScriptableModule>();
            this.moduleToProvidedType = new Dictionary<IScriptableModule, List<Type>>();
            this.dynamicTypeProviders = new List<(Func<Type, bool>, IScriptableModule)>();
            this.registeredTypes      = new List<Type>();
            this.lazyActions          = new List<Func<UniTask>>();
        }

        internal static ScriptableModuleInstaller Create(IEnumerable<IScriptableModule> modules, string platform, IProgress<float> progress, ServicesContainer container) {
            var allModules = new List<IScriptableModule>();

            foreach (var module in modules) {
                AddModule(module);
            }

            var platformModules = allModules
                .Where(it => it.IsPlatformSupported(platform))
                .OrderBy(it => ScriptableModulePriority.GetPriority(it))
                .ToList();

            return new ScriptableModuleInstaller(platformModules, progress, container);

            void AddModule(IScriptableModule module) {
                if (!module.IsPlatformSupported(platform)) {
                    return;
                }

                if (module is ISubModuleProvider subModuleProvider) {
                    foreach (var subModule in subModuleProvider.BuildSubModules()) {
                        AddModule(subModule);
                    }
                }

                allModules.Add(module);
            }
        }

        internal void Setup() {
            foreach (var module in this.modules) {
                this.moduleToProvidedType.Add(module, new List<Type>());

                var moduleSetup = new ScriptableModule.ModuleSetup(this, module);
                module.Setup(moduleSetup);
            }
        }

        internal async UniTask SetupAndInstall() {
            if (this.installedModules.Count != 0) {
                throw new ScriptableModuleInstallException("Install already started");
            }

            if (this.modules.Count == 0) {
                this.progress.Report(1f);
                return;
            }

            this.Setup();

            await UniTask.Yield();

            foreach (var module in this.modules) {
                module.PreInstall();
            }

            this.pendingModules.AddRange(this.modules);

            while (this.pendingModules.Count > 0) {
                await this.InstallModuleAsync(this.pendingModules[0]);

                this.progress.Report(1.0f * this.installedModules.Count / this.modules.Count);
            }

            if (this.installedModules.Count != this.modules.Count) {
                var msg = $"Some modules not installed due unknown error: {this.installedModules.Count} of {this.modules.Count}";
                throw new ScriptableModuleInstallException(msg);
            }
            
            foreach (var lazyAction in this.lazyActions) {
                await lazyAction.Invoke();
            }

            foreach (var module in this.modules) {
                module.PostInstall();
            }

            foreach (var module in this.installedModules) {
                if (module is ICompletableModule completableModule) {
                    await completableModule.WaitForCompletionAsync();
                }
            }
        }

        internal bool TryGetTypeProviderFor(Type providedType, out IScriptableModule module) {
            if (this.providedTypeToModule.TryGetValue(providedType, out module)) {
                return true;
            }

            foreach (var (filter, providerModule) in this.dynamicTypeProviders) {
                if (!filter.Invoke(providedType)) {
                    continue;
                }

                module = providerModule;
                return true;
            }

            return false;
        }

        internal void RegisterDynamicTypeProvider(Func<Type, bool> providedTypeFilter, IScriptableModule module) {
            this.dynamicTypeProviders.Add((providedTypeFilter, module));
        }

        internal void RegisterTypeProvider(Type providedType, IScriptableModule module) {
            if (this.providedTypeToModule.TryGetValue(providedType, out var otherModule)) {
                var msg = $"Type '{providedType}' cannot be provided by '{module.name}' because it already provided by '{otherModule.name}'";
                throw new ScriptableModuleInstallException(msg);
            }

            this.providedTypeToModule[providedType] = module;
            this.moduleToProvidedType[module].Add(providedType);
        }

        internal bool HasProviderFor(Type providedType) {
            return this.providedTypeToModule.ContainsKey(providedType);
        }

        internal async UniTask InstallModuleForProvideTypeAsync(Type providedType, IScriptableModule resolverModule) {
            string msg;

            if (this.providedTypeToModule.TryGetValue(providedType, out var module)) {
                if (module != resolverModule) {
                    await this.InstallModuleAsync(module);
                }

                if (!this.registeredTypes.Contains(providedType)) {
                    msg = $"Type '{providedType}' not registered. Possible due to module circular dependency";
                    throw new ScriptableModuleInstallException(msg);
                }

                return;
            }

            foreach (var (filter, providerModule) in this.dynamicTypeProviders) {
                if (!filter.Invoke(providedType)) {
                    continue;
                }

                await this.InstallModuleAsync(providerModule);
                return;
            }

            msg = $"No provider found for '{providedType}' in module '{resolverModule.name}'";
            throw new ScriptableModuleInstallException(msg);
        }

        internal void RegisterType(Type providedType, IScriptableModule module) {
            if (!this.TryGetTypeProviderFor(providedType, out var providerModule) ||
                providerModule != module) {
                var msg = $"Module '{module.name}' cannot register type '{providedType}' because module not declared as provider for that type";
                throw new Exception(msg);
            }

            this.registeredTypes.Add(providedType);
        }

        internal void QueueLazyAction(Func<UniTask> action) {
            this.lazyActions.Add(action);
        }

        private async UniTask InstallModuleAsync(IScriptableModule module) {
            if (!this.pendingModules.Remove(module)) {
                if (this.installingModules.Contains(module)) {
                    Debug.LogError($"[Install] Parallel module installation does not supported: {module.name}");
                }

                return;
            }

            this.installingModules.Add(module);

            using (var resolver = new ScriptableModule.Resolver(this, module, this.containter)) {
                await module.Install(resolver);
            }

            this.installingModules.Remove(module);

            foreach (var providedType in this.moduleToProvidedType[module]) {
                if (!this.registeredTypes.Contains(providedType)) {
                    var msg = $"Module '{module.name}' declared as '{providedType}' provider but not register '{providedType}' instance during Install phase";
                    throw new ScriptableModuleInstallException(msg);
                }
            }

            this.installedModules.Add(module);
        }
    }
}