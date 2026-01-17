namespace Game.Shared.UserProfile.Commands.Storage {
    using System.Threading.Tasks;
    using Data;
    using MessagePack;
    using Multicast;

    [MessagePackObject, RequireFieldsInit]
    public class UserProfileUpgradeStorageCommand : IUserProfileServerCommand, IServerCommandExecutableFromClient {
        [Key(0)] public int WidthIncrease;
        [Key(1)] public int HeightIncrease;
    }

    public class UserProfileUpgradeStorageCommandHandler : UserProfileServerCommandHandler<UserProfileUpgradeStorageCommand> {
        public override Task<ServerCommandResult> Execute(UserProfileServerCommandContext context, SdUserProfile gameData, UserProfileUpgradeStorageCommand command) {
            if (command.WidthIncrease < 0 || command.HeightIncrease < 0) {
                return Task.FromResult(BadRequest("Invalid upgrade values"));
            }

            var currentWidth = gameData.StorageWidth.Value;
            var currentHeight = gameData.StorageHeight.Value;

            var maxWidth = 20;
            var maxHeight = 15;

            var newWidth = currentWidth + command.WidthIncrease;
            var newHeight = currentHeight + command.HeightIncrease;

            if (newWidth > maxWidth || newHeight > maxHeight) {
                return Task.FromResult(BadRequest($"Storage size cannot exceed {maxWidth}x{maxHeight}"));
            }

            gameData.StorageWidth.Value = newWidth;
            gameData.StorageHeight.Value = newHeight;

            return Task.FromResult(Ok);
        }
    }
}






