namespace Game.Shared.UserProfile.Commands.Storage {
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Data;
    using MessagePack;
    using Multicast;

    [MessagePackObject, RequireFieldsInit]
    public class UserProfileUpdateStorageItemPositionCommand : IUserProfileServerCommand, IServerCommandExecutableFromClient {
        [Key(0)] public string ItemGuid;
        [Key(1)] public int IndexI;
        [Key(2)] public int IndexJ;
        [Key(3)] public bool Rotated;
    }

    public class UserProfileUpdateStorageItemPositionCommandHandler : UserProfileServerCommandHandler<UserProfileUpdateStorageItemPositionCommand> {
        public override Task<ServerCommandResult> Execute(UserProfileServerCommandContext context, SdUserProfile gameData, UserProfileUpdateStorageItemPositionCommand command) {
            if (!gameData.Storage.Lookup.TryGetValue(command.ItemGuid, out var storageItem)) {
                return Task.FromResult(BadRequest("Item not found in storage"));
            }

            storageItem.IndexI.Value = command.IndexI;
            storageItem.IndexJ.Value = command.IndexJ;
            storageItem.Rotated.Value = command.Rotated;

            return Task.FromResult(Ok);
        }
    }
}





