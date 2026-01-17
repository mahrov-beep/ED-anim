using UnityEngine;

public class EscapeMovementMenuBehaviour : AEscapeMovementBehaviour {
    public override float GetLastJumpTime() => 0;

    public override Vector3 GetVelocity() => Vector3.zero;

    public override bool IsGrounded()  => true;
    public override bool WasGrounded() => true;
    public override bool IsJumping()   => false;
    public override bool IsCrouching() => false;
}
