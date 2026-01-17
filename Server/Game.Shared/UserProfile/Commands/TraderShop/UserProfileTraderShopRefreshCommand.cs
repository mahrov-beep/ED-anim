namespace Game.Shared.UserProfile.Commands.TraderShop {
    using System;
    using System.Linq;
    using MessagePack;
    using Multicast;
    using System.Threading.Tasks;
    using Data;
    using DTO;
    using Quantum;

    [MessagePackObject, RequireFieldsInit]
    public class UserProfileTraderShopRefreshCommand : IUserProfileServerCommand, IServerCommandExecutableFromClient {
    }

    public class UserProfileTraderShopRefreshCommandHandler : UserProfileServerCommandHandler<UserProfileTraderShopRefreshCommand> {
        private readonly GameDef gameDef;

        public UserProfileTraderShopRefreshCommandHandler(GameDef gameDef) {
            this.gameDef = gameDef;
        }

        public override async Task<ServerCommandResult> Execute(UserProfileServerCommandContext context, SdUserProfile gameData, UserProfileTraderShopRefreshCommand command) {
            var items = this.gameDef.Items.Items
                .Where(def => def.BuyCost.Count > 0)
                .SelectMany(def => Enumerable.Repeat(def.key, def.QuantityInTraderShop))
                .Select(itemKey => new GameSnapshotLoadoutItem {
                    ItemKey           = itemKey,
                    ItemGuid              = Guid.NewGuid().ToString(),
                    WeaponAttachments     = null,
                    IndexI                = 0,
                    IndexJ                = 0,
                    Rotated               = false,
                    Used                  = 0,
                    SafeGuid              = null,
                    AddToLoadoutAfterFail = false,
                })
                .ToList();

            gameData.TraderShop.Value = new TraderShopState {
                TradedItems = items,
            };

            return Ok;
        }
    }
}