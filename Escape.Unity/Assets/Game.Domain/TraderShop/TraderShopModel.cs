namespace Game.Domain.TraderShop {
    using System.Collections.Generic;
    using Currencies;
    using items;
    using Multicast;
    using Multicast.Numerics;
    using Shared;
    using Shared.Balance;
    using Shared.UserProfile.Data;
    using UniMob;

    public class TraderShopModel : Model {
        [Inject] private GameDef         gameDef;
        [Inject] private CurrenciesModel currenciesModel;
        [Inject] private ItemsModel      itemsModel;
        [Inject] private SdUserProfile   userProfile;

        private readonly MutableAtom<List<string>> toSellItemGuids;
        private readonly MutableAtom<List<string>> toBuyItemGuids;

        public TraderShopModel(Lifetime lifetime) : base(lifetime) {
            this.toSellItemGuids = Atom.Value(lifetime, new List<string>());
            this.toBuyItemGuids  = Atom.Value(lifetime, new List<string>());
        }

        public List<string> EnumerateToBuyGuids()  => this.toBuyItemGuids.Value;
        public List<string> EnumerateToSellGuids() => this.toSellItemGuids.Value;

        [Atom] public IntCost SellCost {
            get {
                var totalCost = IntCost.Empty;

                foreach (var itemGuid in this.EnumerateToSellGuids()) {
                    if (!this.userProfile.Storage.Lookup.TryGetValue(itemGuid, out var item)) {
                        continue;
                    }

                    totalCost += ItemBalance.CalculateSellCost(this.gameDef, item.Item.Value);
                }

                return totalCost;
            }
        }

        [Atom] public IntCost BuyCost {
            get {
                var totalCost   = IntCost.Empty;
                var traderState = this.userProfile.TraderShop.Value;

                foreach (var itemGuid in this.EnumerateToBuyGuids()) {
                    var item = traderState.TradedItems.Find(it => it.ItemGuid == itemGuid);
                    if (item == null) {
                        continue;
                    }

                    totalCost += ItemBalance.CalculateBuyCost(this.gameDef, item);
                }

                return totalCost;
            }
        }

        [Atom]
        public bool IsDealAvailable {
            get {
                if (this.EnumerateToSellGuids().Count == 0 && this.EnumerateToBuyGuids().Count == 0) {
                    return false;
                }

                var cost = this.BuyCost - this.SellCost;
                return this.currenciesModel.HasEnough(cost);
            }
        }

        public void Cleanup() {
            this.CleanupToSell();
            this.CleanupToBuy();
        }
        
        public void CleanupToSell() {
            this.toSellItemGuids.Value.Clear();
            this.toSellItemGuids.Invalidate();
        }
        
        public void CleanupToBuy() {
            this.toBuyItemGuids.Value.Clear();
            this.toBuyItemGuids.Invalidate();
        }

        public void AddToSellGuid(string itemGuid) {
            this.toSellItemGuids.Value.Add(itemGuid);
            this.toSellItemGuids.Invalidate();
        }

        public void RemoveToSellGuid(string itemGuid) {
            this.toSellItemGuids.Value.Remove(itemGuid);
            this.toSellItemGuids.Invalidate();
        }

        public void AddToBuyGuid(string itemGuid) {
            this.toBuyItemGuids.Value.Add(itemGuid);
            this.toBuyItemGuids.Invalidate();
        }

        public void RemoveToBuyGuid(string itemGuid) {
            this.toBuyItemGuids.Value.Remove(itemGuid);
            this.toBuyItemGuids.Invalidate();
        }
    }
}