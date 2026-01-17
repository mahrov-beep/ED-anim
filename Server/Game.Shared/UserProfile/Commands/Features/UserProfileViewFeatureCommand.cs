namespace Game.Shared.UserProfile.Commands.Features {
    using System;
    using MessagePack;
    using Multicast;
    using System.Threading.Tasks;
    using Data;

    [MessagePackObject, RequireFieldsInit]
    public class UserProfileViewFeatureCommand : IUserProfileServerCommand, IServerCommandExecutableFromClient {
        [Key(0)] public string FeatureKey;
    }

    public class UserProfileViewFeatureCommandHandler : UserProfileServerCommandHandler<UserProfileViewFeatureCommand> {
        private readonly GameDef gameDef;

        public UserProfileViewFeatureCommandHandler(GameDef gameDef) {
            this.gameDef = gameDef;
        }

        public override async Task<ServerCommandResult> Execute(UserProfileServerCommandContext context, SdUserProfile gameData, UserProfileViewFeatureCommand command) {
            if (!this.gameDef.Features.TryGet(command.FeatureKey, out var featureDef)) {
                return BadRequest("Feature not exist");
            }

            gameData.Features.Get(featureDef.key).Viewed.Value = true;

            return Ok;
        }
    }
}