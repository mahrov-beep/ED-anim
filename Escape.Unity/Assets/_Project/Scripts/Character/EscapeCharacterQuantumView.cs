using _Project.Scripts.GameView;
using InfimaGames.LowPolyShooterPack;
using JetBrains.Annotations;
using Game.Domain.Game;
using Multicast;
using Quantum;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
using Weapon = Quantum.Weapon;

public class EscapeCharacterQuantumView : QuantumEntityViewComponent<CustomViewContext> {
    [SerializeField, Required] private EscapeCharacterBehaviour escapeCharacterBehaviour;
    [SerializeField, Required] private GameObject unarmedWeaponPrefab;
    private WeaponSetup unarmedWeaponSetup;
    private bool unarmedWeaponSetupCached;
    private GameLocalCharacterModel localCharacterModel;
    private bool wasHealing;
    private bool wasReloading;
    private bool knifeAnimationTriggered;
    private bool wasThrowingGrenade;
    private float currentSpeedCoefficient;
        
    private EntityRef lastActiveWeaponRef;

    private const float ANIMATOR_FADE = 0.025f;
    
    public bool IsAiming => this.TryGetPredictedQuantumComponent(out Unit unit) && unit.Aiming;

    public bool IsRunning => this.TryGetPredictedQuantumComponent(out CharacterFsm fsm) && fsm.CurrentState == CharacterStates.Sprint;

    public bool IsCrouching => this.TryGetPredictedQuantumComponent(out CharacterFsm fsm)
                               && (fsm.CurrentState == CharacterStates.CrouchIdle || fsm.CurrentState == CharacterStates.CrouchMove);

    public bool IsGrounded => this.TryGetPredictedQuantumComponent(out KCC kcc) && kcc.Data.IsGrounded;

    public bool IsJumping => this.TryGetPredictedQuantumComponent(out CharacterFsm fsm) && fsm.CurrentState == CharacterStates.Jump;

    public Vector3 Velocity => this.TryGetPredictedQuantumComponent(out InputContainer inputContainer)
        ? inputContainer.InputAccelerated.ToUnityVector3()
        : Vector3.zero;

    public Vector2 AxisMovement => this.TryGetPredictedQuantumComponent(out InputContainer inputContainer)
        ? inputContainer.InputAccelerated.XZ.ToUnityVector2()
        : Vector3.zero;

    public Vector2 AxisLookDelta => this.TryGetPredictedQuantumComponent(out InputContainer inputContainer)
        ? inputContainer.Input.LookRotationDelta.ToUnityVector2()
        : Vector2.zero;

    public float CameraRotationAngle {
        get {
            if (!this.TryGetPredictedQuantumComponent(out CharacterSpectatorCamera spectatorCamera)) {
                return 0f;
            }

            var spectatorCameraView = this.EntityView.EntityViewUpdater.GetView(spectatorCamera.CameraEntity);
            if (spectatorCameraView == null) {
                return 0f;
            }

            return ToAngle180(spectatorCameraView.transform.eulerAngles.x);
        }
    }

    public float VolumeMultiplierBoost => this.localCharacterModel.Stats.audioVolume.AdditiveMultiplierMinus1.AsFloat;

    public float MaxDistanceMultiplierBoost => this.localCharacterModel.Stats.audioDistance.AdditiveMultiplierMinus1.AsFloat;

    public float CurrentSpeedCoefficient => this.currentSpeedCoefficient;

    protected virtual void Awake() {
        this.localCharacterModel = App.Get<GameLocalCharacterModel>();
    }

    protected virtual void Start() {
        QuantumEvent.Subscribe(this, (EventOnWeaponAttackFired evt) => this.OnWeaponAttackFired(evt), onlyIfActiveAndEnabled: true);
    }

    public override void OnUpdateView() {
        if (!this.TryGetPredictedQuantumComponent(out Quantum.Unit unit)) {
            return;
        }

        this.escapeCharacterBehaviour.GetAudioPlayer().SetBoosts(this.VolumeMultiplierBoost, this.MaxDistanceMultiplierBoost);

        this.currentSpeedCoefficient = unit.CurrentSpeedCoefficient.AsFloat;

        if (!this.TryGetPredictedQuantumComponent(out AnimationTriggers animationTriggers)) {
            return;
        }

        var shouldPlayGrenade = animationTriggers.Throw;
        if (shouldPlayGrenade && !this.wasThrowingGrenade && this.escapeCharacterBehaviour.CanPlayAnimationGrenadeThrow()) {
            this.escapeCharacterBehaviour.PlayGrenadeThrow();
        }
        this.wasThrowingGrenade = shouldPlayGrenade;

        CharacterFsm characterFsm;
        var hasCharacterFsm = this.TryGetPredictedQuantumComponent(out characterFsm);

        var f = this.PredictedFrame;

        var healingProgress = 0f;
        var shouldHideWeaponsWhileHealing = false;
        var shouldHideWeaponsWhileGrenade = this.escapeCharacterBehaviour.IsThrowingGrenade();

        var isHealing = this.TryGetHealingStateData(
            f,
            hasCharacterFsm && characterFsm.CurrentState == CharacterStates.Healing,
            out healingProgress,
            out shouldHideWeaponsWhileHealing);
        this.escapeCharacterBehaviour.OnUpdateView(new EscapeCharacterState {
            IsAiming            = this.IsAiming,
            IsRunning           = this.IsRunning,
            IsCrouching         = this.IsCrouching,
            AxisLookDelta       = this.AxisLookDelta,
            AxisMovement        = this.AxisMovement,
            CameraRotationAngle = this.CameraRotationAngle,
            IsGrounded          = this.IsGrounded,
            IsJumping           = this.IsJumping,
            IsHealing           = isHealing,
            HealingProgress     = healingProgress,
        });

        if (isHealing && !this.wasHealing) {
            this.escapeCharacterBehaviour.PlayHealingAnimation();
        }
        this.wasHealing = isHealing;


        var isKnocked         = hasCharacterFsm && characterFsm.CurrentState == CharacterStates.Knocked;
        var shouldHideWeapons = isKnocked || shouldHideWeaponsWhileHealing;

        if (this.escapeCharacterBehaviour.GetCharacterType() != CharacterTypes.LocalView) {          
            this.escapeCharacterBehaviour.SetEquippedWeaponActive(!shouldHideWeaponsWhileGrenade);  
        }

        using (ListPool<WeaponSetup>.Get(out var activeWeaponPrefabs)) {
            var activeWeaponIndex = default(int?);

            if (f.Exists(unit.PrimaryWeapon)) {
                activeWeaponPrefabs.Add(CreateWeaponSetup(f, unit.PrimaryWeapon));

                if (unit.PrimaryWeapon == unit.ActiveWeaponRef) {
                    activeWeaponIndex = activeWeaponPrefabs.Count - 1;
                }
            }

            if (f.Exists(unit.SecondaryWeapon)) {
                activeWeaponPrefabs.Add(CreateWeaponSetup(f, unit.SecondaryWeapon));

                if (unit.SecondaryWeapon == unit.ActiveWeaponRef) {
                    activeWeaponIndex = activeWeaponPrefabs.Count - 1;
                }
            }

            if (activeWeaponPrefabs.Count == 0 && this.unarmedWeaponPrefab != null) {
                if (!this.unarmedWeaponSetupCached || this.unarmedWeaponSetup.WeaponPrefab != this.unarmedWeaponPrefab) {
                    this.unarmedWeaponSetup = new WeaponSetup {
                        WeaponPrefab = this.unarmedWeaponPrefab,
                    };
                    this.unarmedWeaponSetupCached = true;
                }

                activeWeaponPrefabs.Add(this.unarmedWeaponSetup);
                activeWeaponIndex = 0;
            }

            if (this.escapeCharacterBehaviour.CanAssignWeapons()) {
                var weaponIndexToAssign = shouldHideWeapons ? (int?)null : activeWeaponIndex;
                this.escapeCharacterBehaviour.AssignWeapons(activeWeaponPrefabs, weaponIndexToAssign);
            }
        }

        var weaponChanged = this.lastActiveWeaponRef != unit.ActiveWeaponRef;
        
        if (weaponChanged && this.wasReloading) {
            this.wasReloading = false;
            this.escapeCharacterBehaviour.GetCharacterAnimator().SetBool(AHashes.Reloading, false);

            var animator = this.escapeCharacterBehaviour.GetCharacterAnimator();
            animator.CrossFade("Default", ANIMATOR_FADE, animator.GetLayerIndex("Layer Actions"));
        }
        
        this.lastActiveWeaponRef = unit.ActiveWeaponRef;

        if (this.PredictedFrame.TryGet(unit.ActiveWeaponRef, out Weapon activeWeapon)) {
            var isReloading = activeWeapon.IsReloading;

            if (isReloading && !this.wasReloading && this.escapeCharacterBehaviour.CanPlayAnimationReload()) {
                this.wasReloading = true;
                this.escapeCharacterBehaviour.PlayReloadAnimation(emptyReload: false);
            }

            if (!isReloading) {
                this.wasReloading = false;
            }
        }
        else {
            this.wasReloading = false;
        }

        //Set the slide back if we just ran out of ammunition after last shot.
        if (false) {
            this.escapeCharacterBehaviour.SetSlideBack(1);
        }

        if (hasCharacterFsm) {
            var targetFullBodyState = characterFsm.CurrentState switch {
                CharacterStates.Dead => CharacterFullBodyStates.Died,
                CharacterStates.Knocked => CharacterFullBodyStates.Knocked,
                CharacterStates.Reviving => CharacterFullBodyStates.Reviving,
                CharacterStates.Roll => CharacterFullBodyStates.Roll,
                _ => CharacterFullBodyStates.Default,
            };

            if (this.escapeCharacterBehaviour.GetFullBodyState() != targetFullBodyState) {
                this.escapeCharacterBehaviour.PlayFullBody(targetFullBodyState);
            }
        }

        var isKnifeAttackingState = hasCharacterFsm && characterFsm.CurrentState == CharacterStates.KnifeAttack;
        var useLocalModelState = this.escapeCharacterBehaviour.GetCharacterType() == CharacterTypes.LocalView;
        var isKnifeAttacking = isKnifeAttackingState ||
                               (useLocalModelState ? (this.localCharacterModel?.IsKnifeAttacking ?? false) : false);
        if (isKnifeAttacking && !this.knifeAnimationTriggered) {
            if (this.escapeCharacterBehaviour.CanPlayAnimationMelee()) {
                this.escapeCharacterBehaviour.PlayMelee();
            }

            var characterConfig = this.escapeCharacterBehaviour.GetConfig();
            if (characterConfig != null && this.escapeCharacterBehaviour.GetCharacterType() == CharacterTypes.LocalView) {
                this.escapeCharacterBehaviour.GetAudioPlayer().PlayOneShot(
                    CharacterAudioLayers.Action,
                    characterConfig.audioClipsMelee);
            }

            this.knifeAnimationTriggered = true;
        }

        if (!isKnifeAttacking) {
            this.knifeAnimationTriggered = false;
        }

        /*
        if (this.TryGetPredictedQuantumComponent(out CharacterFsm characterFsm)) {
            this.fpsCharacter.OnMovementState(characterFsm.CurrentState switch {
                CharacterStates.Idle => FPSCharacter.FPSMovementState.Idle,
                CharacterStates.Walk => FPSCharacter.FPSMovementState.Walking,
                CharacterStates.Roll => FPSCharacter.FPSMovementState.Idle,
                CharacterStates.Dead => FPSCharacter.FPSMovementState.Idle,
                CharacterStates.Sprint => FPSCharacter.FPSMovementState.Sprinting,
                _ => FPSCharacter.FPSMovementState.Idle,
            });
        }
         */
    }

    private bool TryGetHealingStateData(Frame frame, bool isHealingState, out float healingProgress, out bool shouldHideWeaponsWhileHealing) {
        healingProgress = 0f;
        shouldHideWeaponsWhileHealing = false;

        if (!isHealingState || frame == null) {
            return false;
        }

        if (!frame.TryGet(this.EntityRef, out CharacterStateHealing healingState)) {
            return false;
        }

        var duration  = Mathf.Max(0f, healingState.Duration.AsFloat);
        var remaining = Mathf.Max(0f, healingState.Timer.AsFloat);

        healingProgress = duration <= 0f ? 1f : 1f - Mathf.Clamp01(remaining / duration);

        var hideDelay = 0f;
        if (healingState.ItemEntity != EntityRef.None &&
            frame.TryGet(healingState.ItemEntity, out Item healingItem) &&
            frame.FindAsset(healingItem.Asset) is HealBoxItemAsset healAsset) {
            hideDelay = healAsset.hideWeaponsDelaySeconds.AsFloat;
        }

        var elapsed = duration - remaining;
        shouldHideWeaponsWhileHealing = elapsed >= hideDelay;
        return true;
    }

    private unsafe void OnWeaponAttackFired(EventOnWeaponAttackFired evt) {
        if (evt.sourceUnitRef != this.EntityRef) {
            return;
        }

        var frame = this.PredictedFrame;
        var isKnifeAttack = false;

        if (frame != null && frame.TryGetPointer(this.EntityRef, out Quantum.Unit* unit)) {
            if (unit->MeleeWeapon != EntityRef.None &&
                frame.TryGetPointer(unit->MeleeWeapon, out Weapon* meleeWeapon)) {
                isKnifeAttack = meleeWeapon->Config == evt.weaponConfig;
            }
        }

        if (isKnifeAttack) {
            if (this.escapeCharacterBehaviour.CanPlayAnimationMelee()) {
                this.escapeCharacterBehaviour.PlayMelee();
            }

            var characterConfig = this.escapeCharacterBehaviour.GetConfig();
            if (characterConfig != null && this.escapeCharacterBehaviour.GetCharacterType() == CharacterTypes.LocalView) {
                this.escapeCharacterBehaviour.GetAudioPlayer().PlayOneShot(
                    CharacterAudioLayers.Action,
                    characterConfig.audioClipsMelee);
            }

            this.knifeAnimationTriggered = true;
            return;
        }

        if (this.escapeCharacterBehaviour.CanPlayAnimationFire()) {
            this.escapeCharacterBehaviour.Fire(new WeaponFireData {
                HasAmmoForNextShot = true,
                HitPoint           = evt.hitPoint.ToUnityVector3(),
                HitNormal          = evt.hitNormal.HasValue ? evt.hitNormal.Value.ToUnityVector3() : null,
                FromPosition       = evt.fromPosition.ToUnityVector3(),
            });
        }
    }

    public static WeaponSetup CreateWeaponSetup(Frame f, EntityRef weaponRef) {
        var weapon     = f.Get<Weapon>(weaponRef);
        var weaponItem = f.Get<WeaponItem>(weaponRef);
        var config     = f.FindAsset(weapon.Config);

        return new WeaponSetup {
            WeaponPrefab = config.visualPrefab,

            ScopePrefab    = GetAttachmentPrefab(f, weaponItem.AttachmentAtSlot(WeaponAttachmentSlots.Scope)),
            GripPrefab     = GetAttachmentPrefab(f, weaponItem.AttachmentAtSlot(WeaponAttachmentSlots.Grip)),
            MuzzlePrefab   = GetAttachmentPrefab(f, weaponItem.AttachmentAtSlot(WeaponAttachmentSlots.Muzzle)),
            MagazinePrefab = GetAttachmentPrefab(f, weaponItem.AttachmentAtSlot(WeaponAttachmentSlots.Magazine)),
            StockPrefab    = GetAttachmentPrefab(f, weaponItem.AttachmentAtSlot(WeaponAttachmentSlots.Stock)),
            LaserPrefab    = GetAttachmentPrefab(f, weaponItem.AttachmentAtSlot(WeaponAttachmentSlots.Laser)),
        };
    }

    [CanBeNull]
    private static GameObject GetAttachmentPrefab(Frame f, EntityRef attachmentRef) {
        if (!f.Exists(attachmentRef)) {
            return null;
        }

        if (!f.TryGet(attachmentRef, out Item item)) {
            return null;
        }

        if (f.FindAsset(item.Asset) is not WeaponAttachmentItemAsset attachmentItemAsset) {
            return null;
        }

        return attachmentItemAsset.visualPrefab;
    }

    // return angle in range -180..+180
    private static float ToAngle180(float angle) {
        return angle > 180 ? angle - 360 : angle;
    }
}
