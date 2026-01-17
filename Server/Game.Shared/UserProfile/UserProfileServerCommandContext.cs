namespace Game.Shared.UserProfile {
    using System;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using Multicast;

    public class UserProfileServerCommandContext : IServerCommandContext {
        private readonly Func<IUserProfileServerCommand, Task> commandExecutor;

        public UserProfileServerCommandContext(Func<IUserProfileServerCommand, Task> commandExecutor) {
            this.commandExecutor = commandExecutor;
        }

        [PublicAPI]
        public Task Execute(IUserProfileServerCommand command) {
            return this.commandExecutor(command);
        }
    }
}