namespace Game.ECS.Systems.Input {
    using System;
    using Camera;
    using Components.Camera;
    using Components.Quantum;
    using Components.Unit;
    using Multicast;
    using Photon.Deterministic;
    using Player;
    using Quantum;
    using Quantum.Commands;
    using Scellecs.Morpeh;
    using Services.Photon;
    using Unity.Cinemachine;
    using UnityEngine;
    using UnityEngine.InputSystem;

    public sealed class InputPollSystem : LastPreUpdateSystem {

        public ref Quantum.Input AccumulatedInput => ref accumulatedInput;
        public     FPVector2     LastLookDeltaRad { get; private set; }
        public     FPVector2     LookRotation     => _lookRotation;

        private FPVector2 _lookRotation;

        private const string RELOAD_ACTION                  = "Reload";
        private const string SELECT_PRIMARY_WEAPON_ACTION   = "SelectPrimaryWeapon";
        private const string SELECT_SECONDARY_WEAPON_ACTION = "SelectSecondaryWeapon";
        private const string SELECT_MELEE_WEAPON_ACTION     = "SelectMeleeWeapon";
        private const string JUMP_ACTION                    = "Jump";
        private const string CROUCH_ACTION                  = "Ctrl";

        [Inject] private PlayerInputConfig playerInputConfig;

        [Inject] private PhotonService photonService;

        [Inject] private LocalPlayerSystem   localPlayerSystem;
        [Inject] private CurrentCameraSystem currentCameraSystem;

        [Inject] private Stash<PlayerInputComponent>              stashPlayerInput;
        [Inject] private Stash<QuantumEntityViewUpdaterComponent> stashUpdater;

        private Filter filterPlayerInput;
        private Filter filterViewUpdater;

        private PlayerInput              input;
        private QuantumEntityViewUpdater updater;

        private Quantum.Input accumulatedInput;
        private bool          resetAccumulatedInput;
        private int           lastAccumulateFrame;

        private IDisposable pollInputCallback;

        public override void OnDispose() {
            pollInputCallback.Dispose();
        }

        public override void Awake() {
            filterPlayerInput = World.Filter.With<PlayerInputComponent>().Build();
            filterViewUpdater = World.Filter.With<QuantumEntityViewUpdaterComponent>().Build();

            pollInputCallback = QuantumCallback.SubscribeManual<CallbackPollInput>(PollInput);
        }

        private void PollInput(CallbackPollInput callback) {
            AccumulateInput();

            var snapshot = updater.SnapshotInterpolation;
            accumulatedInput.InterpolationOffset = (byte)Mathf.Clamp(callback.Frame - snapshot.CurrentFrom, 0, 255);
            accumulatedInput.InterpolationAlpha  = snapshot.Alpha.ToFP();

            callback.SetInput(accumulatedInput, DeterministicInputFlags.Repeatable);
            accumulatedInput.LookRotationDelta = FPVector2.Zero;
        }

        public override void OnLastPreUpdate(float deltaTime) {
            if (!photonService.TryGetPredicted(out _)) {
                return;
            }

            if (filterPlayerInput.IsEmpty() || stashPlayerInput.IsEmpty()) {
                return;
            }

            if (filterViewUpdater.IsEmpty() || stashUpdater.IsEmpty()) {
                return;
            }

            var inputEntity   = filterPlayerInput.FirstOrDefault();
            var updaterEntity = filterViewUpdater.FirstOrDefault();

            if (!stashPlayerInput.Has(inputEntity) || !stashUpdater.Has(updaterEntity)) {
                return;
            }

            input   = stashPlayerInput.Get(inputEntity).playerInput;
            updater = stashUpdater.Get(updaterEntity).updater;

            AccumulateInput();
            OnReloadButton();
            OnSelectPrimaryWeaponButton();
            OnSelectSecondaryWeaponButton();
            OnSelectMeleeWeaponButton();
            OnJumpButton();
            OnCrouchButton();
        }

        void AccumulateInput() {
            if (lastAccumulateFrame == Time.frameCount) {
                return;
            }

            lastAccumulateFrame = Time.frameCount;

            if (resetAccumulatedInput) {
                resetAccumulatedInput = false;
                accumulatedInput      = default;
            }

            ProcessStandaloneInput();
        }

        unsafe void ProcessStandaloneInput() {
            if (!photonService.TryGetPredicted(out var f)) return;
            if (localPlayerSystem.HasNotLocalEntityRef(out var localRef)) return;

            accumulatedInput.SecondaryAction = input.actions["UseAbility"].IsPressed();
            accumulatedInput.AimButton       = input.actions["AimButton"].IsPressed();

            var moveDirection = input.actions["Move"].ReadValue<Vector2>().ToFPVector2();

            var lookDeltaScreen = input.actions["LookDelta_Screen"];
            var lookDeltaMouse  = input.actions["LookDelta_Mouse"];

            var lookDelta = (lookDeltaScreen.enabled ? lookDeltaScreen.ReadValue<Vector2>().ToFPVector2() : FPVector2.Zero) +
                            (lookDeltaMouse.enabled ? lookDeltaMouse.ReadValue<Vector2>().ToFPVector2() : FPVector2.Zero);

            var isSprint = input.actions["Sprint"].IsPressed();

            var sensitivity = PlayerPrefs.GetFloat("Settings_MouseSensitivity", playerInputConfig.MouseSensitivityDefault);
            lookDelta *= sensitivity.ToFP();

            lookDelta   *= playerInputConfig.lookDeltaMultiplier;
            lookDelta.Y *= -1;

            var unit = f.GetPointer<Unit>(localRef);
            lookDelta *= unit->CurrentStats.rotationSpeed.AsFP;

            LastLookDeltaRad = lookDelta;

            accumulatedInput.LookRotationDelta += lookDelta;

            _lookRotation                 += lookDelta;
            accumulatedInput.LookRotation =  _lookRotation;

            FP moveMagnitude       = FPMath.Clamp01(moveDirection.Magnitude);
            FP curvedMoveMagnitude = playerInputConfig.movementSpeedCurve.Evaluate(moveMagnitude);

            if (isSprint && moveMagnitude < FP._0_99) {
                isSprint = false;
            }

            accumulatedInput.SprintButton      = isSprint;
            accumulatedInput.MovementDirection = moveDirection.Normalized;
            accumulatedInput.MovementMagnitude = curvedMoveMagnitude;

            if (f.GameModeAiming is ThirdPersonAimingAsset &&
                this.currentCameraSystem.TryGetCurrentCameraExtension(out CinemachineThirdPersonAimQuantum thirdPersonAimQuantum)) {
                accumulatedInput.AimTarget = thirdPersonAimQuantum.AimTarget.ToFPVector3();
            }
            else if (f.GameModeAiming is FirstPersonAimingAsset) {
                if (this.currentCameraSystem.TryGetCurrentCameraExtension(out CinemachineFirstPersonAimQuantum firstPersonAimQuantum)) {
                    accumulatedInput.AimTarget = firstPersonAimQuantum.AimTarget.ToFPVector3();
                }
                else {
                    accumulatedInput.AimTarget = FPVector3.Zero;
                }
            }
            else {
                accumulatedInput.AimTarget = FPVector3.Forward;
            }

//            Debug.LogWarning($"{Time.frameCount}: {accumulatedInput.AimTarget} , {accumulatedInput.LookRotation}");
        }

        void OnReloadButton() {
            if (input.actions[RELOAD_ACTION].ReadValue<float>() > 0) {
                SendCommand(new ReloadWeaponCommand());
            }
        }

        void OnSelectPrimaryWeaponButton() {
            var action = input.actions[SELECT_PRIMARY_WEAPON_ACTION];
            if (action != null && action.WasPressedThisFrame()) {
                SendCommand(new SelectWeaponCommand {
                    SlotType = CharacterLoadoutSlots.PrimaryWeapon,
                });
            }
        }

        void OnSelectSecondaryWeaponButton() {
            var action = input.actions[SELECT_SECONDARY_WEAPON_ACTION];
            if (action != null && action.WasPressedThisFrame()) {
                SendCommand(new SelectWeaponCommand {
                    SlotType = CharacterLoadoutSlots.SecondaryWeapon,
                });
            }
        }

        void OnSelectMeleeWeaponButton() {
            var action = input.actions[SELECT_MELEE_WEAPON_ACTION];
            if (action != null && action.WasPressedThisFrame()) {
                SendCommand(new SelectWeaponCommand {
                    SlotType = CharacterLoadoutSlots.MeleeWeapon,
                });
            }
        }

        unsafe void OnJumpButton() {
            var jumpAction = input.actions[JUMP_ACTION];
            if (jumpAction == null || !jumpAction.WasPressedThisFrame()) {
                return;
            }

            if (photonService.TryGetPredicted(out var f) &&
                !localPlayerSystem.HasNotLocalEntityRef(out var localRef) &&
                f.TryGetPointer(localRef, out Unit* unit) &&
                f.TryGetPointer(localRef, out UnitFeatureSprintWithStamina* stamina)) {

                var unitAsset    = f.FindAsset(unit->Asset);
                var jumpSettings = unitAsset.GetJumpSettings();
                
                FP requiredStamina = jumpSettings.GetRequiredStamina(unitAsset.sprintSettings.maxStamina);
                if (stamina->current < requiredStamina) {
                    QuantumEvent.Dispatcher.Publish(new EventStaminaJumpDenied());
                    return;
                }
            }

            SendCommand(new JumpCommand());
        }

        void OnCrouchButton() {
            var crouchAction = input.actions[CROUCH_ACTION];
            if (crouchAction != null && crouchAction.WasPressedThisFrame()) {
                SendCommand(new CrouchCommand());
            }
        }

        void SendCommand(DeterministicCommand command) {
            var result = QuantumRunner.DefaultGame.SendCommand(command);
            if (result != DeterministicCommandSendResult.Success) {
                Debug.LogError(result.ToString());
            }
        }
    }
}
