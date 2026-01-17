namespace InfimaGames.LowPolyShooterPack {
    using Sirenix.OdinInspector;
    using UnityEngine;

    public class MovementBehaviourDelegated : MovementBehaviour {
        [SerializeField]
        [RequiredIn(PrefabKind.PrefabInstance)]
        [DisableIn(PrefabKind.PrefabAsset)]
        [InfoBox("Must be assigned by outer prefab")]
        private MovementBehaviour impl;

        protected override void Awake() {
            base.Awake();

            if (this.impl == null) {
                Debug.LogError($"Impl field is null on {this} ({nameof(MovementBehaviourDelegated)})", this);
            }

            if (this.impl == this) {
                Debug.LogError($"Impl must be set to non self component on {this} ({nameof(MovementBehaviourDelegated)})", this);
                this.impl = null;
            }
        }

        public override float GetLastJumpTime() => this.impl.GetLastJumpTime();

        public override Vector3 GetVelocity() => this.impl.GetVelocity();

        public override bool IsGrounded() => this.impl.IsGrounded();

        public override bool WasGrounded() => this.impl.WasGrounded();

        public override bool IsJumping() => this.impl.IsJumping();

        public override bool IsCrouching() => this.impl.IsCrouching();
    }
}