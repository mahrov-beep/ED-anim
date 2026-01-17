namespace Game.ECS.Systems.Unit {
    using Multicast;
    using Quantum;
    using Sirenix.OdinInspector;
    using UniMob;
    using UnityEngine;

    public class MainMenuRemoteCharacterVisual : LifetimeMonoBehaviour {
        [SerializeField, Required]
        private CharacterVisual characterVisual;

        [SerializeField]
        private EscapeCharacterRemotePlayer escapeCharacterRemotePlayer;

        private MenuCharacterEquipmentApplier equipmentApplier;

        protected override void Start() {
            if (this.escapeCharacterRemotePlayer != null) {
                this.equipmentApplier = new MenuCharacterEquipmentApplier(this.escapeCharacterRemotePlayer);
            }
        }

        public void ApplyLoadout(GameSnapshotLoadout loadout) {
            var generic = this.GetComponent<MainMenuCharacterVisual>();
            
            if (generic != null) {
                generic.ApplyLoadout(loadout);
                
                return;
            }
            
            if (loadout == null || loadout.SlotItems == null) {
                return;
            }

            foreach (var slotType in CharacterLoadoutSlotsExtension.AllValidSlots) {
                var slotItem = slotType.ToInt() < loadout.SlotItems.Length ? loadout.SlotItems[slotType.ToInt()] : null;
                var slotName = EnumNames<CharacterLoadoutSlots>.GetName(slotType);
                
                this.characterVisual.SetVisual(slotName, slotItem?.ItemKey);
            }

            this.equipmentApplier?.ApplyEquipment(loadout);
        }
    }
}
