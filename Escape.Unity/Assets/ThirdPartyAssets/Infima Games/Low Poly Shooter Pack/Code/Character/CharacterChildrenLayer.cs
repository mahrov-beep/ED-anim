namespace InfimaGames.LowPolyShooterPack {
    using Sirenix.OdinInspector;
    using UnityEngine;

    public class CharacterChildrenLayer : MonoBehaviour {
        [SerializeField, Required] private CharacterBehaviour characterBehaviour;

        private CharacterTypes currentCharacterType;

        private void LateUpdate() {
            var characterType = this.characterBehaviour.GetCharacterType();

            if (characterType == this.currentCharacterType) {
                return;
            }

            this.currentCharacterType = characterType;

            switch (characterType) {
                case CharacterTypes.LocalView:
                    LayersUtil.SetLayerRecursively(this.transform, this.characterBehaviour.GetConfig().localViewLayer);
                    break;

                case CharacterTypes.RemotePlayer:
                    LayersUtil.SetLayerRecursively(this.transform, this.characterBehaviour.GetConfig().remotePlayerLayer);
                    break;
            }
        }
    }
}