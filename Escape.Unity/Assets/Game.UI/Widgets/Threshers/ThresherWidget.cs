namespace Game.UI.Widgets.Threshers {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Controllers.Features.Thresher;
    using Domain.Threshers;
    using Multicast;
    using Shared;
    using Shared.Defs;
    using Shared.UserProfile.Data;
    using Shared.UserProfile.Data.Threshers;
    using Storage;
    using Storage.TraderShop;
    using UniMob;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using UnityEngine;
    using Views;
    using Views.Threshers;

    [RequireFieldsInit]
    public class ThresherWidget : StatefulWidget {
        public string ThresherKey;
    }

    public class ThresherState : ViewState<ThresherWidget>, IThresherState {
        [Inject] private GameDef        gameDef;
        [Inject] private SdUserProfile  userProfile;
        [Inject] private ThreshersModel threshersModel;

        private readonly StateHolder itemsState;

        [Atom] private ThresherModel ThresherModel => this.threshersModel.Get(this.Widget.ThresherKey);

        public ThresherState() {
            this.itemsState = this.CreateChild(this.BuildItems);
        }

        public override WidgetViewReference View => UiConstants.Views.Threshers.Details;

        public string ThresherKey => this.ThresherModel.Key;
        public int    Level       => this.ThresherModel.Level;
        public bool   CanLevelUp  => this.ThresherModel.CanLevelUp;

        public IState Items => this.itemsState.Value;

        private Widget BuildItems(BuildContext context) {
            return new ScrollGridFlow {
                Padding            = new RectPadding(10, 10, 10, 10),
                MaxCrossAxisExtent = 600,
                Children = {
                    this.ThresherModel.ThresherLevelDef.items.Select(it => new StorageItemSimpleWidget {
                        ItemKey = it.Key,
                        Details = new StorageItemPartialFillWidget {
                            CurrentParts  = this.ThresherModel.ItemsInStorageCount(it.Key),
                            RequiredParts = it.Value,
                            Notify        = false,
                            OnClick       = () => { },
                        },
                    }),
                },
            };
        }

        public bool CanMoveItemToThresh(DragAndDropPayloadItem payload) {
            if (payload is not DragAndDropPayloadItemFromTraderShopStorage fromTraderShopStorage) {
                return false;
            }

            return false; // legacy
        }

        public void OnMoveItemToThresh(DragAndDropPayloadItem payload) {
            if (payload is not DragAndDropPayloadItemFromTraderShopStorage fromTraderShopStorage) {
                return;
            }

            // legacy
        }

        public void LevelUp() {
            ThresherFeatureEvents.LevelUp.Raise(new ThresherFeatureEvents.LevelUpArgs {
                thresherKey = this.ThresherModel.Key,
            });
        }
    }
}