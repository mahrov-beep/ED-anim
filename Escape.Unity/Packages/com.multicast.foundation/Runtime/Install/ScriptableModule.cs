namespace Multicast.Install {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Cysharp.Threading.Tasks;
    using Diagnostics;
    using Sirenix.OdinInspector;
#if UNITY_EDITOR
    using Sirenix.Utilities;
#endif
    using UnityEngine;

    public abstract class ScriptableModule : ScriptableObject, IScriptableModule {
        [SerializeField]
        private bool anyPlatform = true;

        [SerializeField, Required]
        [LabelText("@anyPlatform ? \"Excluded Platforms\" : \"Included Platforms\"")]
        [ListDrawerSettings(ShowFoldout = false, ShowPaging = false)]
#if UNITY_EDITOR
        [ValueDropdown(nameof(AllPlatforms))]
#endif
        private List<string> platforms = new List<string>();

        public bool IsPlatformSupported(string platform) {
            if (this.anyPlatform) {
                return !this.platforms.Contains(platform);
            }

            return this.platforms.Contains(platform);
        }

        public abstract void Setup(ModuleSetup module);

        public abstract UniTask Install(Resolver resolver);

        public virtual void PreInstall() {
        }

        public virtual void PostInstall() {
        }

#if UNITY_EDITOR
        internal static List<string> AllPlatforms => AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(asm => asm.GetAttributes<RegisterScriptableModulePlatformAttribute>())
            .Select(it => it.Platform)
            .ToList();
#endif

        public class ModuleSetup {
            private readonly ScriptableModuleInstaller installer;
            private readonly IScriptableModule         module;

            internal ModuleSetup(ScriptableModuleInstaller installer, IScriptableModule module) {
                this.installer = installer;
                this.module    = module;
            }

            public void ProvidesDynamic(Func<Type, bool> filter) {
                this.installer.RegisterDynamicTypeProvider(filter, this.module);
            }

            public void Provides<T>() {
                this.installer.RegisterTypeProvider(typeof(T), this.module);
            }

            internal void Provides(Type type) {
                this.installer.RegisterTypeProvider(type, this.module);
            }

            public void ProvidesFactory<T1, TResult>() {
                this.installer.RegisterTypeProvider(typeof(Func<T1, TResult>), this.module);
            }

            public void ProvidesFactory<T1, T2, TResult>() {
                this.installer.RegisterTypeProvider(typeof(Func<T1, T2, TResult>), this.module);
            }

            public void ProvidesFactory<T1, T2, T3, TResult>() {
                this.installer.RegisterTypeProvider(typeof(Func<T1, T2, T3, TResult>), this.module);
            }

            internal bool HasProviderFor(Type type) {
                return this.installer.HasProviderFor(type);
            }
        }

        public class Resolver : IDisposable {
            private readonly ScriptableModuleInstaller installer;
            private readonly IScriptableModule         module;
            private readonly ServicesContainer         container;

            private DebugTimer debugTimer;

            internal Resolver(ScriptableModuleInstaller installer, IScriptableModule module, ServicesContainer container) {
                this.installer  = installer;
                this.module     = module;
                this.container  = container;
                this.debugTimer = DebugTimer.Create("module_install", module.name);
            }

            void IDisposable.Dispose() {
                this.debugTimer.Dispose(logResults: this.module is not INonLoggedScriptableModule);
            }

            internal bool HasProviderFor(Type type) {
                return this.installer.HasProviderFor(type);
            }

            internal async UniTask<object> GetInternal(Type type) {
                this.debugTimer.Pause();
                await this.installer.InstallModuleForProvideTypeAsync(type, this.module);
                this.debugTimer.Resume();

                return this.container.Get(type);
            }

            public async UniTask<T> Get<T>() {
                this.debugTimer.Pause();
                await this.installer.InstallModuleForProvideTypeAsync(typeof(T), this.module);
                this.debugTimer.Resume();

                return (T) this.container.Get(typeof(T));
            }

            internal void RegisterInternal(Type type, object instance) {
                this.installer.RegisterType(type, this.module);

                this.container.Register(type, instance);
            }

            public void RegisterDynamic(Func<Type, object> func) {
                this.container.RegisterBuilder(func);
            }

            public ResolverRegistrationBuilder<T> Register<T>() {
                return new ResolverRegistrationBuilder<T>(this);
            }

            public async UniTask RegisterFactory<T1, TResult>(bool lazyInject = false) {
                var factory = await this.CreateFactoryInternalAsync<T1, TResult>(typeof(TResult), lazyInject);

                this.RegisterInternal(typeof(Func<T1, TResult>), factory);
            }

            public async UniTask RegisterFactory<T1, T2, TResult>(bool lazyInject = false) {
                var factory = await this.CreateFactoryInternalAsync<T1, T2, TResult>(typeof(TResult), lazyInject);

                this.RegisterInternal(typeof(Func<T1, T2, TResult>), factory);
            }

            public async UniTask RegisterFactory<T1, T2, T3, TResult>(bool lazyInject = false) {
                var factory = await this.CreateFactoryInternalAsync<T1, T2, T3, TResult>(typeof(TResult), lazyInject);

                this.RegisterInternal(typeof(Func<T1, T2, T3, TResult>), factory);
            }

            internal async UniTask<object> CreateInstanceInternalAsync(Type type, bool lazyInject = false) {
                try {
                    var parameterValues = await this.ResolveConstructorParameters(type, Array.Empty<Type>());
                    var injector        = await this.CreateFieldsInjector(type, lazyInject);

                    return this.CreateAndInjectInstance(type, parameterValues, injector, lazyInject);
                }
                catch (Exception ex) {
                    if (ex is TargetInvocationException targetInvocationException) {
                        ex = targetInvocationException.InnerException;
                    }

                    var msg = $"Failed to resolve instance of '{type.Name}' in module '{this.module.name}'";
                    throw new ScriptableModuleTypeResolveException(msg, ex);
                }
            }

            internal async UniTask<object> CreateInstanceInternalAsync<T1>(Type type, T1 arg1) {
                try {
                    var parameterValues = await this.ResolveConstructorParameters(type, new[] {typeof(T1)});
                    var injector        = await this.CreateFieldsInjector(type);

                    for (var index = 0; index < parameterValues.Length; index++) {
                        if (parameterValues[index] is InjectValuePlaceholder placeholder) {
                            if (placeholder.ParameterType == typeof(T1)) {
                                parameterValues[index] = arg1;
                            }
                        }
                    }

                    return this.CreateAndInjectInstance(type, parameterValues, injector);
                }
                catch (Exception ex) {
                    var msg = $"Failed to resolve instance of '{type.Name}' in module '{this.module.name}'";
                    throw new ScriptableModuleTypeResolveException(msg, ex);
                }
            }

            internal async UniTask<Func<TResult>> CreateFactoryInternalAsync<TResult>(Type type) {
                var parameterValues = await this.ResolveConstructorParameters(type, Array.Empty<Type>());
                var injector        = await this.CreateFieldsInjector(type);

                return () => (TResult) this.CreateAndInjectInstance(type, parameterValues, injector);
            }

            internal async UniTask<Func<T1, TResult>> CreateFactoryInternalAsync<T1, TResult>(Type type, bool lazyInject = false) {
                var parameterValues = await this.ResolveConstructorParameters(type, new[] {typeof(T1)});
                var injector        = await this.CreateFieldsInjector(type, lazyInject);

                return (arg1) => {
                    var localParameterValues = parameterValues.ToArray();

                    for (var index = 0; index < localParameterValues.Length; index++) {
                        if (localParameterValues[index] is InjectValuePlaceholder placeholder) {
                            if (placeholder.ParameterType == typeof(T1)) {
                                localParameterValues[index] = arg1;
                            }
                        }
                    }

                    return (TResult) this.CreateAndInjectInstance(type, localParameterValues, injector);
                };
            }

            internal async UniTask<Func<T1, T2, TResult>> CreateFactoryInternalAsync<T1, T2, TResult>(Type type, bool lazyInject = false) {
                var parameterValues = await this.ResolveConstructorParameters(type, new[] {typeof(T1), typeof(T2)});
                var injector        = await this.CreateFieldsInjector(type, lazyInject);

                return (arg1, arg2) => {
                    var localParameterValues = parameterValues.ToArray();

                    for (var index = 0; index < localParameterValues.Length; index++) {
                        if (localParameterValues[index] is InjectValuePlaceholder placeholder) {
                            if (placeholder.ParameterType == typeof(T1)) {
                                localParameterValues[index] = arg1;
                            }

                            if (placeholder.ParameterType == typeof(T2)) {
                                localParameterValues[index] = arg2;
                            }
                        }
                    }

                    return (TResult) this.CreateAndInjectInstance(type, localParameterValues, injector);
                };
            }

            internal async UniTask<Func<T1, T2, T3, TResult>> CreateFactoryInternalAsync<T1, T2, T3, TResult>(Type type, bool lazyInject = false) {
                var parameterValues = await this.ResolveConstructorParameters(type, new[] {typeof(T1), typeof(T2), typeof(T3)});
                var injector        = await this.CreateFieldsInjector(type, lazyInject);

                return (arg1, arg2, arg3) => {
                    var localParameterValues = parameterValues.ToArray();

                    for (var index = 0; index < localParameterValues.Length; index++) {
                        if (localParameterValues[index] is InjectValuePlaceholder placeholder) {
                            if (placeholder.ParameterType == typeof(T1)) {
                                localParameterValues[index] = arg1;
                            }

                            if (placeholder.ParameterType == typeof(T2)) {
                                localParameterValues[index] = arg2;
                            }

                            if (placeholder.ParameterType == typeof(T3)) {
                                localParameterValues[index] = arg3;
                            }
                        }
                    }

                    return (TResult) this.CreateAndInjectInstance(type, localParameterValues, injector);
                };
            }

            private async UniTask<Action<object>> CreateFieldsInjector(Type type, bool lazyInject = false) {
                const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

                Action<object> injector = null;

                for (; type != null; type = type.BaseType) {
                    var fields = type.GetFields(flags);

                    foreach (var fi in fields) {
                        if (!fi.IsDefined(typeof(InjectAttribute), false)) {
                            continue;
                        }

                        object injectedValue    = null;
                        var    injectedValueSet = false;

                        if (lazyInject) {
                            async UniTask LazyGetInjectionValue() {
                                try {
                                    injectedValue    = await this.GetInternal(fi.FieldType);
                                    injectedValueSet = true;
                                }
                                catch (Exception ex) {
                                    var msg = $"Failed to resolve instance of '{fi.FieldType.Name}' in module '{this.module.name}'";
                                    throw new ScriptableModuleTypeResolveException(msg, ex);
                                }
                            }

                            this.installer.QueueLazyAction(LazyGetInjectionValue);
                        }
                        else {
                            injectedValue    = await this.GetInternal(fi.FieldType);
                            injectedValueSet = true;
                        }

                        injector += instance => {
                            if (!injectedValueSet) {
                                Debug.LogError($"Injection for '{type.Name}' was initiated during modules installation");
                            }

                            fi.SetValue(instance, injectedValue);
                        };
                    }
                }

                return injector;
            }

            private async UniTask<object[]> ResolveConstructorParameters(Type type, Type[] placeholderParameterTypes) {
                var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

                if (constructors.Length == 0) {
                    throw new InvalidOperationException($"Type '{type}' has not public constructor");
                }

                if (constructors.Length > 1) {
                    throw new InvalidOperationException($"Type '{type}' must contains a single public constructor");
                }

                var parameterInfos  = constructors[0].GetParameters();
                var parameterValues = new object[parameterInfos.Length];

                for (var i = 0; i < parameterInfos.Length; i++) {
                    var parameterInfo = parameterInfos[i];
                    var parameterType = parameterInfo.ParameterType;

                    if (Array.IndexOf(placeholderParameterTypes, parameterType) != -1) {
                        parameterValues[i] = new InjectValuePlaceholder(parameterType);
                        continue;
                    }

                    try {
                        parameterValues[i] = await this.GetInternal(parameterType);
                    }
                    catch {
                        Debug.LogError($"Failed to construct ctor of type '{type.Name}'");
                        throw;
                    }
                }

                return parameterValues;
            }

            private object CreateAndInjectInstance(Type type, object[] ctorParameters, Action<object> injector, bool lazyInject = false) {
                var obj = Activator.CreateInstance(type, ctorParameters);

                if (lazyInject) {
                    this.InjectInstanceLazy(obj, injector);
                }
                else {
                    injector?.Invoke(obj);
                }

                return obj;
            }

            private void InjectInstanceLazy(object obj, Action<object> injector) {
                this.installer.QueueLazyAction(LazyInjectTask);

                UniTask LazyInjectTask() {
                    injector?.Invoke(obj);
                    return UniTask.CompletedTask;
                }
            }

            private class InjectValuePlaceholder {
                public Type ParameterType { get; }

                public InjectValuePlaceholder(Type parameterType) {
                    this.ParameterType = parameterType;
                }
            }
        }

        public readonly struct ResolverRegistrationBuilder<T> {
            private readonly Resolver resolver;

            public ResolverRegistrationBuilder(Resolver resolver) {
                this.resolver = resolver;
            }

            public void To(T instance) {
                this.resolver.RegisterInternal(typeof(T), instance);
            }

            public async UniTask<TImpl> ToAsync<TImpl>(bool lazyInject = false) where TImpl : class, T {
                var instance = (TImpl) await this.resolver.CreateInstanceInternalAsync(typeof(TImpl), lazyInject);
                this.resolver.RegisterInternal(typeof(T), instance);
                return instance;
            }

            public async UniTask<TImpl> ToAsync<TImpl, T1>(T1 arg1) where TImpl : class, T {
                var instance = (TImpl) await this.resolver.CreateInstanceInternalAsync(typeof(TImpl), arg1);
                this.resolver.RegisterInternal(typeof(T), instance);
                return instance;
            }
        }

        public class ScriptableModuleTypeResolveException : Exception {
            public ScriptableModuleTypeResolveException(string message, Exception innerException)
                : base(message + Environment.NewLine + innerException?.Message) {
            }
        }
    }
}