namespace Game.UI.Widgets {
    using ECS.Systems.Unit;
    using Multicast;
    using UniMob.UI;
    using UnityEngine;
    using Views;

    public class SlowDebuffWidget : StatefulWidget { }

    public class SlowDebuffState : ViewState<SlowDebuffWidget>, ISlowDebuffViewState {
        [Inject] private UISlowDebuffSystem uiSlowDebuffSystem;

        public override WidgetViewReference View =>
                        UiConstants.Views.World.DebuffSlowView;

        public int StackCount => uiSlowDebuffSystem.Debuff.hitCount;
    }
}