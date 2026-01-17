namespace Multicast {
    using System;
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using JetBrains.Annotations;
    using Scellecs.Morpeh;
    using Sirenix.OdinInspector;
    using UniMob;
    using UniMob.UI.Widgets;
    using UnityEngine;
    using UnityEngine.Pool;
    using UnityEngine.Scripting;

    public enum ControllerStatus {
        Created      = 0,
        Activating   = 1,
        Running      = 2,
        RunForResult = 3,
        Disposing    = 4,
        Disposed     = 5,
    }

    public interface IControllerBase {
        [PublicAPI]
        Lifetime Lifetime { get; }

        [PublicAPI]
        bool IsRunning { get; }

        [PublicAPI]
        ControllerStatus Status { get; }
    }

    // ReSharper disable once UnusedTypeParameter
    public interface IControllerBase<TArgs>
        where TArgs : struct, IControllerArgsBase {
    }

    public interface IDisposableController : IControllerBase, IUniTaskAsyncDisposable {
    }

    public static class ControllersShared {
        [RuntimeInitializeOnLoadMethod]
        private static void Reset() {
            ActiveFlowControllers.Clear();
            RootController = null;
        }

        internal static readonly Stack<ControllerBase> ActiveFlowControllers = new();

        public static ControllerBase RootController { get; private set; }

        public static async UniTask RunRootController<TControllerArgs>(TControllerArgs args)
            where TControllerArgs : struct, IFlowControllerArgs {
            if (RootController != null) {
                throw new InvalidOperationException("RootController already running");
            }

            var controller = InstantiateController(args);

            RootController = controller;

            var activation = ControllerBase.ActivationGrabScope.Create(ControllersShared.RootController);
            try {
                await controller.Activate();
            }
            finally {
                activation.Dispose();
            }
        }

        public static void StopRootController() {
            RootController?.Stop();
            RootController = null;
        }

        [CanBeNull]
        public static ControllerBase ActiveFlowController =>
            ActiveFlowControllers.Count > 0 ? ActiveFlowControllers.Peek() : null;

        internal static ControllerBase<TArgs> InstantiateController<TArgs>(TArgs args)
            where TArgs : struct, IControllerArgsBase {
            var factory = ControllerCache<TArgs>.Factory;

            if (factory == null) {
                throw new InvalidOperationException($"No factory registered for controller with args {typeof(TArgs).Name}");
            }

            var controller = (ControllerBase<TArgs>)factory.Invoke();
            controller.SetArgs(args);
            return controller;
        }

        public static bool IsControllerRegisteredForArgs<TArgs>() where TArgs : struct, IControllerArgsBase {
            return ControllerCache<TArgs>.Factory != null;
        }

        public static void RegisterController<TControllerArgs, TController>()
            where TControllerArgs : IControllerArgsBase
            where TController : ControllerBase, new() {
            RegisterControllerFactory(typeof(TControllerArgs), () => new TController());
        }

        internal static void RegisterControllerFactory([NotNull] Type argsType, [NotNull] Func<IControllerBase> factory) {
            if (argsType == null) {
                throw new ArgumentNullException(nameof(argsType));
            }

            if (factory == null) {
                throw new ArgumentNullException(nameof(factory));
            }

            try {
                var runnerCache = typeof(ControllerCache<>).MakeGenericType(argsType);
                var setMethod   = runnerCache.GetMethod("SetFactory") ?? throw new Exception("SetFactory not exists");
                setMethod.Invoke(null, new object[] { factory });
            }
            catch (Exception) {
                Debug.LogWarning($"Failed to register FlowController<> for args '{argsType.Name}'");
            }
        }

        private static class ControllerCache<TArgs>
            where TArgs : struct, IControllerArgsBase {
            public static string Name { get; } = typeof(TArgs).Name;

            // ReSharper disable once StaticMemberInGenericType
            public static Func<IControllerBase> Factory { get; private set; }

            [Preserve]
            public static void SetFactory(Func<IControllerBase> factory) {
                Factory = factory;
            }
        }
    }

    public abstract class ControllerBase : ILifetimeScope, IControllerBase {
        internal static event Action<ControllerBase> ControllerChildrenChanged;
        internal static event Action<ControllerBase> ControllerStatusChanged;
        internal static event Action<ControllerBase> ActiveControllerChanged;

        private readonly List<FlowDelegate>   requestedFlows     = new();
        private readonly List<ControllerBase> children           = new();
        private readonly LifetimeController   lifetimeController = new();

        [PublicAPI]
        public Lifetime Lifetime => this.lifetimeController.Lifetime;

        [PublicAPI]
        [ShowInInspector, DisplayAsString, BoxGroup("Controller")]
        public ControllerStatus Status { get; internal set; } = ControllerStatus.Created;

        public bool IsRunning => this.Status is ControllerStatus.Running or ControllerStatus.RunForResult;

        [ShowInInspector, DisplayAsString, BoxGroup("Controller")]
        private int RequestedFlowCount => this.requestedFlows.Count;

        public List<ControllerBase> Children => this.children;

        public virtual string DebugName => null;

        public override string ToString() {
            return this.GetType().Name;
        }

        [PublicAPI]
        protected internal void Stop() {
            this.SetStatus(ControllerStatus.Disposed);
            this.lifetimeController.Dispose();
        }

        internal async UniTask Activate() {
            if (this.Lifetime.IsDisposed) {
                throw new InvalidOperationException("Failed to Activate controller with disposed lifetime");
            }

            this.SetStatus(ControllerStatus.Activating);
            var activation = ActivationGrabScope.Create(this);
            try {
                await this.Activate(new Context {
                    Controller = this,
                });
            }
            finally {
                activation.Dispose();
                this.SetStatus(ControllerStatus.Running);
            }

            this.CleanupChildren();
        }

        internal async UniTask ExecuteFlowHierarchical() {
            if (this.Lifetime.IsDisposed) {
                return;
            }

            var activation = ActivationGrabScope.Create(this);
            try {
                var context = new Context {
                    Controller = this,
                };

                while (this.requestedFlows.Count > 0) {
                    using (ListPool<FlowDelegate>.Get(out var tempFlows)) {
                        tempFlows.AddRange(this.requestedFlows);
                        this.requestedFlows.Clear();

                        foreach (var flow in tempFlows) {
                            try {
                                await flow(context);
                            }
                            catch (Exception ex) {
                                Debug.LogException(ex);
                            }
                        }
                    }
                }

                await this.OnFlow(context);

                for (int i = 0, length = this.children.Count; i < length; i++) {
                    await this.children[i].ExecuteFlowHierarchical();
                }
            }
            finally {
                activation.Dispose();
            }

            this.CleanupChildren();
        }

        internal void UpdateHierarchical() {
            if (this.Lifetime.IsDisposed) {
                return;
            }

            this.OnUpdate();

            for (int i = 0, length = this.children.Count; i < length; i++) {
                this.children[i].UpdateHierarchical();
            }

            this.CleanupChildren();
        }

        internal void SetStatus(ControllerStatus status) {
            this.Status = status;

            ControllerStatusChanged?.Invoke(this);
        }

        internal void CleanupChildren() {
            if (this.children.RemoveAll(static it => it.Status == ControllerStatus.Disposed) > 0) {
                ControllerChildrenChanged?.Invoke(this);
            }
        }

        [PublicAPI]
        protected void RequestFlow(FlowDelegate call, FlowOptions options = FlowOptions.Queued) {
            if (options == FlowOptions.NowOrNever && ControllersShared.ActiveFlowController != null) {
                return;
            }

            this.requestedFlows.Add(call);
            App.RequestAppUpdateFlow();
        }

        [PublicAPI]
        protected void RequestFlow<TArgs>(FlowDelegate<TArgs> call, TArgs args, FlowOptions options = FlowOptions.Queued) {
            this.RequestFlow(context => call(context, args), options);
        }

        [PublicAPI]
        protected ActivationGrabScope Experimental_GrabActivationFrom(Context callerContext, out Context context) {
            if (ControllersShared.ActiveFlowController != callerContext.Controller) {
                throw new InvalidOperationException("Cannot grab context from non active controller");
            }

            context = new Context {
                Controller = this,
            };

            return ActivationGrabScope.Create(this);
        }

        [PublicAPI]
        protected virtual void OnUpdate() {
        }

        [PublicAPI]
        protected virtual UniTask Activate(Context context) {
            return UniTask.CompletedTask;
        }

        [PublicAPI]
        protected virtual UniTask OnFlow(Context context) {
            return UniTask.CompletedTask;
        }

        private async UniTask<IControllerBase> RunChild<TChildArgs>(TChildArgs args)
            where TChildArgs : struct, IControllerArgsBase {
            if (ControllersShared.ActiveFlowController is var currentFlowController && currentFlowController != this) {
                Debug.LogError($"RunChild in '{this.GetType().Name}' but current flow controller is {currentFlowController?.GetType().Name}");
            }

            if (this.Lifetime.IsDisposed) {
                throw new InvalidOperationException($"Cannot RunChild({typeof(TChildArgs).Name}) on disposed controller '{this.GetType().Name}'");
            }

            var child = ControllersShared.InstantiateController(args);

            this.Lifetime.Register(child.Stop);

            this.children.Add(child);
            ControllerChildrenChanged?.Invoke(this);

            child.Lifetime.Register(() => this.CleanupChildren());

            await child.Activate();

            return child;
        }

        protected delegate UniTask FlowDelegate(Context context);

        protected delegate UniTask FlowDelegate<in TArgs>(Context context, TArgs args);

        [RequireFieldsInit]
        public struct Context {
            public ControllerBase Controller;

            /// <summary>
            /// Запускает вложенный контроллер.
            /// </summary>
            [PublicAPI]
            public async UniTask<IControllerBase> RunChild<TChildArgs>(TChildArgs args)
                where TChildArgs : struct, IFlowControllerArgs {
                return await this.Controller.RunChild(args);
            }

            /// <summary>
            /// Запускает вложенный контроллер.
            /// </summary>
            [PublicAPI]
            public async UniTask<IDisposableController> RunDisposable<TChildArgs>(TChildArgs args)
                where TChildArgs : struct, IDisposableControllerArgs {
                return (IDisposableController)await this.Controller.RunChild(args);
            }

            /// <summary>
            /// Запускает контроллер и дожидается завершения его работы.<br/>
            /// </summary>
            [PublicAPI]
            public async UniTask RunForResult<TChildArgs>(TChildArgs args)
                where TChildArgs : struct, IResultControllerArgs {
                await this.Controller.RunChild(args);
            }

            /// <summary>
            /// Запускает контроллер, дожидается завершения его работы, и возвращает результат.<br/>
            /// Вторым аргументом нужно передать default(TResult) чтобы помочь компилятору определить типы<br/>
            /// Пример: bool result = context.RunForResult(new MyControllerWithBoolResult(), default(bool));
            /// </summary>
            [PublicAPI]
            public async UniTask<TResult> RunForResult<TChildArgs, TResult>(TChildArgs args, TResult _)
                where TChildArgs : struct, IResultControllerArgs<TResult> {
                var child = await this.Controller.RunChild(args);
                return ((ResultController<TChildArgs, TResult>)child).Result;
            }

            [PublicAPI]
            public NavigatorState RootNavigator => App.Current.GetNavigator(AppNavigatorType.Root);

            [PublicAPI]
            public NavigatorState GetNavigator(AppNavigatorType type) => App.Current.GetNavigator(type);

            /// <summary>
            /// Выполняет команду.<br/>
            /// Пример: context.Execute(new MyCommand());
            /// </summary>
            [PublicAPI]
            public void Execute<TCommand>(TCommand command) where TCommand : struct, ICommand {
                App.Current.ExecuteWithTransaction(command);
            }

            /// <summary>
            /// Выполняет команду и возвращает результат.<br/>
            /// Вторым аргументом нужно передать default(TResult) чтобы помочь компилятору отпределить типы<br/>
            /// Пример: bool result = context.Execute(new MyCommandWithBoolResult(), default(bool));
            /// </summary>
            [PublicAPI]
            [MustUseReturnValue]
            public TResult Execute<TCommand, TResult>(TCommand command, TResult _) where TCommand : struct, ICommand<TResult> {
                return App.Current.ExecuteWithTransaction<TCommand, TResult>(command);
            }

            [PublicAPI]
            public ServerRequests Server => new ServerRequests();

            [PublicAPI]
            public void PublishEvent<TEvent>(TEvent data) where TEvent : struct, IEventData {
                World.Default.GetEvent<TEvent>().NextFrame(data);
            }
        }

        public struct ActivationGrabScope : IDisposable {
            public static ActivationGrabScope Create(ControllerBase controller) {
                ControllersShared.ActiveFlowControllers.Push(controller);
                ActiveControllerChanged?.Invoke(ControllersShared.ActiveFlowController);
                return new ActivationGrabScope();
            }

            public void Dispose() {
                ControllersShared.ActiveFlowControllers.Pop();
                ActiveControllerChanged?.Invoke(ControllersShared.ActiveFlowController);
            }
        }

        public enum FlowOptions {
            /// <summary>
            /// Добавляет задачу в очередь на выполнение.
            /// Задача будет выполнена в любом случае, но возможно через некоторое время.
            /// </summary>
            Queued,

            /// <summary>
            /// Добавляет задачу в очередь на выполнение ТОЛЬКО если в данный момент не выполняются другие задачи,
            /// то есть задача будет выполнена сразу или никогда.
            /// </summary>
            NowOrNever,
        }
    }

    public abstract class ControllerBase<TArgs> : ControllerBase, IControllerBase<TArgs>
        where TArgs : struct, IControllerArgsBase {
        [PublicAPI]
        [ShowInInspector, ReadOnly, InlineProperty, HideLabel, BoxGroup("Controller Args")]
        protected TArgs Args { get; private set; }

        internal void SetArgs(TArgs args) {
            this.Args = args;
        }
    }

    public interface IControllerArgsBase {
    }

    public interface IFlowControllerArgs : IControllerArgsBase {
    }

    public interface IDisposableControllerArgs : IControllerArgsBase {
    }

    // ReSharper disable once UnusedTypeParameter
    public interface IResultControllerArgs<TResult> : IControllerArgsBase {
    }

    public interface IResultControllerArgs : IControllerArgsBase {
    }

    public abstract class ResultController<TArgs, TResult> : ControllerBase<TArgs>
        where TArgs : struct, IResultControllerArgs<TResult> {
        internal TResult Result { get; private set; }

        protected sealed override async UniTask Activate(Context context) {
            try {
                this.SetStatus(ControllerStatus.RunForResult);
                this.Result = await this.Execute(context);
            }
            finally {
                this.Stop();
            }
        }

        [PublicAPI]
        protected abstract UniTask<TResult> Execute(Context context);
    }

    public abstract class ResultController<TArgs> : ControllerBase<TArgs>
        where TArgs : struct, IResultControllerArgs {
        protected sealed override async UniTask Activate(Context context) {
            try {
                this.SetStatus(ControllerStatus.RunForResult);
                await this.Execute(context);
            }
            finally {
                this.Stop();
            }
        }

        [PublicAPI]
        protected abstract UniTask Execute(Context context);
    }

    public abstract class FlowController<TArgs> : ControllerBase<TArgs>
        where TArgs : struct, IFlowControllerArgs {
    }

    public abstract class DisposableController<TArgs> : ControllerBase<TArgs>, IDisposableController
        where TArgs : struct, IDisposableControllerArgs {
        [Obsolete("Do not use Stop() in DisposableControllers. Use await DisposeAsync() instead", true)]
        protected internal new void Stop() {
            base.Stop();
        }

        [PublicAPI]
        public async UniTask DisposeAsync() {
            if (this.Status is ControllerStatus.Disposing or ControllerStatus.Disposed) {
                return;
            }

            this.SetStatus(ControllerStatus.Disposing);

            var activation = ActivationGrabScope.Create(this);
            try {
                await this.OnDisposeAsync(new Context {
                    Controller = this,
                });
            }
            finally {
                activation.Dispose();
                base.Stop();
            }
        }

        [PublicAPI]
        protected virtual UniTask OnDisposeAsync(Context context) {
            return UniTask.CompletedTask;
        }
    }

    public static class ControllerExtensions {
        [PublicAPI]
        public static async UniTask DisposeAsyncNullable([CanBeNull] this IDisposableController controller) {
            if (controller != null) {
                await controller.DisposeAsync();
            }
        }
    }
}