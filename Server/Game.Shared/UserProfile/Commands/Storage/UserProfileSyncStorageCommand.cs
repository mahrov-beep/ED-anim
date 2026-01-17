namespace Game.Shared.UserProfile.Commands.Storage {
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Data;
    using MessagePack;
    using Multicast;
    using Quantum;

    [MessagePackObject, RequireFieldsInit]
    public class UserProfileSyncStorageCommand : IUserProfileServerCommand, IServerCommandExecutableFromClient {
        [Key(0)] public GameSnapshotStorage StorageSnapshot;
        [Key(1)] public string              LoadoutGuid;
    }

    public class UserProfileSyncStorageCommandHandler : UserProfileServerCommandHandler<UserProfileSyncStorageCommand> {
        private readonly GameDef gameDef;

        public UserProfileSyncStorageCommandHandler(GameDef gameDef) {
            this.gameDef = gameDef;
        }

        public override Task<ServerCommandResult> Execute(UserProfileServerCommandContext context, SdUserProfile gameData, UserProfileSyncStorageCommand command) {
            var oldItems = gameData.Storage.Lookup.ToList();

            for (var i = oldItems.Count - 1; i >= 0; i--) {
                gameData.Storage.Lookup.Remove(oldItems[i]);
            }

            var snapshotItems = command.StorageSnapshot?.items;

            if (snapshotItems == null) {
                return Task.FromResult(Ok);
            }

            foreach (var snapshotItem in snapshotItems) {
                if (snapshotItem == null) {
                    continue;
                }

                if (string.IsNullOrEmpty(snapshotItem.ItemKey)) {
                    return Task.FromResult(BadRequest("Storage item must have a valid item key"));
                }

                if (!this.gameDef.Items.TryGet(snapshotItem.ItemKey, out _)) {
                    return Task.FromResult(BadRequest($"Storage item key '{snapshotItem.ItemKey}' is invalid"));
                }

                var itemGuid = string.IsNullOrEmpty(snapshotItem.ItemGuid)
                    ? Guid.NewGuid().ToString()
                    : snapshotItem.ItemGuid;

                var storageItem = gameData.Storage.Lookup.GetOrCreate(itemGuid, out _);

                var snapshotCopy = snapshotItem.DeepClone();
                snapshotCopy.ItemGuid = itemGuid;

                storageItem.Item.Value    = snapshotCopy;
                storageItem.IndexI.Value  = snapshotCopy.IndexI;
                storageItem.IndexJ.Value  = snapshotCopy.IndexJ;
                storageItem.Rotated.Value = snapshotCopy.Rotated;
            }

            return Task.FromResult(Ok);
        }
    }
}

