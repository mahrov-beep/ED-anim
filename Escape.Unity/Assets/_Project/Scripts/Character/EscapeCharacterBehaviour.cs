using System;
using _Project.Scripts.Character;
using Game.Services.Cheats;
using InfimaGames.LowPolyShooterPack;
using Multicast;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract partial class EscapeCharacterBehaviour : CharacterBehaviour {
    // ReSharper disable InconsistentNaming
    [SerializeField, Required] protected Animator                   characterAnimator;
    [SerializeField, Required] protected EscapeCharacterInventory   inventory;
    [SerializeField, Required] protected AEscapeMovementBehaviour   movement;
    [SerializeField, Required] protected CharacterAudioPlayer       audioPlayer;
    // ReSharper restore InconsistentNaming

    [SerializeField, Required]
    private CharacterConfig config;

    [Tooltip("Normal Camera.")]
    [SerializeField, Required]
    private CinemachineCamera cameraWorld;

    [Tooltip("Weapon-Only Camera. Depth.")]
    [SerializeField, Required]
    private Camera cameraDepth;

    [SerializeField, Required]
    private GameObject knife;

    private static IDebugFireSettingsService DebugFireSettings => App.Get<IDebugFireSettingsService>();

    [SerializeField, Required] private string layerActionsName         = "Layer Actions";
    [SerializeField, Required] private string layerOverlayName         = "Layer Overlay";
    [SerializeField, Required] private string layerFullBodyActionsName = "Layer FullBody Actions";

    [ShowInInspector, ReadOnly] private bool aiming;
    [ShowInInspector, ReadOnly] private bool wasAiming;
    [ShowInInspector, ReadOnly] private bool running;
    [ShowInInspector, ReadOnly] private bool healing;
    [ShowInInspector, ReadOnly] private float healingProgress;

    /// <summary>
    /// Alpha Aiming Value. Zero to one value representing aiming. Zero if we're not aiming, and one if we are fully aiming.
    /// </summary>
    [ShowInInspector, ReadOnly] private float aimingAlpha;
    /// <summary>
    /// Crouching Alpha. This value dictates how visible the crouching state is at any given time.
    /// </summary>
    [ShowInInspector, ReadOnly] private float crouchingAlpha;
    /// <summary>
    /// Running Alpha. This value dictates how visible the running state is at any given time.
    /// </summary>
    [ShowInInspector, ReadOnly] private float runningAlpha;

    /// <summary>
    /// Amount of shots fired in succession. We use this value to increase the spread, and also to apply recoil
    /// </summary>
    [ShowInInspector, ReadOnly] private int shotsFired;

    /// <summary>
    /// Look Axis Values.
    /// </summary>
    [ShowInInspector, ReadOnly] private Vector2 axisLook;

    /// <summary>
    /// Look Axis Values.
    /// </summary>
    [ShowInInspector, ReadOnly] private Vector2 axisMovement;

    /// <summary>
    /// Overlay Layer Index. Useful for playing things like firing animations.
    /// </summary>
    private int layerOverlay;
    /// <summary>
    /// Actions Layer Index. Used to play actions like reloading.
    /// </summary>
    private int layerActions;
    /// <summary>
    /// FullBody Actions Layer Index. Used to play actions like death and roll.
    /// </summary>
    private int layerFullBodyActions;

    private bool wasHealing;

    protected override void Awake() {
        base.Awake();

        this.layerActions         = this.characterAnimator.GetLayerIndex(this.layerActionsName);
        this.layerOverlay         = this.characterAnimator.GetLayerIndex(this.layerOverlayName);
        this.layerFullBodyActions = this.characterAnimator.GetLayerIndex(this.layerFullBodyActionsName);

        this.inventory.Init(0);
    }

    protected override void Start() {
        base.Start();

        if (this.knife != null) {
            this.knife.SetActive(false);
        }
    }

    public virtual void OnUpdateView(EscapeCharacterState state) {
        this.aiming  = state.IsAiming;
        this.running = state.IsRunning;
        this.healing = state.IsHealing;
        this.healingProgress = state.HealingProgress;
        this.axisLook     = state.AxisLookDelta;
        this.axisMovement = state.AxisMovement;

        this.movement.UpdateState(state);

        var debugFireSettings = DebugFireSettings;

        if (this.healing) {
            this.audioPlayer.PlayOneShot(CharacterAudioLayers.Action, this.config.audioClipsHealing);
        }

        if (!this.healing && this.wasHealing) {
            this.audioPlayer.StopLayer(CharacterAudioLayers.Action);
        } 

        if (debugFireSettings.DebugFireEnabled) {
            var keyboard   = Keyboard.current;
            var keyControl = keyboard != null ? keyboard[debugFireSettings.DebugFireKey] : null;
            if (keyControl != null && keyControl.wasPressedThisFrame && this.CanPlayAnimationFire()) {
                var camTransform = this.cameraWorld.transform;
                var origin       = camTransform.position;
                var direction    = camTransform.forward;

                if (Physics.Raycast(origin, direction, out var hitInfo, 300f, ~0, QueryTriggerInteraction.Ignore)) {
                    this.Fire(new WeaponFireData {
                        HasAmmoForNextShot = true,
                        HitPoint           = hitInfo.point,
                        HitNormal          = hitInfo.normal,
                        FromPosition       = origin,
                    });
                }
                else {
                    var fallbackPoint = origin + direction * 50f;
                    this.Fire(new WeaponFireData {
                        HasAmmoForNextShot = true,
                        HitPoint           = fallbackPoint,
                        HitNormal          = direction,
                        FromPosition       = origin,
                    });
                }
            }
        }

        {
            if (this.inventory.GetEquipped() is var equippedWeapon && equippedWeapon) {
                switch (this.aiming) {
                    case true when !this.wasAiming:
                        equippedWeapon.GetAttachmentManager().GetEquippedScope().OnAim();
                        
                        this.audioPlayer.PlayOneShot(CharacterAudioLayers.Action, this.config.audioClipsAimStart);

                        break;

                    case false when this.wasAiming:
                        equippedWeapon.GetAttachmentManager().GetEquippedScope().OnAimStop();
                 
                        this.audioPlayer.PlayOneShot(CharacterAudioLayers.Action, this.config.audioClipsAimStop);

                        break;
                }
            }
        }

        this.UpdateAnimator();

        //Update Aiming Alpha. We need to get this here because we're using the Animator to interpolate the aiming value.
        this.aimingAlpha = this.characterAnimator.GetFloat(AHashes.AimingAlpha);

        //Interpolate the crouching alpha. We do this here as a quick and dirty shortcut, but there's definitely better ways to do this.
        this.crouchingAlpha = Mathf.Lerp(this.crouchingAlpha, this.movement.IsCrouching() ? 1.0f : 0.0f, Time.deltaTime * 12.0f);
        //Interpolate the running alpha. We do this here as a quick and dirty shortcut, but there's definitely better ways to do this.
        this.runningAlpha = Mathf.Lerp(this.runningAlpha, this.running ? 1.0f : 0.0f, Time.deltaTime * this.config.runningInterpolationSpeed);

        //Running Field Of View Multiplier.
        var runningFieldOfView = Mathf.Lerp(1.0f, this.config.fieldOfViewRunningMultiplier, this.runningAlpha);

        {
            if (this.inventory.GetEquipped() is var equippedWeapon && equippedWeapon) {
                //Interpolate the world camera's field of view based on whether we are aiming or not.
                this.cameraWorld.Lens.FieldOfView = Mathf.Lerp(this.config.fieldOfView, this.config.fieldOfView * equippedWeapon.GetFieldOfViewMultiplierAim(), this.aimingAlpha) * runningFieldOfView;
                //Interpolate the depth camera's field of view based on whether we are aiming or not.
                this.cameraDepth.fieldOfView = Mathf.Lerp(this.config.fieldOfViewWeapon, this.config.fieldOfViewWeapon * equippedWeapon.GetFieldOfViewMultiplierAimWeapon(), this.aimingAlpha);
            }
        }

        //Save Aiming Value.
        this.wasAiming = this.aiming;

        this.wasHealing = this.healing;
    }

    private void UpdateAnimator() {
        // //Check if we're currently reloading cycled.
        // const string boolNameReloading = "Reloading";
        // if (characterAnimator.GetBool(boolNameReloading)) {
        //     //If we only have one more bullet to reload, then we can change the boolean already.
        //     if (equippedWeapon.GetAmmunitionTotal() - equippedWeapon.GetAmmunitionCurrent() < 1) {
        //         //Update the character animator.
        //         characterAnimator.SetBool(boolNameReloading, false);
        //         //Update the weapon animator.
        //         equippedWeapon.GetAnimator().SetBool(boolNameReloading, false);
        //     }
        // }

        // //Leaning. Affects how much the character should apply of the leaning additive animation.
        // float leaningValue = Mathf.Clamp01(axisMovement.y);
        // characterAnimator.SetFloat(AHashes.LeaningForward, leaningValue, 0.5f, Time.deltaTime);

        //Movement Value. This value affects absolute movement. Aiming movement uses this, as opposed to per-axis movement.
        var movementValue = Mathf.Clamp01(Mathf.Abs(this.axisMovement.x) + Mathf.Abs(this.axisMovement.y));
        this.characterAnimator.SetFloat(AHashes.Movement, movementValue, this.config.dampTimeLocomotion, Time.deltaTime);

        //Aiming Speed Multiplier.
        this.characterAnimator.SetFloat(AHashes.AimingSpeedMultiplier, this.config.aimingSpeedMultiplier);

        //Turning Value. This determines how much of the turning animation to play based on our current look rotation.
        this.characterAnimator.SetFloat(AHashes.Turning, Mathf.Abs(this.axisLook.x), this.config.dampTimeTurning, Time.deltaTime);

        //Horizontal Movement Float.
        this.characterAnimator.SetFloat(AHashes.Horizontal, this.axisMovement.x, this.config.dampTimeLocomotion, Time.deltaTime);
        //Vertical Movement Float.
        this.characterAnimator.SetFloat(AHashes.Vertical, this.axisMovement.y, this.config.dampTimeLocomotion, Time.deltaTime);

        //Update the aiming value, but use interpolation. This makes sure that things like firing can transition properly.
        this.characterAnimator.SetFloat(AHashes.AimingAlpha, Convert.ToSingle(this.aiming), this.config.dampTimeAiming, Time.deltaTime);

        //Set the locomotion play rate. This basically stops movement from happening while in the air.
        const string playRateLocomotionBool = "Play Rate Locomotion";
        this.characterAnimator.SetFloat(playRateLocomotionBool, this.movement.IsGrounded() ? 1.0f : 0.0f, 0.2f, Time.deltaTime);

        //Update Forward Multiplier. This allows us to change the play rate of our animations based on our movement multipliers.
        this.characterAnimator.SetFloat(AHashes.PlayRateLocomotionForward, this.config.walkingMultiplierForward, 0.2f, Time.deltaTime);
        //Update Sideways Multiplier. This allows us to change the play rate of our animations based on our movement multipliers.
        this.characterAnimator.SetFloat(AHashes.PlayRateLocomotionSideways, this.config.walkingMultiplierSideways, 0.2f, Time.deltaTime);
        //Update Backwards Multiplier. This allows us to change the play rate of our animations based on our movement multipliers.
        this.characterAnimator.SetFloat(AHashes.PlayRateLocomotionBackwards, this.config.walkingMultiplierBackwards, 0.2f, Time.deltaTime);

        //Update Animator Aiming.
        this.characterAnimator.SetBool(AHashes.Aim, this.aiming);
        //Update Animator Running.
        this.characterAnimator.SetBool(AHashes.Running, this.running);
        //Update Animator Crouching.
        this.characterAnimator.SetBool(AHashes.Crouching, this.movement.IsCrouching());
        
        this.characterAnimator.SetBool(AHashes.Healing, this.healing);

        this.characterAnimator.SetBool(AHashes.Jumping, this.movement.IsJumping());

        var equipped = this.GetInventory().GetEquipped();
        if (equipped != null) {
            // Sync weapon animator with character animator
            equipped.GetAnimator().SetBool(AHashes.Reloading, this.IsReloading());
        }
    }

    public static void SetDebugFireEnabled(bool enabled) => DebugFireSettings.DebugFireEnabled = enabled;
    public static bool GetDebugFireEnabled() => DebugFireSettings.DebugFireEnabled;
    public static void SetDebugFireKey(Key key) => DebugFireSettings.DebugFireKey = key;
    public static Key GetDebugFireKey() => DebugFireSettings.DebugFireKey;

    #region ANIMATION EVENTS

    public override void EjectCasing() {
        if (this.inventory.GetEquipped() is var equipped && equipped) {
            equipped.EjectCasing();
        }
    }

    public override void FillAmmunition(int amount) {
    }

    public override void Grenade() {
    }

    public override void SetActiveMagazine(int active) {
        if (this.inventory.GetEquipped() is var equipped && equipped &&
            equipped.GetAttachmentManager().GetEquippedMagazine() is var equippedMagazine & equippedMagazine) {
            equippedMagazine.gameObject.SetActive(active != 0);
        }
    }

    public override void SetSlideBack(int back) {
        if (this.inventory.GetEquipped() is var equipped && equipped) {
            equipped.SetSlideBack(back);
        }
    }

    public override void SetActiveKnife(int active) {
        this.knife.SetActive(active != 0);
    }

    #endregion
}