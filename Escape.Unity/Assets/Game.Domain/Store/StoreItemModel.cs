namespace Game.Domain.Models.Purchases {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Currencies;
    using Multicast;
    using Multicast.Collections;
    using Multicast.Numerics;
    using Multicast.Purchasing;
    using Multicast.RewardSystem;
    using Shared.Defs;
    using Shared.UserProfile.Data;
    using Shared.UserProfile.Data.Store;
    using UniMob;

    public class StoreItemsModel : KeyedSingleInstanceModel<StoreItemDef, SdStoreItem, StoreItemModel> {
        [Inject] private CurrenciesModel                    currenciesModel;
        [Inject] private CurrencyPurchasesModel             currencyPurchasesModel;
        [Inject] private LookupCollection<PurchaseDef>      iapPurchasesDef;
        [Inject] private LookupCollection<StoreCategoryDef> storeCategoriesDef;
        [Inject] private AppSharedFormulaContext            appSharedContext;

        public StoreItemsModel(Lifetime lifetime, LookupCollection<StoreItemDef> defs, SdUserProfile sdUserProfile)
            : base(lifetime, defs, sdUserProfile.StoreItems.Lookup) {
            this.AutoConfigureData = true;
        }

        public List<StoreItemModel> AllValues => base.Values;

        public List<StoreItemModel> FindByCurrency(string currencyKey, BigDouble requiredAmount) {
            var currencyModel = this.currenciesModel.Get(currencyKey);

            var remAmount = BigDouble.Max(0, requiredAmount - currencyModel.Amount);

            return Enumerable.Empty<string>()
                .Concat(FilterMatchedOrLargest(SearchCurrencyPurchases().ToList()))
                .Concat(FilterMatchedOrLargest(SearchStoreItems().ToList()))
                .Select(it => this.Get(it))
                .ToList();

            IEnumerable<(string itemKey, BigDouble amount)> SearchStoreItems() {
                foreach (var storeItem in this.AllValues) {
                    if (!storeItem.IsAvailable) {
                        continue;
                    }

                    if (storeItem.ExtraDrops != null) {
                        foreach (var drop in storeItem.ExtraDrops) {
                            if (drop is CurrencyRewardDef currencyDrop &&
                                currencyDrop.currency == currencyKey) {
                                var amount = currencyDrop.amount;

                                yield return (storeItem.Key, amount);
                            }
                        }
                    }
                }
            }

            IEnumerable<(string itemKey, BigDouble amount)> SearchCurrencyPurchases() {
                foreach (var currencyPurchaseModel in this.currencyPurchasesModel.Values) {
                    if (currencyPurchaseModel.RewardCurrency == currencyKey) {
                        yield return (currencyPurchaseModel.Key, currencyPurchaseModel.Reward);
                    }
                }
            }

            IEnumerable<string> FilterMatchedOrLargest(List<(string itemKey, BigDouble amount)> items) {
                if (items.Any(it => it.amount >= remAmount)) {
                    return items
                        .Where(it => it.amount >= remAmount)
                        .OrderBy(it => it.amount)
                        .Select(it => it.itemKey);
                }

                if (items.Any()) {
                    return new string[] {
                        items.OrderBy(it => it.amount).Last().itemKey,
                    };
                }

                return Array.Empty<string>();
            }
        }
    }

    public class StoreItemModel : Model<StoreItemDef, SdStoreItem> {
        private readonly CurrencyPurchasesModel currencyPurchasesModel;
        public string        Category   => this.Def.category;
        public string        UiPrefab   => this.Def.uiPrefab;
        public StoreItemType ItemType   => this.Def.itemType;

        public string IapPurchaseKey      => this.Def.iapPurchaseKey;
        public string CurrencyPurchaseKey => this.Def.currencyPurchaseKey;

        public List<RewardDef> ExtraDrops => this.Def.extraDrops;

        public StoreItemModel(
            Lifetime lifetime,
            StoreItemDef def,
            SdStoreItem data,
            CurrencyPurchasesModel currencyPurchasesModel) : base(lifetime, def, data) {
            this.currencyPurchasesModel = currencyPurchasesModel;
        }

        [Atom] public bool IsAvailable =>
            this.ItemType switch {
                StoreItemType.CurrencyPurchase => this.currencyPurchasesModel.TryGet(this.CurrencyPurchaseKey, out var item) && item.IsAvailable,
                StoreItemType.IapPurchase => true,
                StoreItemType.KeyIapPurchase => true,
                StoreItemType.IapWithBonusPurchase => true,
                StoreItemType.PowerUp => true,
                StoreItemType.BundleIapPurchase => true,
                _ => false,
            };

        public bool HasBeenSeen => this.Data.HasBeenSeen.Value;
    }
}