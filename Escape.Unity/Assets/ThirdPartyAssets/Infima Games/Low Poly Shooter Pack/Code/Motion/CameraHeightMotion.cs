namespace InfimaGames.LowPolyShooterPack {
    using Sirenix.OdinInspector;
    using UnityEngine;

    public class CameraHeightMotion : Motion {
        [SerializeField, Required]
        private CharacterBehaviour characterBehaviour;

        [SerializeField]
        private CharacterTypes applyOnlyOn;

        private readonly SpringSettings defaultSprintSettings = SpringSettings.Default();
        private readonly Spring         sprintPosition        = new Spring();

        private SpringSettings springSettings;

        public override void Tick() {
            var characterConfig = this.characterBehaviour.GetConfig();
            var characterType   = this.characterBehaviour.GetCharacterType();

            var cameraHeights = characterType switch {
                CharacterTypes.LocalView => characterConfig.LocalViewCameraHeights,
                CharacterTypes.RemotePlayer => characterConfig.RemotePlayerCameraHeight,
                _ => null,
            };

            if (characterType != this.applyOnlyOn || cameraHeights == null) {
                this.springSettings = this.defaultSprintSettings;
                this.sprintPosition.UpdateEndValue(Vector3.zero);
                return;
            }

            Vector3 height;

            if (this.characterBehaviour.IsCrouching()) {
                var alpha = this.characterBehaviour.GetCharacterAnimator().GetFloat(AHashes.Movement);
                height              = Vector3.Lerp(cameraHeights.CrouchIdle.Offset, cameraHeights.CrouchMove.Offset, alpha);
                this.springSettings = alpha <= 0.5f ? cameraHeights.CrouchIdle.Spring : cameraHeights.CrouchMove.Spring;
            }
            else {
                height              = cameraHeights.Normal.Offset;
                this.springSettings = cameraHeights.Normal.Spring;
            }

            switch (this.characterBehaviour.GetFullBodyState()) {
                case CharacterFullBodyStates.Died:
                    height              = cameraHeights.Died.Offset;
                    this.springSettings = cameraHeights.Died.Spring;
                    break;

                case CharacterFullBodyStates.Knocked:
                    height              = cameraHeights.Knocked.Offset;
                    this.springSettings = cameraHeights.Knocked.Spring;
                    break;

                case CharacterFullBodyStates.Roll:
                    height              = cameraHeights.Roll.Offset;
                    this.springSettings = cameraHeights.Roll.Spring;
                    break;
            }

            this.sprintPosition.UpdateEndValue(height);
        }

        public override Vector3 GetLocation() {
            return this.sprintPosition.Evaluate(this.springSettings);
        }

        public override Vector3 GetEulerAngles() {
            return default;
        }
    }
}