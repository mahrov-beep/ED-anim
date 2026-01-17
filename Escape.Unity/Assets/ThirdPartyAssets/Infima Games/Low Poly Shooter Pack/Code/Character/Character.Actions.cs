namespace InfimaGames.LowPolyShooterPack {
    using Multicast;
    using Sirenix.OdinInspector;
    using UnityEngine;

    public partial class Character {
        [ShowInInspector]
        [DisableInEditorMode]
        [EnumToggleButtons]
        [Title("Click to play full body animation")]
        public CharacterFullBodyStates DebugFullBodyState {
            get => Application.isPlaying ? (CharacterFullBodyStates)this.characterAnimator.GetInteger(AHashes.FUllBodyState) : CharacterFullBodyStates.Default;
            set => this.PlayFullBody(value);
        }

        public void PlayFullBody(CharacterFullBodyStates state) {
            this.characterAnimator.CrossFade(EnumNames<CharacterFullBodyStates>.GetName(state), 0.2f, this.layerFullBodyActions);
        }
    }
}