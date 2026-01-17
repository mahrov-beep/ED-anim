namespace Game.ECS.Systems.GameInventory {
    using Domain.GameInventory;
    using Multicast;
    using Player;
    using Quantum;
    using Services.Photon;
    using SystemBase = Scellecs.Morpeh.SystemBase;

    public class GameInventoryWeaponSystem : SystemBase {
        [Inject] private GameInventoryModel gameInventoryModel;
        [Inject] private PhotonService      photonService;
        [Inject] private LocalPlayerSystem  localPlayerSystem;

        public override void OnAwake() {
        }

        public override void OnUpdate(float deltaTime) {
            if (this.photonService.PredictedFrame is not { } f) {
                return;
            }

            if (localPlayerSystem.HasNotLocalEntityRef(out var localRef)) {
                return;
            }

            if (this.gameInventoryModel.TryGetSlotItem(CharacterLoadoutSlots.PrimaryWeapon, out var primary)) {
                this.UpdateModel(f, localRef, primary.ItemEntity, this.gameInventoryModel.PrimaryWeapon);
            }

            if (this.gameInventoryModel.TryGetSlotItem(CharacterLoadoutSlots.SecondaryWeapon, out var secondary)) {
                this.UpdateModel(f, localRef, secondary.ItemEntity, this.gameInventoryModel.SecondaryWeapon);
            }

            if (this.gameInventoryModel.TryGetSlotItem(CharacterLoadoutSlots.MeleeWeapon, out var melee)) {
                this.UpdateModel(f, localRef, melee.ItemEntity, this.gameInventoryModel.MeleeWeapon);
            }
        }

        private void UpdateModel(Frame f, EntityRef characterEntity, EntityRef weaponEntity, GameWeaponModel weaponModel) {
            var unit   = f.Get<Unit>(characterEntity);
            var weapon = f.Get<Weapon>(weaponEntity);

            var bulletCount     = weapon.BulletsCount;
            var ammoInInventory = weapon.CalcAvailableAmmoWithInventory(f, weaponEntity, characterEntity);
            var reloadingTimer  = weapon.ReloadingTimer.AsFloat;
            var changingTimer   = unit.WeaponChangingTimerLeft.AsFloat;

            weaponModel.Bullets          = bulletCount;
            weaponModel.MaxBullets       = weapon.MaxAmmo;
            weaponModel.AmmoInInventory  = ammoInInventory;
            weaponModel.IsSelected       = unit.ActiveWeaponRef == weaponEntity;
            weaponModel.ReloadingTimer   = reloadingTimer;
            weaponModel.IsWeaponChanging = changingTimer > 0;
        }
    }
}