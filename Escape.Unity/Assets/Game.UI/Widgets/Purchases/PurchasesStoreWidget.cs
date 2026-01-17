namespace Game.UI.Widgets.Purchases {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common;
    using Domain.Commands.Purchases;
    using Domain.Models.Purchases;
    using Items;
    using Multicast;
    using Multicast.Collections;
    using Shared;
    using Shared.Defs;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using World = Scellecs.Morpeh.World;

    public class PurchasesStoreWidget : StatefulWidget {
        public static Widget BuildPurchaseItem(StoreItemModel storeItem, string categoryKey) => storeItem.ItemType switch {
            StoreItemType.CurrencyPurchase => new PurchasesCurrencyItemWidget(storeItem.CurrencyPurchaseKey, categoryKey, storeItem.Key) {
                ViewReference = WidgetViewReference.Addressable(storeItem.UiPrefab),
            },

            StoreItemType.IapPurchase => new PurchaseIapItemWidget(storeItem.IapPurchaseKey, categoryKey, storeItem.Key, false) {
                ViewReference = WidgetViewReference.Addressable(storeItem.UiPrefab),
            },

            StoreItemType.KeyIapPurchase => new PurchaseKeyIapItemWidget(storeItem.IapPurchaseKey, categoryKey, storeItem.Key) {
                ViewReference = WidgetViewReference.Addressable(storeItem.UiPrefab),
            },
            _ => new Empty(),
        };
    }

    public class PurchasesStoreState : HocState<PurchasesStoreWidget> {
        [Inject] private readonly GameDef                            gameDef;
        [Inject] private readonly StoreItemsModel                    storeItemsModel;
        [Inject] private readonly LookupCollection<StoreCategoryDef> storeCategories;
        [Inject] private readonly World                              world;

        public override Widget Build(BuildContext context) {
            return new HorizontalScrollGridFlow {
                MainAxisAlignment = MainAxisAlignment.Center,
                CrossAxisAlignment = CrossAxisAlignment.Center,
                MaxCrossAxisCount = 1,
                Children = {
                    BuildPadding(UniMobDeviceState.Of(context).SafeArea.Left),
                    this.BuildItemsCategories(),
                    BuildPadding(UniMobDeviceState.Of(context).SafeArea.Right),
                },
            };
        }

        public override void InitState() {
            base.InitState();

            foreach (var category in this.storeCategories.Items) {
                var storeItems = this.storeItemsModel.AllValues.Where(x => x.Category == category.key);

                foreach (var item in storeItems) {
                    var storeItem = this.storeItemsModel.Get(item.Key);

                    if (storeItem.HasBeenSeen) {
                        continue;
                    }

                    App.Execute(new SetStoreItemHasBeenSeenCommand(storeItem.Key));
                }
            }
        }

        private IEnumerable<Widget> BuildItemsCategories() => new List<Widget>() {
            this.storeCategories.Items.Select(it => new GridFlow {
                Padding = new RectPadding(30, 30, 0, 0),
                MaxCrossAxisExtent = 240 * 2 + 10 + 60,
                Children = {
                    this.SelectCurrencyCategory(it),
                },
            }),
        };

        private IEnumerable<Widget> SelectCurrencyCategory(StoreCategoryDef categoryDef) {
            return this.BuildCurrencyCategory(this.gameDef.StoreCategories.Get(categoryDef.key),
                this.storeItemsModel.AllValues.Where(x => x.Category == categoryDef.key).Select(x => x.Key).ToList());
        }

        private IEnumerable<Widget> BuildCurrencyCategory(StoreCategoryDef category, List<string> storeItems) {
            var categoryItems = storeItems.Select(it => this.storeItemsModel.Get(it)).ToList();
            if (categoryItems.Count > 0) {
                return new List<Widget>() {
                    new PurchasesStoreCategoryWidget(Key.Of(category.key)) {
                        StoreCategoryKey = category.key,
                    },

                    categoryItems.Select(it => PurchasesStoreWidget.BuildPurchaseItem(it, it.Category)),
                };
            }

            return new[] { new Empty() };
        }

        private static Container BuildPadding(float width) {
            return new Container {
                Size = WidgetSize.Fixed(width, 1000),
            };
        }
    }
}