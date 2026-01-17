namespace Game.UI.Views.Game {
    using UniMob.UI;
    using Multicast;

    public class GameNearbyItemBoxView : AutoView<IGameNearbyItemBoxState> {
        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("can_equip_best", () => this.State.CanEquipBest, true),
            this.Variable("can_open", () => this.State.CanOpen, true),
            this.Variable("has_timer", () => this.State.HasTimer),
            this.Variable("progress", () => this.State.Progress),
            this.Variable("time_left", () => this.State.TimeLeft),
            this.Variable("is_backpack", () => this.State.IsBackpack),
        };
    }

    public interface IGameNearbyItemBoxState : IViewState {
        bool  CanEquipBest { get; }
        bool  CanOpen      { get; }
        bool  IsBackpack   { get; }
        bool  HasTimer     { get; }
        float Progress     { get; }
        int   TimeLeft     { get; }
    }
}