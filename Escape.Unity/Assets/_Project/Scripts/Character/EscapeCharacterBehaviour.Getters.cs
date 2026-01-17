using InfimaGames.LowPolyShooterPack;
using Unity.Cinemachine;
using UnityEngine;

public partial class EscapeCharacterBehaviour {
    public override CharacterConfig GetConfig() => this.config;

    public override Animator GetCharacterAnimator() => this.characterAnimator;

    public override CharacterAudioPlayer GetAudioPlayer() => this.audioPlayer;

    public override int GetShotsFired() => this.shotsFired;

    public override bool IsLowered() => false;

    public override CinemachineCamera GetCameraWorld() => this.cameraWorld;
    public override Camera            GetCameraDepth() => this.cameraDepth;

    public override InventoryBehaviour GetInventory() => this.inventory;

    public override Vector2 GetInputMovement() => this.axisMovement;
    public override Vector2 GetInputLook()     => this.axisLook;

    public override bool IsRunning()   => this.running;
    public override bool IsCrouching() => this.movement.IsCrouching();
    public override bool IsAiming()    => this.aiming;

    public override bool IsHealing() => this.healing;
    public override float GetHealingProgress01() => this.healingProgress;
}
