namespace Game.UI.Views.Game.Hud {
    using Multicast;
    using TMPro;
    using UniMob.UI;
    using UnityEngine;

    /// <summary>
    /// Отдельная строка в списке вариантов боеприпасов (например, обычные/трейсеры/разрывающие).
    /// </summary>
    public class WeaponAmmoOptionView : AutoView<IWeaponAmmoOptionState> {
        [SerializeField] private TextMeshProUGUI countText;
        [SerializeField] private GameObject     selectionHighlight;

        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("ammo_icon", () => this.State.Icon),
            this.Variable("mag_count", () => this.State.MagazineCount),
            this.Variable("mag_max", () => this.State.TotalAmmoCount),
            this.Variable("is_selected", () => this.State.IsSelected),
        };

        protected override AutoViewEventBinding[] Events => new[] {
            this.Event("select", () => this.State.Select()),
        };

        protected override void Render() {
            base.Render();

            if (countText != null) {
                countText.text = $"{this.State.MagazineCount}/{this.State.TotalAmmoCount}";
            }

            if (selectionHighlight != null) {
                selectionHighlight.SetActive(this.State.IsSelected);
            }
        }
    }

    public interface IWeaponAmmoOptionState : IViewState {
        string Icon             { get; }
        int    MagazineCount    { get; }
        int    TotalAmmoCount   { get; }
        bool   IsSelected       { get; }

        void Select();
    }
}
