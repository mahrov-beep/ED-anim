namespace Game.Shared.UserProfile.Commands {
    using System;
    using MessagePack;
    using Multicast;
    using System.Threading.Tasks;
    using Data;

    [MessagePackObject, RequireFieldsInit]
    public class UserProfileFetchCommand : IUserProfileServerCommand, IServerCommandExecutableFromClient {
    }

    public class UserProfileFetchCommandHandler : UserProfileServerCommandHandler<UserProfileFetchCommand> {
        private readonly GameDef gameDef;

        public UserProfileFetchCommandHandler(GameDef gameDef) {
            this.gameDef = gameDef;
        }

        public override async Task<ServerCommandResult> Execute(UserProfileServerCommandContext context, SdUserProfile gameData, UserProfileFetchCommand command) {
            return Ok;
        }
    }
}