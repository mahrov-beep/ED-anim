namespace Game.UI.Views.World {
    using UniMob.UI;
    using Multicast;
    using Sirenix.OdinInspector;
    using UnityEngine;

    [RequireComponent(typeof(WorldView))]
    public class HitMarkView : AutoView<IHitMarkState> {
        [SerializeField, Required] private WorldView worldView = default;

        [SerializeField] private Vector3 worldOffset = Vector3.zero;

        [SerializeField, Required] private CanvasGroup alphaCanvasGroup;

        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("damage", () => this.State.Damage, 10),
            this.Variable("alpha", () => this.State.Alpha, 1f),
        };

        protected override void Activate() {
            base.Activate();

            this.worldView.SetTarget(() => this.State.WorldPos + this.worldOffset);
        }

        protected override void Deactivate() {
            this.worldView.SetTarget(null);

            base.Deactivate();
        }

        protected override void Render() {
            base.Render();
            
            this.alphaCanvasGroup.alpha = State.Alpha;
        }
    }

    public interface IHitMarkState : IViewState {
        Vector3 WorldPos { get; }
        float Damage { get; }
        float Alpha     { get; }
    }
}