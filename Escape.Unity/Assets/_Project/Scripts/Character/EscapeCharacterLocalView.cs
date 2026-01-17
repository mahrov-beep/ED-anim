using InfimaGames.LowPolyShooterPack;
using Sirenix.OdinInspector;
using UnityEngine;

public class EscapeCharacterLocalView : EscapeCharacterBehaviour {
    [SerializeField, Required] private Transform cameraTransform;

    public override CharacterTypes GetCharacterType() => CharacterTypes.LocalView;

    public override void OnUpdateView(EscapeCharacterState state) {
        this.cameraTransform.localRotation = Quaternion.AngleAxis(state.CameraRotationAngle, Vector3.right);

        base.OnUpdateView(state);
    }
}