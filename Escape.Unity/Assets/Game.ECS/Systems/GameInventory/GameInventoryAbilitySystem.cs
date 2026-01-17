namespace Game.ECS.Systems.GameInventory {
    using Domain.GameInventory;
    using Multicast;
    using Player;
    using Quantum;
    using Services.Photon;
    using SystemBase = Scellecs.Morpeh.SystemBase;

    public class GameInventoryAbilitySystem : SystemBase {
        [Inject] private PhotonService      photonService;
        [Inject] private LocalPlayerSystem  localPlayerSystem;
        [Inject] private GameInventoryModel gameInventoryModel;

        public override void OnAwake() {
        }

        public override void OnUpdate(float deltaTime) {
            if (this.photonService.PredictedFrame is not { } f) {
                return;
            }

            if (localPlayerSystem.HasNotLocalEntityRef(out var localRef)) {
                return;
            }

            if (this.gameInventoryModel.TryGetSlotItem(CharacterLoadoutSlots.Skill, out var ability)) {
                this.UpdateModel(f, localRef, ability);
            }
            else {
                this.gameInventoryModel.AbilityModel.HasAbility = false;
            }
        }

        private void UpdateModel(Frame f, EntityRef e, GameInventorySlotItemModel model) {
            var ability = f.Get<Ability>(model.ItemEntity);

            var reloadingTimer = ability.CooldownTimer.NormalizedTimeLeft.AsFloat;

            var abilityModel = this.gameInventoryModel.AbilityModel;

            abilityModel.HasAbility     = true;
            abilityModel.ReloadingTimer = reloadingTimer;
        }
    }
}