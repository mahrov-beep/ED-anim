namespace Game.UI.Widgets.Game.Hud {
    using System;
    using Multicast;
    using UniMob.UI;
    using Views.Game.Hud;

    public class WeaponAmmoOptionWidget : StatefulWidget {
        public string Icon;
        public int    MagazineCount;
        public int    TotalAmmoCount;
        public bool   IsSelected;
        public Action OnSelect;
    }

    public class WeaponAmmoOptionState : ViewState<WeaponAmmoOptionWidget>, IWeaponAmmoOptionState {
        public override WidgetViewReference View => UiConstants.Views.HUD.WeaponAmmoOption;

        public string Icon => this.Widget.Icon ?? string.Empty;

        public int MagazineCount    => this.Widget.MagazineCount;
        public int TotalAmmoCount => this.Widget.TotalAmmoCount;
        public bool IsSelected      => this.Widget.IsSelected;

        public void Select() {
            this.Widget.OnSelect?.Invoke();
        }
    }
}
