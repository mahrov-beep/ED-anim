namespace Game.UI.Views.Game {
using Multicast;
using UI.Views.Game.Hud;
using TMPro;
using UniMob.UI;
using UnityEngine;
using UnityEngine.UI;

public sealed class SelectableWeaponView : AutoView<ISelectableWeaponState> {
    protected override AutoViewVariableBinding[] Variables => new[] {
        this.Variable("reloading_progress", () => this.State.ReloadingProgress),
        this.Variable("weapon_changing_progress", () => this.State.WeaponChangingProgress),
        this.Variable("item_key", () => this.State.ItemKey),
        this.Variable("item_icon", () => this.State.ItemIcon),
        this.Variable("bullet_count", () => this.State.BulletCount),
        this.Variable("max_bullet_count", () => this.State.MaxBulletCount),
        this.Variable("is_selected", () => this.State.IsSelected),
        this.Variable("ammo_in_inventory", () => this.State.AmmoInInventory, 99),
    };

    [SerializeField] private TextMeshProUGUI bulletsText;
    [SerializeField] private Image           bulletsFillImage;
    [SerializeField] private WeaponAmmoDropdownView ammoDropdownView;

    private float lowMagPercent = 0.3f;

    protected override void Render() {
        base.Render();

        var isMagLow = (float)this.State.BulletCount / this.State.MaxBulletCount <= this.lowMagPercent;
        this.bulletsFillImage.color = isMagLow ? Color.red : Color.white;
        this.bulletsText.color      = isMagLow ? Color.red : Color.white;

        if (this.ammoDropdownView != null) {
            var active = this.State.IsSelected;
            if (this.ammoDropdownView.gameObject.activeSelf != active) {
                this.ammoDropdownView.gameObject.SetActive(active);
            }

            this.ammoDropdownView.Render(this.State.AmmoDropdown);
        }
    }

    protected override AutoViewEventBinding[] Events => new[] {
        this.Event("select_weapon", () => this.State.SelectWeapon()),
    };
}

public interface ISelectableWeaponState : IViewState {
    float ReloadingProgress      { get; }
    float WeaponChangingProgress { get; }

    string ItemKey { get; }
    string ItemIcon { get; }

    int BulletCount     { get; }
    int MaxBulletCount  { get; }
    int AmmoInInventory { get; }

    bool IsSelected { get; }

    IWeaponAmmoDropdownState AmmoDropdown { get; }
    
    void SelectWeapon();
}
}
