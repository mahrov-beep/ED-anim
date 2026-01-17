namespace Game.UI.Views.World {
    using UniMob.UI;
    using Multicast;
    using Sirenix.OdinInspector;
    using UnityEngine;

    [RequireComponent(typeof(WorldView))]
    public class UnitPartyView : AutoView<IUnitPartyState> {
        [SerializeField, Required] private WorldView worldView = default;

        [SerializeField] private Vector3 worldOffset = Vector3.zero;

        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("nickname", () => this.State.NickName, "Player#000000000"),
            this.Variable("level", () => this.State.Level, 1),
        };

        protected override void Activate() {
            base.Activate();

            this.worldView.SetTarget(() => this.State.WorldPos + this.worldOffset);
        }

        protected override void Deactivate() {
            this.worldView.SetTarget(null);

            base.Deactivate();
        }
    }

    public interface IUnitPartyState : IViewState {
        Vector3 WorldPos { get; }

        string NickName { get; }
        int    Level    { get; }
    }
}