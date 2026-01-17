namespace Game.UI.Widgets.Game {
    using Domain.GameInventory;
    using ECS.Systems.Player;
    using Hud;
    using Multicast;
    using Quantum;
    using Quantum.Commands;
    using Services.Photon;
    using UniMob.UI;
    using Views.Game;
    using Views.Game.Hud;
    using UniMob;
    using System;

    [RequireFieldsInit]
    public class SelectableWeaponWidget : StatefulWidget {
        public GameInventorySlotItemModel Model;
        public GameWeaponModel            WeaponModel;
    }

    public class SelectableWeaponState : ViewState<SelectableWeaponWidget>, ISelectableWeaponState {
        [Inject] private PhotonService     photonService;
        [Inject] private LocalPlayerSystem localPlayerSystem;

        private WeaponAmmoOptionModel[] cachedMockOptions;

        public override WidgetViewReference View => this.SlotType switch {
            CharacterLoadoutSlots.MeleeWeapon => UiConstants.Views.HUD.SelectableMeleeWeapon,
            CharacterLoadoutSlots.SecondaryWeapon => UiConstants.Views.HUD.SelectableWeapon,
            CharacterLoadoutSlots.PrimaryWeapon => UiConstants.Views.HUD.SelectableWeapon,
            _ => UiConstants.Views.GameInventory.SlotEmptyItem,
        };

        private Frame Frame => this.photonService.PredictedFrame;

        private EntityRef ItemEntity => this.Widget.Model.ItemEntity;
        private Weapon    Weapon     => this.Frame!.Get<Weapon>(this.ItemEntity);
        private Unit      Unit       => this.localPlayerSystem.LocalRef.HasValue ? this.Frame.Get<Unit>(this.localPlayerSystem.LocalRef.Value) : default;

        private GameWeaponModel WeaponModel => this.Widget.WeaponModel;
        private WeaponItemAsset ItemAsset   => this.Weapon.GetConfig(this.Frame);

        public CharacterLoadoutSlots SlotType => this.Widget.Model.SlotType;

        public float ReloadingProgress => this.WeaponModel.ReloadingTimer / this.Weapon.CurrentStats.reloadingTime.AsFloat;

        public float WeaponChangingProgress => this.WeaponModel.IsWeaponChanging ? this.WeaponModel.WeaponChangingTimer / this.Unit.ConfigTimeToChangeWeapon(this.Frame).AsFloat : 0;

        public string ItemKey  => this.ItemAsset.ItemKey;
        public string ItemIcon => this.ItemAsset.IconLarge;

        public int BulletCount     => this.Widget.WeaponModel.Bullets;
    public int MaxBulletCount  => this.WeaponModel.MaxBullets;
    public int AmmoInInventory => this.WeaponModel.AmmoInInventory;

    public bool IsSelected => this.WeaponModel.IsSelected;

    [Atom] public IWeaponAmmoDropdownState AmmoDropdown => this.RenderChildT(_ => new WeaponAmmoDropdownWidget {
        IsSelectedWeapon = this.WeaponModel.IsSelected,
        OptionsBuilder   = this.BuildAmmoOptions,
        SelectedOption   = this.BuildSelectedAmmoOption(),
    }).As<WeaponAmmoDropdownState>();

    public void SelectWeapon() {
        QuantumRunner.DefaultGame.SendCommand(new SelectWeaponCommand() {
            SlotType = this.SlotType,
        });
    }

    private WeaponAmmoOptionModel[] BuildAmmoOptions() {
        // TODO: заменить на реальные типы боеприпасов, когда будут данные.       
        return this.BuildMockAmmoOptions();
    }

    private WeaponAmmoOptionModel BuildSelectedAmmoOption() {
        var bullets        = this.WeaponModel?.Bullets ?? 0;
        var ammoInInventory = this.WeaponModel?.AmmoInInventory ?? 0;
        var totalAmmo      = Math.Max(0, ammoInInventory);

        return new WeaponAmmoOptionModel {
            Icon = this.ItemAsset.IconLarge ?? string.Empty,
            MagazineCount = bullets,
            TotalAmmoCount = totalAmmo,
            IsSelected = true,
            OnSelect = null,
        };
    }

    private WeaponAmmoOptionModel[] BuildMockAmmoOptions() {
        return Array.Empty<WeaponAmmoOptionModel>();
        
        var bullets    = this.WeaponModel?.Bullets ?? 0;
        var ammoInInventory = this.WeaponModel?.AmmoInInventory ?? 0;
        var icon       = this.ItemAsset.IconLarge ?? string.Empty;

        if (this.cachedMockOptions == null || this.cachedMockOptions.Length == 0) {
            this.cachedMockOptions = new[] {
                new WeaponAmmoOptionModel(),
                new WeaponAmmoOptionModel(),
                new WeaponAmmoOptionModel(),
            };
        }

        ApplyOption(this.cachedMockOptions[0], icon, Math.Max(0, bullets - 5), ammoInInventory);
        ApplyOption(this.cachedMockOptions[1], icon, Math.Max(0, bullets - 10), ammoInInventory);
        ApplyOption(this.cachedMockOptions[2], icon, Math.Max(0, bullets - 15), ammoInInventory);

        return this.cachedMockOptions;
    }

    private static void ApplyOption(WeaponAmmoOptionModel target, string icon, int magazineCount, int ammoInInventory) {
        if (target == null) {
            return;
        }

        target.Icon            = icon;
        target.MagazineCount   = magazineCount;
        target.TotalAmmoCount  = Math.Max(0, ammoInInventory);
        target.IsSelected      = false;
        target.OnSelect        = null;
    }
    }
}
