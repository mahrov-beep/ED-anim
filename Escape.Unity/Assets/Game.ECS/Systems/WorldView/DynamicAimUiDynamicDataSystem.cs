namespace Game.ECS.Simulation.Systems.Units {
    using System;
    using Components.Unit;
    using ECS.Systems.Camera;
    using ECS.Systems.Core;
    using ECS.Systems.Unit;
    using Multicast;
    using Photon.Deterministic;
    using Quantum;
    using Scellecs.Morpeh;
    using Scripts;
    using Services.Photon;
    using Unity.Cinemachine;
    using UnityEngine;
    using UnityEngine.Pool;
    using Object = UnityEngine.Object;
    using SystemBase = Scellecs.Morpeh.SystemBase;

    public unsafe class DynamicAimUiDynamicDataSystem : SystemBase {
        [Inject] private PhotonService           photonService;
        [Inject] private LifetimeSystem          lifetimeSystem;
        [Inject] private UiDynamicContext        uiDynamicContext;
        [Inject] private QuantumEntityViewSystem quantumEntityViewSystem;
        [Inject] private CurrentCameraSystem     currentCameraSystem;

        [Inject] private Stash<UnitComponent> unitComponent;

        private ObjectPool<DynamicAimUiDynamicData>            uiDynamicDataPool;
        private SystemStateProcessor<DynamicAimStateComponent> processor;

        public override void OnAwake() {
            this.uiDynamicDataPool = new ObjectPool<DynamicAimUiDynamicData>(
                () => new DynamicAimUiDynamicData(this.lifetimeSystem.SceneLifetime),
                actionOnGet: data => this.uiDynamicContext.Add(data),
                actionOnRelease: data => this.uiDynamicContext.Remove(data));

            this.processor = this.World.Filter
                .With<UnitComponent>()
                .With<LocalCharacterMarker>()
                .ToSystemStateProcessor(this.InitAim, this.CleanupAim);
        }

        public override void Dispose() {
            base.Dispose();

            this.processor.Dispose();
        }

        public override unsafe void OnUpdate(float deltaTime) {
            if (this.photonService.PredictedFrame is not { } f) {
                return;
            }

            this.processor.Process();

            foreach (var entity in this.processor.Entities) {
                ref var unit  = ref this.unitComponent.Get(entity);
                ref var state = ref this.processor.States.Get(entity);

                if (!f.TryGet(unit.EntityRef, out UnitAim unitAim)) {
                    continue;
                }

                if (!this.quantumEntityViewSystem.TryGetEntityView(unitAim.AimEntity, out var view)) {
                    continue;
                }

                var aimTransform = view.transform;

                var weaponOwner = f.GetPointer<Unit>(unit.EntityRef);

                if (weaponOwner->ActiveWeaponRef == EntityRef.None) {
                    state.Data.Deactivated = true;
                    continue;
                }

                var weapon       = f.Get<Weapon>(weaponOwner->ActiveWeaponRef);
                var weaponConfig = weaponOwner->GetActiveWeaponConfig(f)!;

                if (weaponOwner->IsWeaponChanging || 
                    CharacterFsm.CurrentStateIs<CharacterStateSprint>(f, unit.EntityRef) ||
                    CharacterFsm.CurrentStateIs<CharacterStateJump>(f, unit.EntityRef)) {
                    state.Data.Deactivated = true;
                    continue;
                }

                Vector3 targetAimTargetPos, forwardAimTargetPos;
                if (f.GameModeAiming is ThirdPersonAimingAsset && 
                    this.currentCameraSystem.TryGetCurrentCameraExtension(out CinemachineThirdPersonAimQuantum thirdPersonAimQuantum)) {
                    forwardAimTargetPos = thirdPersonAimQuantum.AimTarget;
                    targetAimTargetPos  = aimTransform.position;
                }
                else {
                    targetAimTargetPos  = unitAim.AimCurrentPosition.ToUnityVector3();
                    forwardAimTargetPos = unitAim.AimCurrentPosition.ToUnityVector3();
                }

                var unitAsset           = f.FindAsset(weaponOwner->Asset);
                var crosshairSettings   = unitAsset.GetAimCrosshairSettings();
                bool isCrouching =
                    CharacterFsm.CurrentStateIs<CharacterStateCrouchIdle>(f, unit.EntityRef) ||
                    CharacterFsm.CurrentStateIs<CharacterStateCrouchMove>(f, unit.EntityRef);

                KCC* unitKcc     = null;
                bool hasUnitKcc  = f.TryGetPointer(unit.EntityRef, out unitKcc);
                bool isJumping   = hasUnitKcc && !unitKcc->Data.IsGrounded;

                var quality = 0;
                
                if (weaponOwner->HasTarget) {
                    if (f.TryGetPointer<CharacterLoadout>(weaponOwner->Target, out var loadout)) {
                        //Debug.Log($"quality {quality}");
                        quality = loadout->GetLoadoutQuality(f);
                    }
                }

                state.Data.Bullets            = weapon.BulletsCount;
                state.Data.MaxBullets         = weaponConfig.magazineAmmoCount;
                state.Data.TargetAimWorldPos  = targetAimTargetPos;
                state.Data.ForwardAimWorldPos = forwardAimTargetPos;
                state.Data.ShootingSpread     = Mathf.Lerp(state.Data.ShootingSpread, weapon.currentShootingSpread.AsFloat, Time.smoothDeltaTime * 15f);
                state.Data.Quality            = quality;
                state.Data.HasTarget          = weaponOwner->HasTarget;
                state.Data.IsTargetBlocked    = weaponOwner->IsTargetBlocked;
                state.Data.IsReloading        = weapon.IsReloading;
                state.Data.Deactivated        = false;

                float baseAimPercent = 1f;
                if (weapon.CurrentStats.preShotAimingSeconds > FP._0_01) {
                    var preShotSeconds = weapon.CurrentStats.preShotAimingSeconds.AsFloat;
                    if (preShotSeconds > Mathf.Epsilon) {
                        baseAimPercent = Mathf.Clamp01(weaponOwner->WeaponAimSecondsElapsed.AsFloat / preShotSeconds);
                    }
                }

                float jumpScale   = Mathf.Clamp01(crosshairSettings.JumpAimPercentScale.AsFloat);
                float crouchScale = Mathf.Clamp(crosshairSettings.CrouchAimPercentScale.AsFloat, 0.01f, 3f);

                float percentScale = 1f;
                if (isJumping) {
                    percentScale = jumpScale;
                }
                else if (isCrouching) {
                    percentScale = crouchScale;
                }

                float targetAimPercent = Mathf.Clamp01(baseAimPercent * percentScale);
                float smoothingSpeed   = Mathf.Max(0f, crosshairSettings.TransitionSpeed.AsFloat);

                if (!state.InitializedAimPercent) {
                    state.AimPercentSmooth      = targetAimPercent;
                    state.InitializedAimPercent = true;
                }
                else if (smoothingSpeed <= 0f) {
                    state.AimPercentSmooth = targetAimPercent;
                }
                else {
                    float lerpFactor = Mathf.Clamp01(Time.smoothDeltaTime * smoothingSpeed);
                    state.AimPercentSmooth = Mathf.Lerp(state.AimPercentSmooth, targetAimPercent, lerpFactor);
                }

                state.Data.AimPercent = state.AimPercentSmooth;
            }
        }

        private DynamicAimStateComponent InitAim(Entity entity) {
            var data = this.uiDynamicDataPool.Get();

            return new DynamicAimStateComponent {
                Data               = data,
                AimPercentSmooth   = 1f,
                InitializedAimPercent = false,
            };
        }

        private void CleanupAim(ref DynamicAimStateComponent state) {
            this.uiDynamicDataPool.Release(state.Data);
        }

        [Serializable, RequireFieldsInit]
        private struct DynamicAimStateComponent : ISystemStateComponent {
            public DynamicAimUiDynamicData Data;
            public float                   AimPercentSmooth;
            public bool                    InitializedAimPercent;
        }
    }
}