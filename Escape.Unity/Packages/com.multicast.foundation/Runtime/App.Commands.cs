namespace Multicast {
    using System;
    using System.Collections.Generic;
    using JetBrains.Annotations;
    using UniMob;
    using UnityEngine;
    using UnityEngine.Scripting;
    using UserData;

    public partial class App {
        private static readonly UdDebugStringDataChangeSet SharedDebugStringDataChangeSet = new UdDebugStringDataChangeSet();

        private readonly Stack<ICommandHandlerBase> executingCommandHandlers = new Stack<ICommandHandlerBase>();

        internal readonly CommandContext CommandContext = new CommandContext();

        /// <summary>
        /// Выполняет команду.<br/>
        /// Пример: App.Execute(new MyCommand());
        /// </summary>
        [PublicAPI]
        public static void Execute<TCommand>(TCommand command) where TCommand : struct, ICommand {
            Current.ExecuteWithTransaction(command);
        }

        /// <summary>
        /// Выполняет команду и возвращает результат.<br/>
        /// Вторым аргументом нужно передать default(TResult) чтобы помочь компилятору отпределить типы<br/>
        /// Пример: bool result = App.Execute(new MyCommandWithBoolResult(), default(bool));
        /// </summary>
        [PublicAPI]
        [MustUseReturnValue]
        public static TResult Execute<TCommand, TResult>(TCommand command, TResult _) where TCommand : struct, ICommand<TResult> {
            return Current.ExecuteWithTransaction<TCommand, TResult>(command);
        }

        internal void ExecuteWithTransaction<TCommand>(TCommand command)
            where TCommand : struct, ICommand {
            var commandName = CommandHandlerCache<TCommand>.Name;
            var handler     = CommandHandlerCache<TCommand>.Handler;

            var userDataService = UserDataService;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (Atom.CurrentScope != null) {
                throw new InvalidOperationException($"Command '{commandName}' cannot be executed in observable scope");
            }

            if (userDataService.Root.TryGetActiveTransaction(out var transactionId)) {
                throw new InvalidOperationException($"Command '{commandName}' cannot be executed inside transaction '{transactionId}'");
            }

            if (handler == null) {
                throw new InvalidOperationException($"Command '{commandName}' cannot be executed, no handler registered");
            }
#endif

            if (CommandHandlerCache<TCommand>.IsLoggingEnabled) {
                this.ReportCommandToAnalytics(CommandHandlerCache<TCommand>.JsonPayload);
            }

            {
                userDataService.Root.BeginTransaction(commandName);
                {
                    this.executingCommandHandlers.Push(handler);
                    {
                        handler.Execute(this.CommandContext, command);
                    }
                    this.executingCommandHandlers.Pop();
                }
                this.CommitTransactionWithLog(commandName, CommandHandlerCache<TCommand>.IsLoggingEnabled);
            }

            this.SaveUserDataIfRequired();
        }

        internal void ExecuteDirect<TCommand>(TCommand command)
            where TCommand : struct, ICommand {
            var commandName = CommandHandlerCache<TCommand>.Name;
            var handler     = CommandHandlerCache<TCommand>.Handler;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (handler == null) {
                throw new InvalidOperationException($"Command '{commandName}' cannot be executed, no handler registered");
            }
#endif

            handler.Execute(this.CommandContext, command);
        }

        internal TResult ExecuteWithTransaction<TCommand, TResult>(TCommand command)
            where TCommand : struct, ICommand<TResult> {
            var commandName = CommandHandlerCache<TCommand, TResult>.Name;
            var handler     = CommandHandlerCache<TCommand, TResult>.Handler;

            var userDataService = UserDataService;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (Atom.CurrentScope != null) {
                throw new InvalidOperationException($"Command '{commandName}' cannot be executed in observable scope");
            }

            if (userDataService.Root.TryGetActiveTransaction(out var transactionId)) {
                throw new InvalidOperationException($"Command '{commandName}' cannot be executed inside transaction '{transactionId}'");
            }

            if (handler == null) {
                throw new InvalidOperationException($"Command '{commandName}' cannot be executed, no handler registered");
            }
#endif

            if (CommandHandlerCache<TCommand, TResult>.IsLoggingEnabled) {
                this.ReportCommandToAnalytics(CommandHandlerCache<TCommand, TResult>.JsonPayload);
            }

            TResult result;
            {
                userDataService.Root.BeginTransaction(commandName);
                {
                    this.executingCommandHandlers.Push(handler);
                    {
                        result = handler.Execute(this.CommandContext, command);
                    }
                    this.executingCommandHandlers.Pop();
                }
                this.CommitTransactionWithLog(commandName, CommandHandlerCache<TCommand, TResult>.IsLoggingEnabled);
            }

            this.SaveUserDataIfRequired();

            return result;
        }

        internal TResult ExecuteDirect<TCommand, TResult>(TCommand command)
            where TCommand : struct, ICommand<TResult> {
            var commandName = CommandHandlerCache<TCommand, TResult>.Name;
            var handler     = CommandHandlerCache<TCommand, TResult>.Handler;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (handler == null) {
                throw new InvalidOperationException($"Command '{commandName}' cannot be executed, no handler registered");
            }
#endif

            var result = handler.Execute(this.CommandContext, command);

            return result;
        }

        private void ReportCommandToAnalytics(string payload) {
            CoreAnalytics.ReportEvent("command", payload);
        }

        private void CommitTransactionWithLog(string commandName, bool isLoggingEnabled) {
            var userDataService = UserDataService;
            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (isLoggingEnabled && Debug.unityLogger.IsLogTypeAllowed(LogType.Log)) {
                SharedDebugStringDataChangeSet.Clear();

                userDataService.Root.CommitTransaction(SharedDebugStringDataChangeSet);

                Debug.Log($"Execute {commandName}:\n{SharedDebugStringDataChangeSet.Stringify()}");
            }
            else
#endif
            {
                userDataService.Root.CommitTransaction();
            }
        }

        private void SaveUserDataIfRequired() {
            if (!this.CommandContext.UserDataSaveRequested || this.executingCommandHandlers.Count > 0) {
                return;
            }

            var userDataService = UserDataService;

            this.CommandContext.UserDataSaveRequested = false;

            userDataService.SaveUserData();
        }

        internal void RegisterCommandHandler([NotNull] Type commandType, [NotNull] ICommandHandlerBase handler) {
            if (commandType == null) {
                throw new ArgumentNullException(nameof(commandType));
            }

            if (handler == null) {
                throw new ArgumentNullException(nameof(handler));
            }

            try {
                var handlerCache = typeof(CommandHandlerCache<>).MakeGenericType(commandType);
                var setMethod    = handlerCache.GetMethod("SetHandler") ?? throw new Exception("SetHandler not exists");
                setMethod.Invoke(null, new object[] {handler});
            }
            catch (Exception) {
                Debug.LogWarning($"Failed to register command handler for command '{commandType.Name}'");
            }
        }

        internal void RegisterCommandHandler([NotNull] Type commandType, [NotNull] Type resultType, [NotNull] ICommandHandlerBase handler) {
            if (commandType == null) {
                throw new ArgumentNullException(nameof(commandType));
            }

            if (resultType == null) {
                throw new ArgumentNullException(nameof(resultType));
            }

            if (handler == null) {
                throw new ArgumentNullException(nameof(handler));
            }

            try {
                var handlerCache = typeof(CommandHandlerCache<,>).MakeGenericType(commandType, resultType);
                var setMethod    = handlerCache.GetMethod("SetHandler") ?? throw new Exception("SetHandler not exists");
                setMethod.Invoke(null, new object[] {handler});
            }
            catch (Exception) {
                Debug.LogWarning($"Failed to register command handler for command '{commandType.Name}'");
            }
        }

        private static class CommandHandlerCache<TCommand>
            where TCommand : struct, ICommand {
            public static string Name { get; } = typeof(TCommand).Name;

            internal static string JsonPayload { get; } = $"{{\"command\": \"{typeof(TCommand).Name}\"}}";

            public static ICommandHandler<TCommand> Handler { get; private set; }

            internal static bool IsLoggingEnabled { get; private set; }

            [Preserve]
            public static void SetHandler(ICommandHandler<TCommand> handler) {
                Handler          = handler;
                IsLoggingEnabled = handler is IDebugLoggedCommandHandler;
            }
        }

        private static class CommandHandlerCache<TCommand, TResult>
            where TCommand : struct, ICommand<TResult> {
            public static string Name { get; } = typeof(TCommand).Name;

            internal static string JsonPayload { get; } = $"{{\"command\": \"{typeof(TCommand).Name}\"}}";

            public static ICommandHandler<TCommand, TResult> Handler { get; private set; }

            internal static bool IsLoggingEnabled { get; private set; }

            [Preserve]
            public static void SetHandler(ICommandHandler<TCommand, TResult> handler) {
                Handler          = handler;
                IsLoggingEnabled = handler is IDebugLoggedCommandHandler;
            }
        }
    }
}