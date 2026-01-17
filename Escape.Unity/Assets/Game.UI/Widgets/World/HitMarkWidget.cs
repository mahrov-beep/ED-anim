namespace Game.UI.Widgets.World {
    using ECS.Scripts;
    using UniMob.UI;
    using UnityEngine;
    using Views.World;

    public class HitMarkWidget : StatefulWidget {
        public HitMarkUiDynamicData Data { get; }

        public HitMarkWidget(HitMarkUiDynamicData data) {
            this.Data = data;
        }
    }

    public class HitMarkState : ViewState<HitMarkWidget>, IHitMarkState {

        public override WidgetViewReference View => UiConstants.Views.World.HitMarkView;

        public Vector3 WorldPos  => this.Widget.Data.WorldPos;
        public float   Damage    => this.Widget.Data.Damage;
        public float   Alpha     => this.Widget.Data.Alpha;
    }
}