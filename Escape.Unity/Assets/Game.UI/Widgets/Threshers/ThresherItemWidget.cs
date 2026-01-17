namespace Game.UI.Widgets.Threshers {
    using System;
    using Domain.Threshers;
    using Multicast;
    using UniMob;
    using UniMob.UI;
    using Views.Threshers;

    [RequireFieldsInit]
    public class ThresherItemWidget : StatefulWidget {
        public string ThresherKey;

        public bool IsSelected;

        public Action OnSelect;
    }

    public class ThresherItemState : ViewState<ThresherItemWidget>, IThresherItemState {
        [Inject] private ThreshersModel threshersModel;

        [Atom] private ThresherModel ThresherModel => this.threshersModel.Get(this.Widget.ThresherKey);

        public override WidgetViewReference View => UiConstants.Views.Threshers.Item;

        public string ThresherKey => this.Widget.ThresherKey;

        public bool IsSelected => this.Widget.IsSelected;

        public int Level    => this.ThresherModel.Level;
        public int MaxLevel => this.ThresherModel.MaxLevel;

        public int Notify => this.ThresherModel.Notify;

        public void Select() {
            this.Widget.OnSelect?.Invoke();
        }
    }
}