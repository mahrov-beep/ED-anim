namespace Game.Shared.UserProfile.Commands.GameModes {
    using System;
    using MessagePack;
    using Multicast;
    using System.Threading.Tasks;
    using Data;

    [MessagePackObject, RequireFieldsInit]
    public class UserProfileSelectGameModeCommand : IUserProfileServerCommand, IServerCommandExecutableFromClient {
        [Key(0)] public string GameModeKey;
    }

    public class UserProfileSelectGameModeCommandHandler : UserProfileServerCommandHandler<UserProfileSelectGameModeCommand> {
        private readonly GameDef gameDef;

        public UserProfileSelectGameModeCommandHandler(GameDef gameDef) {
            this.gameDef = gameDef;
        }

        public override async Task<ServerCommandResult> Execute(UserProfileServerCommandContext context, SdUserProfile gameData, UserProfileSelectGameModeCommand command) {
            if (!this.gameDef.GameModes.TryGet(command.GameModeKey, out var gameModeDef)) {
                return BadRequest("GameMode not exist");
            }

            if (!gameModeDef.visible) {
                return BadRequest("GameMoode not visible");
            }

            gameData.GameModes.SelectedGameMode.Value = gameModeDef.key;
            return Ok;
        }
    }
}