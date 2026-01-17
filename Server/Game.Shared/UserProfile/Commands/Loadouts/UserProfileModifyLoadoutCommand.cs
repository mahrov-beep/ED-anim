namespace Game.Shared.UserProfile.Commands.Loadouts {
    using System.Buffers;
    using System.Collections.Generic;
    using MessagePack;
    using Multicast;
    using System.Threading.Tasks;
    using Data;
    using Quantum;
    using Helpers;

    [MessagePackObject, RequireFieldsInit]
    public class UserProfileModifyLoadoutCommand : IUserProfileServerCommand, IServerCommandExecutableFromClient {
        [Key(0)] public string              LoadoutGuid;
        [Key(1)] public GameSnapshotLoadout LoadoutSnapshot;

        /// <summary>
        /// Идентификаторы всех измененных предметов.
        /// Критически важно чтобы в этом списке были предметы которые перекладываются в/из хранилище
        /// </summary>
        [Key(2)] public List<string> ModifiedItemGuids;
    }

    public class UserProfileModifyLoadoutCommandHandler : UserProfileServerCommandHandler<UserProfileModifyLoadoutCommand> {
        private readonly GameDef gameDef;

        public UserProfileModifyLoadoutCommandHandler(GameDef gameDef) {
            this.gameDef = gameDef;
        }

        public override async Task<ServerCommandResult> Execute(UserProfileServerCommandContext context, SdUserProfile gameData, UserProfileModifyLoadoutCommand command) {
            if (!gameData.Loadouts.Lookup.TryGetValue(command.LoadoutGuid, out var loadout)) {
                return BadRequest("loadout not exist");
            }

            if (!string.IsNullOrEmpty(loadout.LockedForGame.Value)) {
                return BadRequest("Cannot modify locked loadout");
            }

            if (command.ModifiedItemGuids == null || command.ModifiedItemGuids.Count == 0) {
                return BadRequest("No any item modified");
            }

            var prevLoadout = loadout.LoadoutSnapshot.Value;
            var nextLoadout = command.LoadoutSnapshot ?? new GameSnapshotLoadout { SlotItems = null, TrashItems = null };

            // если в новом лодауте нет скина, то оставляем старый
            if (!nextLoadout.TryGetItemAtSlot(CharacterLoadoutSlots.Skin, out _)) {
                if (prevLoadout.TryGetItemAtSlot(CharacterLoadoutSlots.Skin, out var prevSkin)) {
                    nextLoadout.SetItemToSlot(CharacterLoadoutSlots.Skin, prevSkin);
                }
            }

            var itemsToMoveToStorage = new List<GameSnapshotLoadoutItem>();
            var itemsToRemoveFromStorage = new List<string>();

            foreach (var itemGuid in command.ModifiedItemGuids) {
                var prevItem = prevLoadout.GetItemWithGuid(itemGuid);
                var nextItem = nextLoadout.GetItemWithGuid(itemGuid);

                // переложили предмет из хранилища в инвентарь
                if (prevItem == null && nextItem != null) {
                    if (gameData.Storage.Lookup.TryGetValue(itemGuid, out var itemData)) {
                        itemsToRemoveFromStorage.Add(itemGuid);
                    }
                    else {
                        return BadRequest("Failed to move item from storage to loadout, item not exist in storage");
                    }
                }

                // переложили предмет из инвентаря в хранилище
                if (prevItem != null && nextItem == null) {
                    itemsToMoveToStorage.Add(prevItem);
                }
            }

            // Сначала удаляем из storage
            foreach (var itemGuid in itemsToRemoveFromStorage) {
                if (gameData.Storage.Lookup.TryGetValue(itemGuid, out var itemData)) {
                    gameData.Storage.Lookup.Remove(itemData);
                }
            }

            if (itemsToMoveToStorage.Count > 0) {
                var newRanges = ArrayPool<CellsRange>.Shared.Rent(itemsToMoveToStorage.Count);

                try {
                    for (var i = 0; i < itemsToMoveToStorage.Count; i++) {
                        var item = itemsToMoveToStorage[i];
                        
                        if (!this.gameDef.Items.TryGet(item.ItemKey, out var itemDef)) {
                            return BadRequest($"Invalid item key: {item.ItemKey}");
                        }

                        newRanges[i] = CellsRange.FromIJWH(0, 0, itemDef.CellsWidth, itemDef.CellsHeight, true);
                    }

                    if (!StoragePlacementHelper.TryFindPlaceInStorage(this.gameDef, gameData, newRanges, out var foundRanges)) {
                        return BadRequest("Not enough space in storage");
                    }

                    for (var i = 0; i < itemsToMoveToStorage.Count; i++) {
                        var item = itemsToMoveToStorage[i];
                        StoragePlacementHelper.PlaceItemInStorage(gameData, item.ItemGuid, item, foundRanges[i]);
                    }
                }
                finally {
                    ArrayPool<CellsRange>.Shared.Return(newRanges, true);
                }
            }

            loadout.LoadoutSnapshot.Value = nextLoadout;

            return Ok;
        }
    }
}