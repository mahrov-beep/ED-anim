namespace Game.UI.Widgets.World {
    using ECS.Scripts;
    using UniMob.UI;
    using UnityEngine;
    using Views;
    using Views.World;

    public class UnitHealthBarWidget : StatefulWidget {
        public UnitHealthBarUiDynamicData Data { get; }

        public UnitHealthBarWidget(UnitHealthBarUiDynamicData data) {
            this.Data = data;
        }
    }

    public class UnitHealthBarState : ViewState<UnitHealthBarWidget>, IUnitHealthBarState {
        private readonly StateHolder<SlowDebuffState> slowDebuffState;

        public UnitHealthBarState() {
            slowDebuffState = CreateChild<SlowDebuffWidget, SlowDebuffState>(_ => new SlowDebuffWidget());
        }

        public ISlowDebuffViewState SlowDebuffState => slowDebuffState.Value;

        public override WidgetViewReference View => UiConstants.Views.World.HealthBarView;

        public Vector3 WorldPos  => this.Widget.Data.WorldPos;
        public float   Health    => this.Widget.Data.Health;
        public float   MaxHealth => this.Widget.Data.MaxHealth;
        public float   KnockHealth => this.Widget.Data.KnockHealth;
        public bool    IsKnocked   => this.Widget.Data.IsKnocked;
        public bool    IsBeingRevived => this.Widget.Data.IsBeingRevived;
        public bool    IsDead    => this.Widget.Data.IsDead;
        public string  NickName  => this.Widget.Data.NickName;
        public float   Alpha     => this.Widget.Data.Alpha;
    }
}