namespace Game.UI.Views.World {
    using UniMob.UI;
    using Multicast;
    using Sirenix.OdinInspector;
    using UnityEngine;

    [RequireComponent(typeof(WorldView))]
    public class ItemBoxTimerView : AutoView<IItemBoxTimerState> {
        [SerializeField, Required] private WorldView worldView = default;

        [SerializeField] private Vector3 worldOffset = Vector3.zero;

        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("progress", () => this.State.Progress, 10),
            this.Variable("timer", () => this.State.Timer, 10),
        };

        protected override void Activate() {
            base.Activate();

            this.worldView.SetTarget(() => this.State.WorldPos + this.worldOffset, hideWhenOffscreen: true);
        }

        protected override void Deactivate() {
            this.worldView.SetTarget(null);

            base.Deactivate();
        }
    }

    public interface IItemBoxTimerState : IViewState {
        Vector3 WorldPos { get; }

        float Progress { get; }
        int   Timer    { get; }
    }
}