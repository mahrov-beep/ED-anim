namespace Multicast {
    using System;
    using JetBrains.Annotations;
    using Scellecs.Morpeh;

    public interface ICommand {
    }

    public interface ICommand<TResult> {
    }

    public interface ICommandHandlerBase {
    }

    public interface ICommandHandler<in TCommand> : ICommandHandlerBase
        where TCommand : struct, ICommand {
        void Execute(CommandContext context, TCommand command);
    }

    public interface ICommandHandler<in TCommand, out TResult> : ICommandHandlerBase {
        TResult Execute(CommandContext context, TCommand command);
    }

    public class CommandContext {
        internal bool UserDataSaveRequested { get; set; }

        /// <summary>
        /// Выполняет команду.<br/>
        /// Пример: context.Execute(new MyCommand());
        /// </summary>
        [PublicAPI]
        public void Execute<TCommand>(TCommand command) where TCommand : struct, ICommand {
            App.Current.ExecuteDirect(command);
        }

        /// <summary>
        /// Выполняет команду и возвращает результат.<br/>
        /// Вторым аргументом нужно передать default(TResult) чтобы помочь компилятору отпределить типы<br/>
        /// Пример: bool result = context.Execute(new MyCommandWithBoolResult(), default(bool));
        /// </summary>
        [PublicAPI]
        [MustUseReturnValue]
        public TResult Execute<TCommand, TResult>(TCommand command, TResult _) where TCommand : struct, ICommand<TResult> {
            return App.Current.ExecuteDirect<TCommand, TResult>(command);
        }

        [PublicAPI]
        public void PublishEvent<TEvent>(TEvent data) where TEvent : struct, IEventData {
            World.Default.GetEvent<TEvent>().NextFrame(data);
        }

        [PublicAPI]
        public void RequestUserDataSave() {
            this.UserDataSaveRequested = true;
        }
    }

    [Obsolete("INonLoggedCommandHandler is no longer used. Use IDebugLoggedCommandHandler instead", true)]
    public interface INonLoggedCommandHandler {
    }

    public interface IDebugLoggedCommandHandler {
    }
}