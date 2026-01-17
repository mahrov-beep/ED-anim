using UnityEngine;

[RequireFieldsInit]
public struct EscapeCharacterState {
    public bool    IsAiming;
    public bool    IsRunning;
    public Vector2 AxisLookDelta;
    public Vector2 AxisMovement;
    public float   CameraRotationAngle;

    public bool IsCrouching;
    public bool IsGrounded;
    public bool IsJumping;
    public bool IsHealing;
    public float HealingProgress;
}
