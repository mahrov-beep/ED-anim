using InfimaGames.LowPolyShooterPack;
using Sirenix.OdinInspector;
using UnityEngine;

public class EscapeMovementBehaviour : AEscapeMovementBehaviour {
    [SerializeField, Required] private EscapeCharacterQuantumView quantumView;
    [SerializeField, Required] private EscapeCharacterBehaviour   characterBehaviour;

    private bool  isJumping;
    private bool  isGrounded;
    private bool  isCrouching;
    private float lastJumpTime;

    private bool wasCrouching;
    private bool wasJumping;
    private bool wasGrounded;

    private float timerNotGrounded;

    public override void UpdateState(EscapeCharacterState state) {
        this.wasCrouching = this.isCrouching;
        this.wasGrounded  = this.isGrounded;
        this.wasJumping   = this.isJumping;

        this.isGrounded  = state.IsGrounded;
        this.isJumping   = state.IsJumping;
        this.isCrouching = state.IsCrouching;

        if (this.isJumping && !this.wasJumping) {
            this.lastJumpTime = Time.time;

            this.characterBehaviour.GetAudioPlayer().PlayOneShot(CharacterAudioLayers.Env, this.characterBehaviour.GetConfig().audioClipsJumpStart);
        }

        if (state.IsGrounded && !this.wasGrounded) {
            if (this.timerNotGrounded >= this.characterBehaviour.GetConfig().minimalTimeForGroundedAudio) {
                this.characterBehaviour.GetAudioPlayer().PlayOneShot(CharacterAudioLayers.Env, this.characterBehaviour.GetConfig().audioClipsJumpLand);
            }
        }

        if (state.IsGrounded) {
            this.timerNotGrounded = 0;
        }
        else {
            this.timerNotGrounded += Time.deltaTime;
        }

        if (this.isCrouching && !this.wasCrouching) {
            this.characterBehaviour.GetAudioPlayer().PlayOneShot(CharacterAudioLayers.Env, this.characterBehaviour.GetConfig().audioClipsCrouchDown);
        }

        if (!this.isCrouching && this.wasCrouching) {
            this.characterBehaviour.GetAudioPlayer().PlayOneShot(CharacterAudioLayers.Env, this.characterBehaviour.GetConfig().audioClipsCrouchUp);
        }
    }

    public override float GetLastJumpTime() => this.lastJumpTime;

    public override Vector3 GetVelocity() => this.quantumView.Velocity;

    public override bool IsGrounded()  => this.isGrounded;
    public override bool WasGrounded() => this.wasGrounded;
    public override bool IsJumping()   => this.isJumping;
    public override bool IsCrouching() => this.isCrouching;
}

public abstract class AEscapeMovementBehaviour : MovementBehaviour {
    public virtual void UpdateState(EscapeCharacterState state) {
    }
}