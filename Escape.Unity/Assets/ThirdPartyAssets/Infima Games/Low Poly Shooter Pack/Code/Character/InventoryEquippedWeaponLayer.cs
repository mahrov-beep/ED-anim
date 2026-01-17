namespace InfimaGames.LowPolyShooterPack {
    using Sirenix.OdinInspector;
    using UnityEngine;

    public class InventoryEquippedWeaponLayer : MonoBehaviour {
        [SerializeField, Required] private CharacterBehaviour characterBehaviour;

        private CharacterTypes  currentCharacterType;
        private WeaponBehaviour currentEquippedWeapon;

        private void LateUpdate() {
            var characterType  = this.characterBehaviour.GetCharacterType();
            var equippedWeapon = this.characterBehaviour.GetInventory().GetEquipped();

            if (characterType == this.currentCharacterType &&
                equippedWeapon == this.currentEquippedWeapon) {
                return;
            }

            this.currentCharacterType  = characterType;
            this.currentEquippedWeapon = equippedWeapon;

            if (equippedWeapon == null) {
                return;
            }

            switch (characterType) {
                case CharacterTypes.LocalView:
                    LayersUtil.SetLayerRecursively(equippedWeapon.transform, this.characterBehaviour.GetConfig().localViewLayer);
                    break;

                case CharacterTypes.RemotePlayer:
                    LayersUtil.SetLayerRecursively(equippedWeapon.transform, this.characterBehaviour.GetConfig().remotePlayerLayer);
                    break;
            }
        }
    }
}