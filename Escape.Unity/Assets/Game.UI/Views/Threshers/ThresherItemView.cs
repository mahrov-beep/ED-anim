namespace Game.UI.Views.Threshers {
    using UniMob.UI;
    using Multicast;
    using Shared;

    public class ThresherItemView : AutoView<IThresherItemState> {
        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("thresher_key", () => this.State.ThresherKey, SharedConstants.Game.Threshers.TRADER),
            this.Variable("is_selected", () => this.State.IsSelected, false),
            this.Variable("level", () => this.State.Level, 5),
            this.Variable("max_level", () => this.State.MaxLevel, 10),
            this.Variable("notify", () => this.State.Notify, 99),
        };

        protected override AutoViewEventBinding[] Events => new[] {
            this.Event("select", () => this.State.Select()),
        };
    }

    public interface IThresherItemState : IViewState {
        string ThresherKey { get; }

        bool IsSelected { get; }

        int Level    { get; }
        int MaxLevel { get; }

        int Notify { get; }

        void Select();
    }
}