namespace Game.Shared.UserProfile.Commands.Tutorials {
    using System;
    using MessagePack;
    using Multicast;
    using System.Threading.Tasks;
    using Data;

    [MessagePackObject, RequireFieldsInit]
    public class UserProfileCompleteTutorialCommand : IUserProfileServerCommand, IServerCommandExecutableFromClient {
        [Key(0)] public string TutorialId;
    }

    public class UserProfileCompleteTutorialCommandHandler : UserProfileServerCommandHandler<UserProfileCompleteTutorialCommand> {
        private readonly GameDef gameDef;

        public UserProfileCompleteTutorialCommandHandler(GameDef gameDef) {
            this.gameDef = gameDef;
        }

        public override async Task<ServerCommandResult> Execute(UserProfileServerCommandContext context, SdUserProfile gameData, UserProfileCompleteTutorialCommand command) {
            if (!this.gameDef.Tutorials.TryGet(command.TutorialId, out var tutorialDef)) {
                return BadRequest("Tutorial key not exist");
            }

            gameData.Tutorials.SetCompleted(tutorialDef.key);

            return Ok;
        }
    }
}