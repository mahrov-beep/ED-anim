namespace Game.Shared.UserProfile.Commands.TraderShop {
    using System;
    using System.Buffers;
    using System.Collections.Generic;
    using System.Linq;
    using MessagePack;
    using Multicast;
    using System.Threading.Tasks;
    using Balance;
    using Currencies;
    using Data;
    using DTO;
    using Multicast.Numerics;
    using Quantum;
    using Helpers;

    [MessagePackObject, RequireFieldsInit]
    public class UserProfileTraderShopMakeDealCommand : IUserProfileServerCommand, IServerCommandExecutableFromClient {
        [Key(0)] public List<string> ItemGuidsToSell;
        [Key(1)] public List<string> ItemGuidsToBuy;
    }

    public class UserProfileTraderShopMakeDealCommandHandler : UserProfileServerCommandHandler<UserProfileTraderShopMakeDealCommand> {
        private readonly GameDef gameDef;

        public UserProfileTraderShopMakeDealCommandHandler(GameDef gameDef) {
            this.gameDef = gameDef;
        }

        public override async Task<ServerCommandResult> Execute(UserProfileServerCommandContext context, SdUserProfile gameData, UserProfileTraderShopMakeDealCommand command) {
            var itemsToSell = command.ItemGuidsToSell
                .Select(it => gameData.Storage.Lookup.Get(it))
                .ToList();

            var itemsToBuy = new List<GameSnapshotLoadoutItem>();
            foreach (var guid in command.ItemGuidsToBuy) {
                var item = gameData.TraderShop.Value.TradedItems.Find(a => a.ItemGuid == guid);
                if (item == null) {
                    return BadRequest($"Item with guid {guid} not found in trader shop");
                }
                itemsToBuy.Add(item);
            }

            foreach (var storageItem in itemsToSell) {
                if (storageItem.Item.Value == null) {
                    return BadRequest($"Storage item {storageItem.ItemGuid} has null Item.Value");
                }
            }
            
            var totalSellCost = itemsToSell
                .Aggregate(IntCost.Empty, (sum, it) => sum + ItemBalance.CalculateSellCost(this.gameDef, it.Item.Value));

            var totalBuyCost = itemsToBuy
                .Aggregate(IntCost.Empty, (sum, it) => sum + ItemBalance.CalculateBuyCost(this.gameDef, it));

            foreach (var itemToSell in itemsToSell) {
                gameData.Storage.Lookup.Remove(itemToSell);
            }
            
            var newRanges = ArrayPool<CellsRange>.Shared.Rent(itemsToBuy.Count);
            
            for (var i = 0; i < itemsToBuy.Count; i++) {
                if (!this.gameDef.Items.TryGet(itemsToBuy[i].ItemKey, out var itemDef)) {
                    ArrayPool<CellsRange>.Shared.Return(newRanges, true);
                    return BadRequest($"Item definition not found for key: {itemsToBuy[i].ItemKey}");
                }
                
                var range = CellsRange.FromIJWH(0, 0, itemDef.CellsWidth, itemDef.CellsHeight, true);

                newRanges[i] = range;
            }
            
            if (!StoragePlacementHelper.TryFindPlaceInStorage(this.gameDef, gameData, newRanges, out var foundRanges)) {
                ArrayPool<CellsRange>.Shared.Return(newRanges, true);
                return BadRequest($"Not Enough place in storage {gameData.Storage.Lookup.Count}");
            }

            for (var i = 0; i < itemsToBuy.Count; i++) {
                var itemToBuy = itemsToBuy[i];
                
                StoragePlacementHelper.PlaceItemInStorage(gameData, itemToBuy.ItemGuid, itemToBuy, foundRanges[i]);
            }
            
            ArrayPool<CellsRange>.Shared.Return(newRanges, true);

            gameData.TraderShop.Value = new TraderShopState {
                TradedItems = gameData.TraderShop.Value.TradedItems
                    .Except(itemsToBuy)
                    //.Concat(itemsToSell.Select(it => it.Item.Value))
                    .ToList(),
            };

            await context.Execute(new UserProfileAddCurrencyCommand {
                CurrencyToAdd = totalSellCost.ToDictionary(it => it.Key, it => it.Value),
            });

            await context.Execute(new UserProfileCurrencyTakeAmountCommand {
                CurrencyToTake = totalBuyCost.ToDictionary(it => it.Key, it => it.Value),
            });

            return Ok;
        }
    }
}