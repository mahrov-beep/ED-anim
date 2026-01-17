namespace InfimaGames.LowPolyShooterPack {
    using Sirenix.OdinInspector;
    using Unity.Cinemachine;
    using UnityEngine;

    public class CharacterBehaviourDelegated : CharacterBehaviour {
        [SerializeField]
        [RequiredIn(PrefabKind.PrefabInstance)]
        [DisableIn(PrefabKind.PrefabAsset)]
        [InfoBox("Must be assigned by outer prefab")]
        private CharacterBehaviour impl;

        protected override void Awake() {
            base.Awake();

            if (this.impl == null) {
                Debug.LogError($"Impl field is null on {this} ({nameof(CharacterBehaviourDelegated)})", this);
            }
            
            if (this.impl == this) {
                Debug.LogError($"Impl must be set to non self component on {this} ({nameof(CharacterBehaviourDelegated)})", this);
                this.impl = null;
            }
        }

        public override CharacterTypes  GetCharacterType() => this.impl.GetCharacterType();
        public override CharacterConfig GetConfig()        => this.impl.GetConfig();

        public override Animator GetCharacterAnimator() => this.impl.GetCharacterAnimator();

        public override CharacterAudioPlayer GetAudioPlayer() => this.impl.GetAudioPlayer();

        public override int GetShotsFired() => this.impl.GetShotsFired();

        public override bool IsLowered() => this.impl.IsLowered();

        public override CinemachineCamera GetCameraWorld() => this.impl.GetCameraWorld();

        public override Camera GetCameraDepth() => this.impl.GetCameraDepth();

        public override InventoryBehaviour GetInventory() => this.impl.GetInventory();

        public override bool IsRunning() => this.impl.IsRunning();

        public override bool IsCrouching() => this.impl.IsCrouching();

        public override bool IsAiming() => this.impl.IsAiming();

        public override bool IsHealing() => this.impl.IsHealing();

        public override float GetHealingProgress01() => this.impl.GetHealingProgress01();

        public override Vector2 GetInputMovement() => this.impl.GetInputMovement();

        public override Vector2 GetInputLook() => this.impl.GetInputLook();

        public override void EjectCasing() => this.impl.EjectCasing();

        public override void FillAmmunition(int amount) => this.impl.FillAmmunition(amount);

        public override void Grenade() => this.impl.Grenade();

        public override void SetActiveMagazine(int active) => this.impl.SetActiveMagazine(active);

        public override void SetSlideBack(int back) => this.impl.SetSlideBack(back);

        public override void SetActiveKnife(int active) => this.impl.SetActiveKnife(active);
    }
}