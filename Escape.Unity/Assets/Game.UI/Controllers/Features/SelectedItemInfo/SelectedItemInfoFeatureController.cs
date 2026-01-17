namespace Game.UI.Controllers.Features.SelectedItemInfo {
    using System;
    using Cysharp.Threading.Tasks;
    using Domain.GameInventory;
    using Domain.items;
    using ECS.Systems.Player;
    using JetBrains.Annotations;
    using Multicast;
    using Multicast.Routes;
    using Quantum;
    using Services.Photon;
    using UniMob;
    using UniMob.UI.Widgets;
    using UnityEngine;
    using Widgets.ItemInfo;

    [Serializable, RequireFieldsInit]
    public struct SelectedItemInfoFeatureControllerArgs : IDisposableControllerArgs {
    }

    public class SelectedItemInfoFeatureController : DisposableController<SelectedItemInfoFeatureControllerArgs> {
        [Inject] private ItemsModel         itemsModel;
        [Inject] private PhotonService      photonService;
        [Inject] private GameInventoryModel gameInventoryModel;
        [Inject] private LocalPlayerSystem  localPlayerSystem;

        private IUniTaskAsyncDisposable itemInfoScreen;

        private bool closeRequired;

        [Atom] private (EntityRef itemRef, string itemGuid, ItemAsset itemAsset) CurrentEntry { get; set; }

        [Atom] private WidgetPosition.Position CurrentWidgetPosition      { get; set; }
        [Atom] private bool                    CurrentIsTakeButtonVisible { get; set; }

        protected override async UniTask Activate(Context context) {
            SelectedItemInfoFeatureEvents.Close.Listen(this.Lifetime, () => this.RequestFlow(this.Close));
            SelectedItemInfoFeatureEvents.Select.Listen(this.Lifetime, args => this.RequestFlow(this.SelectItem, args));

            // синхронизируем значение переменной ИЗ контроллера В модель
            this.Lifetime.Register(() => this.gameInventoryModel.SetSelectedItem(EntityRef.None));
            Atom.Reaction(this.Lifetime,
                () => this.CurrentEntry.itemRef,
                itemRef => this.gameInventoryModel.SetSelectedItem(itemRef));
        }

        protected override async UniTask OnDisposeAsync(Context context) {
            await this.Close(context);
        }

        protected override void OnUpdate() {
            this.closeRequired = this.IsCloseRequired();
        }

        protected override async UniTask OnFlow(Context context) {
            if (this.closeRequired) {
                this.closeRequired = false;
                await this.Close(context);
            }
        }

        private async UniTask SelectItem(Context context, SelectedItemInfoFeatureEvents.SelectArgs args) {
            ItemAsset itemAsset;
            if (this.photonService.PredictedFrame is { } f &&
                f.Exists(args.ItemEntity) &&
                f.TryGet(args.ItemEntity, out Item item)) {
                itemAsset = f.FindAsset(item.Asset);
            }
            else {
                itemAsset = QuantumUnityDB.Global.GetRequiredAsset<ItemAsset>(
                    ItemAssetCreationData.GetItemAssetPath(args.ItemSnapshot.ItemKey)
                );
            }

            if (itemAsset == null) {
                Debug.LogError($"[{this}]: Cannot show item info without ItemAsset or ItemEntity");
                return;
            }

            var entry = (args.ItemEntity, args.ItemSnapshot?.ItemGuid, itemAsset);

            if (this.CurrentEntry == entry) {
                await this.Close(context);
                return;
            }

            this.CurrentEntry               = entry;
            this.CurrentWidgetPosition      = args.Position;
            this.CurrentIsTakeButtonVisible = args.IsTakeButtonVisible;

            if (this.itemInfoScreen == null) {
                this.itemInfoScreen = await this.CreateAndShowScreen(context);
            }
        }


        private async UniTask Close(Context context) {
            if (this.itemInfoScreen != null) {
                await this.itemInfoScreen.DisposeAsync();
                this.itemInfoScreen = null;
            }

            this.CurrentEntry = default;
        }

        private async UniTask<IUniTaskAsyncDisposable> CreateAndShowScreen(Context context) {
            return await context.RunDisposable(new UiScreenControllerArgs {
                Route = UiConstants.Routes.ItemInfo,
                Page = () => new ItemInfoWidget {
                    OnClose             = () => SelectedItemInfoFeatureEvents.Close.Raise(),
                    Position            = this.CurrentWidgetPosition,
                    IsTakeButtonVisible = this.CurrentIsTakeButtonVisible,
                    ItemAsset           = this.CurrentEntry.itemAsset,
                    ItemEntityOptional  = this.CurrentEntry.itemRef,
                },
                TransitionDuration        = 0.15f,
                ReverseTransitionDuration = 0.05f,
                
                OnBackPerformed = () => SelectedItemInfoFeatureEvents.Close.Raise(),
            });
        }

        private bool IsCloseRequired() {
            if (this.itemInfoScreen == null) {
                return false;
            }

            if (this.CurrentEntry.itemRef != EntityRef.None) {
                if (this.photonService.PredictedFrame is not { } f) {
                    return true;
                }

                if (!f.Exists(this.CurrentEntry.itemRef)) {
                    return true;
                }

                if (!this.IsValidForView(f, this.CurrentEntry.itemRef)) {
                    return false;
                }
            }

            return false;
        }

        private bool IsValidForView(Frame f, EntityRef itemRef) {
            if (f.GameMode.rule is GameRules.MainMenuStorage) {
                return true;
            }

            if (!f.Exists(itemRef) || !f.TryGet(itemRef, out Item item)) {
                return false;
            }

            return item.Owner == EntityRef.None || item.Owner == this.localPlayerSystem.LocalRef || this.IsValidForView(f, item.Owner);
        }
    }
}