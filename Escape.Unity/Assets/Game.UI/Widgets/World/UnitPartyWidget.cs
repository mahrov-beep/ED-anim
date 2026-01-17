namespace Game.UI.Widgets.World {
    using ECS.Scripts;
    using UniMob.UI;
    using UnityEngine;
    using Views.World;

    public class UnitPartyWidget : StatefulWidget {
        public UnitPartyUiDynamicData Data { get; }

        public UnitPartyWidget(UnitPartyUiDynamicData data) {
            this.Data = data;
        }
    }

    public class UnitPartyState : ViewState<UnitPartyWidget>, IUnitPartyState {

        public override WidgetViewReference View => UiConstants.Views.World.UnitPartyView;

        public Vector3 WorldPos => this.Widget.Data.WorldPos;
        public string  NickName => this.Widget.Data.NickName;
        public int     Level    => this.Widget.Data.Level;
    }
}