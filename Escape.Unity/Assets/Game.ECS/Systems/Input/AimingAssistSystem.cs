namespace Game.ECS.Systems.Input {
    using System.Diagnostics;
    using Camera;
    using Components.Unit;
    using Game.Domain.GameProperties;
    using Multicast;
    using Multicast.GameProperties;
    using Photon.Deterministic;
    using Player;
    using Quantum;
    using Scellecs.Morpeh;
    using Services.Photon;
    using Unit;
    using Unity.Cinemachine;
    using UnityEngine;
    using Debug = UnityEngine.Debug;
    using Object = UnityEngine.Object;

    public class AimingAssistSystem : LastPreUpdateSystem {
        [Inject] private PlayerInputConfig   inputConfig;
        [Inject] private GamePropertiesModel gameProperties;

        [Inject] private Stash<VisiblyInFrustumMarker> stashVisiblyInFrustum;
        [Inject] private Stash<EnemyUnitMarker>        stashEnemy;
        [Inject] private Stash<UnitComponent>          stashUnit;
        [Inject] private Stash<AimAssistTarget>        stashAimAssistTarget;
        [Inject] private Stash<LocalCharacterMarker>   stashLocalCharacterMarker;

        [Inject] private LocalPlayerSystem                 localPlayerSystem;
        [Inject] private CurrentCameraSystem               currentCameraSystem;
        [Inject] private InputPollSystem                   inputPollSystem;
        [Inject] private MapperUnitEntityRefToEntitySystem mapperUnitEntityRefToEntity;
        [Inject] private MapperAimTargetGoToUnitEntity     mapperAimTargetGoToUnitEntity;

        [Inject] private PhotonService photonService;

        private Filter filterVisiblyInFrustumEnemy;
        private Filter filterAimAssistTargets;

        private AimAssistZoneDrawer debugAIMAssistZoneDrawer;

        private Entity selectedTarget;

        private float lastFov;
        private float lastFovFactor;

        private float  lastAssistAppliedDeg;
        private string lastActiveMode = "None";

        private Camera mainCamera;

        private readonly Vector2 screenSize   = new(Screen.width, Screen.height);
        private readonly Vector2 screenCenter = new(Screen.width * .5f, Screen.height * .5f);

        public override void Awake() {
            filterVisiblyInFrustumEnemy = World.Filter.With<VisiblyInFrustumMarker>().With<EnemyUnitMarker>().Build();
            filterAimAssistTargets      = World.Filter.With<AimAssistTarget>().Build();
        }

        public override void OnDispose() {
            stashAimAssistTarget.RemoveAll();
        }

        public override unsafe void OnLastPreUpdate(float deltaTime) {
            if (gameProperties == null || !gameProperties.Get(DebugGameProperties.Booleans.EnableAimAssist)) {
                return;
            }

            if (!photonService.TryGetPredicted(out var f)) {
                return;
            }

            if (localPlayerSystem.HasNotLocalEntityRef(out var localRef)) {
                return;
            }

            if (CharacterFsm.CurrentStateIs<CharacterStateDead>(f, localRef) || filterVisiblyInFrustumEnemy.IsEmpty()) {
                stashAimAssistTarget.RemoveAll();
                selectedTarget = null;

                debugAIMAssistZoneDrawer?.SetZones(default, default);
                return;
            }

            if (!mainCamera) {
                mainCamera = Camera.main;
            }

            var isThirdAiming = f.GameModeAiming is ThirdPersonAimingAsset;

            if (!this.currentCameraSystem.TryGetCurrentCameraExtension(out CinemachineThirdPersonAimQuantum thirdPersonAimQuantum) && isThirdAiming) {
                return;
            }

            RemoveNotValidTargets(f);
            UpdateRectsForVisiblyInFrustumTargets(mainCamera.fieldOfView, f);
            TickAimAssistTimers(deltaTime);

            if (isThirdAiming) {
                RaycastHitFromCamera(thirdPersonAimQuantum);
            }

            QuantumRaycastFromCameraCenter(f);

            SelectTarget();

            DebugDrawSelectedTarget();

            if (!HasSelectedTarget()) {
                return;
            }

            var isMoveAimToTarget = IsMoveAimToTarget();

            UpdateAimAssistPower(deltaTime, isMoveAimToTarget);
            ApplyAimAssist(deltaTime, isMoveAimToTarget, f, localRef, thirdPersonAimQuantum);
        }

        private void RemoveNotValidTargets(Frame f) {
            if (filterVisiblyInFrustumEnemy.IsEmpty()) {
                stashAimAssistTarget.RemoveAll();
                debugAIMAssistZoneDrawer?.SetZones(default, default);

                return;
            }

            foreach (var entity in filterAimAssistTargets) {
                if (!stashVisiblyInFrustum.Has(entity)) {
                    // цель пропала из фруснума - мы не можем на нее наводиться, она потеряна

                    stashAimAssistTarget.Remove(entity);

                    if (selectedTarget == entity) {
                        selectedTarget = null;
                    }

                    continue;
                }

                ref var unit = ref stashUnit.Get(entity);

                if (CharacterFsm.CurrentStateIs<CharacterStateDead>(f, unit.EntityRef)) {
                    stashAimAssistTarget.Remove(entity);
                    continue;
                }

                if (CharacterFsm.CurrentStateIs<CharacterStateKnocked>(f, unit.EntityRef)) {
                    stashAimAssistTarget.Remove(entity);
                    continue;
                }
            }
        }

        private void UpdateRectsForVisiblyInFrustumTargets(float fieldOfView, Frame f) {
            ref var localUnitComponent = ref stashUnit.Get(localPlayerSystem.LocalEntity);

            var localPos = localUnitComponent.PositionView;

            var cameraFovRadians                 = fieldOfView * 0.5f * Mathf.Deg2Rad;
            var pixelsPerWorldUnitAtUnitDistance = screenSize.y / (2f * Mathf.Tan(cameraFovRadians));
            var objectWidthScreenFactor          = pixelsPerWorldUnitAtUnitDistance * inputConfig.characterWidth;

            foreach (var entity in filterVisiblyInFrustumEnemy) {
                if (!stashAimAssistTarget.Has(entity)) {
                    stashAimAssistTarget.Set(entity, new AimAssistTarget());
                }

                ref var targetUnit = ref stashUnit.Get(entity);
                var targetHeightFP = UnitColliderHeightHelper.GetCurrentHeight(f, targetUnit.EntityRef);
                var targetHeight   = targetHeightFP.AsFloat;

                var objectHeightScreenFactor = pixelsPerWorldUnitAtUnitDistance * targetHeight;

                CreateRect(localPos, objectHeightScreenFactor, objectWidthScreenFactor, entity, targetHeight);
            }
        }

        private void CreateRect(
                        Vector3 localPos,
                        float objectHeightScreenFactor,
                        float objectWidthScreenFactor,
                        Entity entity,
                        float targetHeight) {

            ref var targetUnit = ref stashUnit.Get(entity);

            var distance = Vector3.Distance(localPos, targetUnit.PositionView);
            if (distance > inputConfig.aimAssistMaxDistance) {
                return;
            }

            var objectScreenPixelHeight = objectHeightScreenFactor / distance;
            var objectScreenPixelWidth  = objectWidthScreenFactor / distance;

            var outerZoneWidth  = objectScreenPixelWidth * inputConfig.outerZoneMultiplierWidth;
            var outerZoneHeight = objectScreenPixelHeight * inputConfig.outerZoneMultiplierHeight;
            var innerZoneWidth  = objectScreenPixelWidth * inputConfig.innerZoneMultiplierWidth;
            var innerZoneHeight = objectScreenPixelHeight * inputConfig.innerZoneMultiplierHeight;

            var outerZoneSize = new Vector2(outerZoneWidth, outerZoneHeight);
            var innerZoneSize = new Vector2(innerZoneWidth, innerZoneHeight);

            var enemyWorldPos = targetUnit.PositionView + Vector3.up * (targetHeight * 0.5f);

            Vector2 enemyScreenPos = mainCamera.WorldToScreenPoint(enemyWorldPos);

            Rect outerZone = new(enemyScreenPos - outerZoneSize * 0.5f, outerZoneSize);
            Rect innerZone = new(enemyScreenPos - innerZoneSize * 0.5f, innerZoneSize);

            ref var aimAssistTarget = ref stashAimAssistTarget.Get(entity);

            aimAssistTarget.outerRect = outerZone;
            aimAssistTarget.innerRect = innerZone;
            aimAssistTarget.screenPos = enemyScreenPos;

            // Debug.LogWarning($"EnemyScreenPosition {enemyScreenPos}  " +
            //                 $"(w:{objectScreenPixelWidth}, h:{objectScreenPixelHeight})\n" +
            //                 $"o = {outerZone.Contains(screenCenter)}, i = {innerZone.Contains(screenCenter)}");
        }

        private void TickAimAssistTimers(float deltaTime) {
            foreach (var entity in filterAimAssistTargets) {
                if (!stashAimAssistTarget.Has(entity)) {
                    continue;
                }

                ref var aimAssistTarget = ref stashAimAssistTarget.Get(entity);

                // тикаем таймер связанный с попаданием рейкастом
                aimAssistTarget.durationWithoutAimRaycastHit += deltaTime;

                // обновляем таймеры связанные с внешней зоной
                var isAimInTargetRect = aimAssistTarget.outerRect.Contains(screenCenter);
                if (isAimInTargetRect) {
                    aimAssistTarget.durationAimInOuterZone            += deltaTime;
                    aimAssistTarget.durationWithoutCrossOuterZoneRect =  0;
                }
                else {
                    aimAssistTarget.durationAimInOuterZone            =  0;
                    aimAssistTarget.durationWithoutCrossOuterZoneRect += deltaTime;
                }
            }
        }

        private void RaycastHitFromCamera(CinemachineThirdPersonAimQuantum thirdPersonAimQuantum) {
            if (!thirdPersonAimQuantum.AimTargetObject) {
                return;
            }

            if (!mapperAimTargetGoToUnitEntity.TryGet(thirdPersonAimQuantum.AimTargetObject, out var hitEntity)) {
                return;
            }

            if (!stashAimAssistTarget.Has(hitEntity)) {
                return;
            }

            stashAimAssistTarget.Get(hitEntity).durationWithoutAimRaycastHit = 0;
        }

        private unsafe void QuantumRaycastFromCameraCenter(Frame f) {
            var sourcePosition = mainCamera.ScreenToWorldPoint(screenCenter).ToFPVector3();
            var endPoint       = (mainCamera.transform.forward * inputConfig.aimAssistMaxDistance).ToFPVector3();

            var hitCollection = f.Physics3D.LinecastAll(
                sourcePosition,
                sourcePosition + endPoint,
                PhysicsHelper.GetBlockRaycastLayerMask(f),
                QueryOptions.HitStatics |
                QueryOptions.HitKinematics |
                QueryOptions.HitDynamics |
                QueryOptions.ComputeDetailedInfo);

            hitCollection.SortCastDistance();

            if (hitCollection.Count <= 0) {
                return;
            }

            var localRef = localPlayerSystem.LocalRef!.Value;

            for (var i = 0; i < hitCollection.Count; i++) {
                var hit = hitCollection[i];
                if (hit.Entity == localRef) {
                    // пропускаем сами себя
                    continue;
                }

                if (hit.IsDynamic) {
                    if (hit.Entity != EntityRef.None) {
                        if (f.IsAlly(localRef, hit.Entity)) {
                            // целимся в союзника
                            break;
                        }

                        if (mapperUnitEntityRefToEntity.TryGet(hit.Entity, out var hitEntity)) {
                            ref var hitTarget = ref stashAimAssistTarget.Get(hitEntity);

                            // попали по какой-то энтити, скидываем ей время без попадания прицела
                            hitTarget.durationWithoutAimRaycastHit = 0;

                            break; // нужно только первое попадание
                        }

                        // по идее, это невозможно просто по архитектуре
                        if (Application.isEditor) {
                            Debug.LogError($"Can't find local entity by quantum '{hit.Entity}'");
                        }
                    }
                }
            }
        }

        private void SelectTarget() {
            var hasSelectedTarget = HasSelectedTarget();

            if (hasSelectedTarget) {
                var canLostSelectedTarget = CanLostSelectedTarget();

                if (canLostSelectedTarget) {
                    selectedTarget = null;
                }
                else {
                    // ничего делать не нужно, игрок хочет текущую цель
                    return;
                }
            }

            foreach (var entity in filterAimAssistTargets) {
                if (!stashAimAssistTarget.Has(entity)) {
                    continue;
                }

                if (CanSelectTarget(entity)) {
                    // ref var aimAssistTarget = ref stashAimAssistTarget.Get(entity);

                    selectedTarget = entity;

                    // if (Application.isEditor) Debug.Log($"Select aim assist target: {entity}");

                    return;
                }
            }
        }

        [Conditional("UNITY_EDITOR")]
        private void DebugDrawSelectedTarget() {
            if (!gameProperties.Get(DebugGameProperties.Booleans.DebugAimAssist)) {
                if (debugAIMAssistZoneDrawer) {
                    Object.Destroy(debugAIMAssistZoneDrawer.gameObject);
                }

                return;
            }

            debugAIMAssistZoneDrawer ??= Object.Instantiate(inputConfig.debugDrawPrefab);

            if (!debugAIMAssistZoneDrawer) {
                return;
            }

            if (HasSelectedTarget()) {
                ref var selectedAimAssistTarget = ref stashAimAssistTarget.Get(selectedTarget);

                debugAIMAssistZoneDrawer.SetZones(selectedAimAssistTarget.outerRect, selectedAimAssistTarget.innerRect);
            }
            else {
                debugAIMAssistZoneDrawer.SetZones(default, default);
            }
        }

        private void UpdateAimAssistPower(float deltaTime, bool isMoveAimToTarget) {
            ref var selectedAimAssistTarget = ref stashAimAssistTarget.Get(selectedTarget);
            ref var aimAssistPower          = ref selectedAimAssistTarget.aimAssistPower;

            var inOuterZone = selectedAimAssistTarget.outerRect.Contains(screenCenter);

            if (inOuterZone) {
                var inInnerZone = selectedAimAssistTarget.innerRect.Contains(screenCenter);
                if (inInnerZone) {
                    aimAssistPower += deltaTime * inputConfig.aimAssistPowerIncreaseInInnerZone;
                }
                else {
                    aimAssistPower -= deltaTime * inputConfig.aimAssistPowerDecreaseOutInnerZone;
                    var hasAimMove = false == HasNotMoveAim();
                    if (hasAimMove) {
                        if (isMoveAimToTarget) {
                            aimAssistPower += deltaTime * inputConfig.p;
                        }
                        else {
                            aimAssistPower -= deltaTime * inputConfig.m;
                        }
                    }
                }
            }
            else {
                // игрок вышел из внешней (значит, и из внутренней тоже)
                aimAssistPower -= deltaTime * inputConfig.m;
            }

            aimAssistPower = Mathf.Clamp(aimAssistPower, 0, inputConfig.aimAssistPowerMax);
        }

        private unsafe void ApplyAimAssist(float deltaTime, bool isMoveAimToTarget, Frame f, EntityRef localCharacter,
            CinemachineThirdPersonAimQuantum thirdPersonAimQuantum) {
            lastAssistAppliedDeg = 0f;
            lastActiveMode       = "None";

            ref var localUnit       = ref stashUnit.Get(localPlayerSystem.LocalEntity);
            ref var targetUnit      = ref stashUnit.Get(selectedTarget);
            ref var aimAssistTarget = ref stashAimAssistTarget.Get(selectedTarget);

            if (!aimAssistTarget.outerRect.Contains(screenCenter)) {
                return;
            }

            var aimOrigin   = localUnit.FPPositionView;
            var aimRotation = localUnit.FPRotationView;

            if (f.GameModeAiming is ThirdPersonAimingAsset thirdPersonAimingAsset) {
                aimOrigin = thirdPersonAimingAsset.GetAimOrigin(f, localCharacter);
                var aimTarget = thirdPersonAimQuantum.AimTarget.ToFPVector3();

                aimRotation = FPQuaternion.LookRotation(aimTarget - aimOrigin);
            }

            var signedAngleDeg = (FP.Rad2Deg * TransformHelper.AngleSignedRadiansToLookAtTarget(
                aimOrigin,
                aimRotation,
                targetUnit.FPPositionView));

            var normalizedAimPower = FP.FromFloat_UNSAFE(Mathf.Clamp01(aimAssistTarget.aimAssistPower / inputConfig.aimAssistPowerMax));

            if (normalizedAimPower <= FP._0_05) {
                return;
            }

            ref var accumulatedInput = ref inputPollSystem.AccumulatedInput;

            var dt = FP.FromFloat_UNSAFE(deltaTime);

            if (HasNotMoveAim()) {
                if (FPMath.Abs(signedAngleDeg) < inputConfig.deadZoneAngle) {
                    return;
                }

                var inInnerZone = aimAssistTarget.innerRect.Contains(screenCenter);
                if (!inInnerZone) {
                    return;
                }

                var assistSign = FPMath.Sign(signedAngleDeg);
                var stepDeg    = FPMath.Abs(signedAngleDeg) * inputConfig.assistPowerMult * normalizedAimPower * dt;

                var clapedStepDeg = assistSign * FPMath.Clamp(
                    stepDeg,
                    FP._0,
                    inputConfig.maxAssistStepDeg);

                // доводка должна быть не больше чем на угол до цели чтобы избежать "перелета" дальше цели
                clapedStepDeg = FPMath.Clamp(clapedStepDeg, -FPMath.Abs(signedAngleDeg), FPMath.Abs(signedAngleDeg));

                accumulatedInput.LookRotationDelta.X = FP.Deg2Rad * clapedStepDeg;

                lastAssistAppliedDeg = (float)clapedStepDeg;
                lastActiveMode       = "Sticky";
            }

            if (isMoveAimToTarget) {
                if (FPMath.Abs(signedAngleDeg) < inputConfig.deadZoneAngle) {
                    return;
                }

                var assistSign = FPMath.Sign(signedAngleDeg);

                var stepDeg = FPMath.Abs(signedAngleDeg) * inputConfig.assistPowerMultForAimMove * normalizedAimPower * dt;

                var clampedStepDeg = assistSign * FPMath.Clamp(
                    stepDeg,
                    FP._0,
                    inputConfig.maxAssistStepDeg
                );

                // доводка должна быть не больше чем на угол до цели чтобы избежать "перелета" дальше цели
                clampedStepDeg = FPMath.Clamp(clampedStepDeg, -FPMath.Abs(signedAngleDeg), FPMath.Abs(signedAngleDeg));

                accumulatedInput.LookRotationDelta.X += FP.Deg2Rad * clampedStepDeg;

                lastAssistAppliedDeg = (float)clampedStepDeg;
                lastActiveMode       = "Slowdown";
            }
        }

        private bool HasNotMoveAim() {
            var lastLookDelta             = inputPollSystem.LastLookDeltaRad;
            var lastHorizontalRotationDeg = FP.Rad2Deg * FPYawPitchRoll.CreateYaw(lastLookDelta.X).Yaw;

            // пользователь не двигал прицел ниже определенной дельты
            var notMoveAim = FPMath.Abs(lastHorizontalRotationDeg) < inputConfig.horizontalThresholdDeg;

            return notMoveAim;
        }

        /// <summary>
        /// true если последння дельта вращения ведет прицель по направлению к текущей цели
        /// false если уводит от текущей цели или не двигает
        /// </summary>
        /// <returns></returns>
        private bool IsMoveAimToTarget() {
            if (!HasSelectedTarget()) {
                return false;
            }

            ref var localUnit  = ref stashUnit.Get(localPlayerSystem.LocalEntity);
            ref var targetUnit = ref stashUnit.Get(selectedTarget);

            // угол поворота на цель
            var signedAngleDeg = (FP.Rad2Deg * TransformHelper.AngleSignedRadiansToLookAtTarget(
                localUnit.FPPositionView,
                localUnit.FPRotationView,
                targetUnit.FPPositionView));

            // дельта поворота в текущем кадре, свайп игрока по экрану
            var lastLookDelta = inputPollSystem.LastLookDeltaRad;

            // пользователь не двигал прицел
            if (HasNotMoveAim()) {
                return false;
            }

            var clockwiseMove    = lastLookDelta.X > FP._0;
            var clockwiseDesired = signedAngleDeg > FP._0;

            // совпало знак угла и дельты перемещения, значит движение в одну сторону
            var movedTowards = clockwiseMove == clockwiseDesired;

            return movedTowards;
        }

        private bool CanSelectTarget(Entity entity) {
            if (entity.IsNullOrDisposed()) {
                if (Application.isEditor) {
                    Debug.LogError("Check null or disposed entity");
                }

                return false;
            }

            if (!stashAimAssistTarget.Has(entity)) {
                if (Application.isEditor) {
                    Debug.LogError($"Entity '{entity}' not found in {nameof(stashAimAssistTarget)}");
                }

                return false;
            }

            if (entity == selectedTarget) {
                if (Application.isEditor) {
                    Debug.LogError($"Check can select '{entity}' twice!!!");
                }

                return false;
            }

            if (stashLocalCharacterMarker.Has(entity)) {
                if (Application.isEditor) {
                    Debug.LogError($"Entity '{entity}' HAS LOCAL CHARACTER MARKER");
                }

                return false;
            }

            ref var aimAssistTarget = ref stashAimAssistTarget.Get(entity);

            var canSelectTarget =
                // какое-то время подержали прицел во внешней зоне
                aimAssistTarget.durationAimInOuterZone >
                inputConfig.timeToHoldAimInOuterRectForSelectTarget;

            var hasSelectedTarget = HasSelectedTarget();

            if (hasSelectedTarget) {
                var canLostSelectedTarget = CanLostSelectedTarget();

                return canLostSelectedTarget && canSelectTarget;
            }

            return canSelectTarget;
        }

        private bool CanLostSelectedTarget() {
            if (!HasSelectedTarget()) {
                if (Application.isEditor) {
                    Debug.LogError($"HAS NOT SELECTED TARGET, BUT CHECKED CAN LOST");
                }

                return true;
            }

            ref var selectedAimAssistTarget = ref stashAimAssistTarget.Get(selectedTarget);

            var canLostSelectedTarget =
                // меняем выбранную цель если для нее:
                //    - прицел не попадал в OuterZoneRect больше чем timeWithoutCrossOuterZoneRectForLostTarget
                //    ИЛИ
                //    - рейкаст из камеры через центр экрана не попадал в колайдер персонажа больше timeWithoutAimRaycastForLostTarget
                selectedAimAssistTarget.durationWithoutCrossOuterZoneRect >
                inputConfig.timeWithoutCrossOuterZoneRectForLostTarget
                ||
                selectedAimAssistTarget.durationWithoutAimRaycastHit >
                inputConfig.timeWithoutAimRaycastForLostTarget;

            return canLostSelectedTarget;
        }

        private bool HasSelectedTarget() {
            var hasSelectedTarget =
                false == selectedTarget.IsNullOrDisposed() &&
                stashAimAssistTarget.Has(selectedTarget);

            return hasSelectedTarget;
        }

        public bool TryGetAimAssistDebugInfo(out float power, out float normalizedPower, out float durationInOuterZone, out float durationWithoutRaycastHit, out float playerInputDeg,
            out float assistAppliedDeg, out string activeMode) {
            power                     = 0f;
            normalizedPower           = 0f;
            durationInOuterZone       = 0f;
            durationWithoutRaycastHit = 0f;
            playerInputDeg            = 0f;
            assistAppliedDeg          = 0f;
            activeMode                = "None";

            var lastLookDelta             = inputPollSystem.LastLookDeltaRad;
            var lastHorizontalRotationDeg = FP.Rad2Deg * FPYawPitchRoll.CreateYaw(lastLookDelta.X).Yaw;
            playerInputDeg = (float)lastHorizontalRotationDeg;

            assistAppliedDeg = lastAssistAppliedDeg;
            activeMode       = lastActiveMode;

            if (!HasSelectedTarget()) {
                return false;
            }

            ref var aimAssistTarget = ref stashAimAssistTarget.Get(selectedTarget);
            power                     = aimAssistTarget.aimAssistPower;
            normalizedPower           = Mathf.Clamp01(power / inputConfig.aimAssistPowerMax);
            durationInOuterZone       = aimAssistTarget.durationAimInOuterZone;
            durationWithoutRaycastHit = aimAssistTarget.durationWithoutAimRaycastHit;

            return true;
        }
    }
}
