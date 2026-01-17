namespace Game.Shared.UserProfile.Commands.Rewards.Impl {
    using System;
    using MessagePack;
    using Multicast;
    using System.Threading.Tasks;
    using Data;
    using Multicast.Numerics;
    using Quantum;
    using Quantum.CellsInventory;
    using Helpers;

    [MessagePackObject, RequireFieldsInit]
    public class UserProfileOpenItemRewardCommand : IUserProfileServerCommand {
        [Key(0)] public Reward Reward;
    }

    public class UserProfileOpenItemRewardCommandHandler : UserProfileServerCommandHandler<UserProfileOpenItemRewardCommand> {
        private readonly GameDef gameDef;

        public UserProfileOpenItemRewardCommandHandler(GameDef gameDef) {
            this.gameDef = gameDef;
        }

        public override async Task<ServerCommandResult> Execute(UserProfileServerCommandContext context, SdUserProfile gameData, UserProfileOpenItemRewardCommand command) {
            var reward = command.Reward;

            if (!reward.ItemTypeIs(SharedConstants.RewardTypes.ITEM)) {
                return BadRequest("Not a item drop");
            }

            if (!reward.AmountTypeIs(RewardAmountType.Int)) {
                return BadRequest("Item drop must be with int amount");
            }

            if (!this.gameDef.Items.TryGet(reward.ItemKey, out var itemDef)) {
                return BadRequest("Item drop must be with valid item key");
            }

            var ranges = new CellsRange[] { CellsRange.FromIJWH(0, 0, itemDef.CellsWidth, itemDef.CellsHeight, true) };
            
            if (StoragePlacementHelper.TryFindPlaceInStorage(this.gameDef, gameData, ranges, out var foundRanges)) {
                var item = new GameSnapshotLoadoutItem {
                    ItemGuid              = Guid.NewGuid().ToString(),
                    ItemKey               = itemDef.key,
                    WeaponAttachments     = null,
                    IndexI                = (byte)ranges[0].I,
                    IndexJ                = (byte)ranges[0].J,
                    Rotated               = ranges[0].Rotated,
                    Used                  = 0,
                    SafeGuid              = null,
                    AddToLoadoutAfterFail = false,
                };
                
                StoragePlacementHelper.PlaceItemInStorage(gameData, item.ItemGuid, item, foundRanges[0]);
                
                return Ok;
            }
            
            return BadRequest("Not enough space in storage");
        }
    }
}