namespace Game.Shared.UserProfile.Commands.Loadouts {
    using System;
    using MessagePack;
    using Multicast;
    using System.Threading.Tasks;
    using Data;
    using Quantum;

    [MessagePackObject, RequireFieldsInit]
    public class TestSetLoadoutCommand : IUserProfileServerCommand {
        [Key(0)] public GameSnapshotLoadout LoadoutSnapshot;
    }

    public class TestSetLoadoutCommandHandler : UserProfileServerCommandHandler<TestSetLoadoutCommand> {
        public override async Task<ServerCommandResult> Execute(UserProfileServerCommandContext context, SdUserProfile gameData, TestSetLoadoutCommand command) {
            if (command.LoadoutSnapshot == null) {
                return BadRequest("LoadoutSnapshot is null");
            }

            var selectedGuid = gameData.Loadouts.SelectedLoadout.Value;
            
            if (string.IsNullOrEmpty(selectedGuid) || !gameData.Loadouts.Lookup.TryGetValue(selectedGuid, out var loadout)) {
                loadout = gameData.Loadouts.Lookup.Create(Guid.NewGuid().ToString());
                gameData.Loadouts.SelectedLoadout.Value = loadout.Guid;
            }

            if (command.LoadoutSnapshot.SlotItems != null) {
                foreach (var item in command.LoadoutSnapshot.SlotItems) {
                    if (item != null && !string.IsNullOrEmpty(item.ItemGuid)) {
                        var storageItem = gameData.Storage.Lookup.GetOrCreate(item.ItemGuid, out _);
                        storageItem.Item.Value    = item;
                        storageItem.IndexI.Value  = item.IndexI;
                        storageItem.IndexJ.Value  = item.IndexJ;
                        storageItem.Rotated.Value = item.Rotated;
                    }
                }
            }
            if (command.LoadoutSnapshot.TrashItems != null) {
                foreach (var item in command.LoadoutSnapshot.TrashItems) {
                    if (item != null && !string.IsNullOrEmpty(item.ItemGuid)) {
                        var storageItem = gameData.Storage.Lookup.GetOrCreate(item.ItemGuid, out _);
                        storageItem.Item.Value    = item;
                        storageItem.IndexI.Value  = item.IndexI;
                        storageItem.IndexJ.Value  = item.IndexJ;
                        storageItem.Rotated.Value = item.Rotated;
                    }
                }
            }

            loadout.LoadoutSnapshot.Value = command.LoadoutSnapshot;

            return Ok;
        }
    }
}

