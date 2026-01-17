namespace Quantum {
  using Photon.Deterministic;

  public static unsafe class KnifeHelper {
    static readonly FP DefaultTargetYOffset = FP._1;

    public static bool TryFindTarget(
            Frame f,
            EntityRef attacker,
            Team* attackerTeam,
            Transform3D* attackerTransform,
            UnitAim* attackerAim,
            FP distanceOverride,
            FP angleDegreesOverride,
            out EntityRef bestTarget,
            out FP bestDistanceSqr,
            out FPVector3 aimPoint) {

      bestTarget      = EntityRef.None;
      bestDistanceSqr = FP.MaxValue;
      aimPoint        = default;

      if (attackerTransform == null || attackerTeam == null) {
        return false;
      }

      var attackDistance = ResolveDistance(distanceOverride);
      if (attackDistance <= FP._0) {
        return false;
      }

      var attackAngleDeg = ResolveAttackAngle(angleDegreesOverride);
      bool hasAngleLimit = attackAngleDeg > FP._0 && attackAngleDeg < FP._360;
      var halfAngle      = hasAngleLimit ? attackAngleDeg * FP._0_50 : FP._180;

      var aimOrigin  = f.GameModeAiming.GetAimOrigin(f, attacker);
      var forwardDir = ResolveForward(attackerAim, attackerTransform);

      if (!TryNormalize(ref forwardDir)) {
        return false;
      }

      return TryFindTargetFallback(
              f,
              attacker,
              attackerTeam,
              aimOrigin,
              forwardDir,
              attackDistance,
              halfAngle,
              out bestTarget,
              out bestDistanceSqr,
              out aimPoint);
    }

    static FP ResolveDistance(FP distanceOverride) {
      return distanceOverride > FP._0 ? distanceOverride : KnifeSettings.Default.Distance;
    }

    static FP ResolveAttackAngle(FP angleOverride) {
      var attackAngle = angleOverride > FP._0 ? angleOverride : KnifeSettings.Default.AttackAngleDegrees;
      return FPMath.Clamp(attackAngle, FP._0, FP._360);
    }

    static FPVector3 ResolveForward(UnitAim* attackerAim, Transform3D* attackerTransform) {
      if (attackerAim != null) {
        return FPQuaternionHelper.CreateFromYawPitchRoll(attackerAim->AimCurrentRotation) * FPVector3.Forward;
      }

      return attackerTransform->Rotation * FPVector3.Forward;
    }

    static bool TryNormalize(ref FPVector3 direction) {
      var magnitudeSqr = direction.SqrMagnitude;
      if (magnitudeSqr <= FP.Epsilon) {
        return false;
      }

      var magnitude = FPMath.Sqrt(magnitudeSqr);
      if (magnitude <= FP.Epsilon) {
        return false;
      }

      direction /= magnitude;
      return true;
    }

    static bool IsWithinAngles(FPVector3 forwardDir, FPVector3 enemyVector, FP halfAngle) {
      if (enemyVector.SqrMagnitude <= FP.Epsilon) {
        return false;
      }

      if (FPVector3.Dot(enemyVector, forwardDir) <= FP._0) {
        return false;
      }

      var verticalPlaneNormal   = FPVector3.Cross(FPVector3.Up, forwardDir);
      var horizontalPlaneNormal = FPVector3.Cross(verticalPlaneNormal, forwardDir);

      var onVertical   = FPVector3.ProjectOnPlane(enemyVector, verticalPlaneNormal);
      var onHorizontal = FPVector3.ProjectOnPlane(enemyVector, horizontalPlaneNormal);

      var verticalAngle   = FPVector3.Angle(forwardDir, onVertical);
      var horizontalAngle = FPVector3.Angle(forwardDir, onHorizontal);

      return verticalAngle < halfAngle && horizontalAngle < halfAngle;
    }

    static bool TryFindTargetFallback(
            Frame f,
            EntityRef attacker,
            Team* attackerTeam,
            FPVector3 origin,
            FPVector3 forwardDir,
            FP attackDistance,
            FP halfAngle,
            out EntityRef bestTarget,
            out FP bestDistanceSqr,
            out FPVector3 aimPoint) {

      bestTarget      = EntityRef.None;
      bestDistanceSqr = FP.MaxValue;
      aimPoint        = FPVector3.Zero;

      var attackDistanceSqr = attackDistance * attackDistance;

      var filter = f.Filter<Unit, Team, Health, Transform3D>();
      while (filter.NextUnsafe(out EntityRef candidate, out Unit* _, out Team* candidateTeam, out Health* candidateHealth, out Transform3D* candidateTransform)) {
        if (candidate == attacker) {
          continue;
        }

        if (attackerTeam != null && candidateTeam != null && attackerTeam->Index == candidateTeam->Index) {
          continue;
        }

        if (candidateHealth == null) {
          continue;
        }

        bool isKnocked = CharacterFsm.CurrentStateIs<CharacterStateKnocked>(f, candidate);
        if (candidateHealth->CurrentValue <= FP._0 && !isKnocked) {
          continue;
        }

        if (CharacterFsm.CurrentStateIs<CharacterStateDead>(f, candidate) || f.Has<UnitExited>(candidate)) {
          continue;
        }

        var targetPoint = GetTargetPoint(f, candidate, candidateTransform);
        var toTarget    = targetPoint - origin;
        var distanceSqr = toTarget.SqrMagnitude;

        if (distanceSqr > attackDistanceSqr) {
          continue;
        }

        bool angleOk = IsWithinAngles(forwardDir, toTarget, halfAngle);

        if (!angleOk) {
          continue;
        }

        if (!LineOfSightHelper.HasLineSight(f, origin, targetPoint)) {
          continue;
        }

        if (distanceSqr < bestDistanceSqr) {
          bestDistanceSqr = distanceSqr;
          bestTarget      = candidate;
          aimPoint        = targetPoint;
        }
      }

      return bestTarget != EntityRef.None;
    }

    public static FPVector3 GetTargetPoint(
            Frame f,
            EntityRef target,
            Transform3D* targetTransform) {

      FP targetHeight = UnitColliderHeightHelper.GetCurrentHeight(f, target);
      FP aimOffset    = targetHeight > FP._0 ? targetHeight * FP._0_50 : DefaultTargetYOffset;

      if (aimOffset <= FP._0) {
        aimOffset = DefaultTargetYOffset;
      }

      return targetTransform->Position + FPVector3.Up * aimOffset;
    }
  }

  public static unsafe class KnifeAttackHelper {
    public static KnifeSettings ResolveSettings(Frame f, Unit* unit) {
      if (unit != null && unit->Asset.IsValid && f.FindAsset(unit->Asset) is UnitAsset asset) {
        return asset.GetKnifeSettings();
      }

      return KnifeSettings.Default;
    }

    public static bool ExecuteAttack(Frame f, EntityRef characterEntity, KnifeSettings knifeSettings) {
      if (!f.TryGetPointer(characterEntity, out Unit* unit) ||
          !f.TryGetPointer(characterEntity, out Team* team) ||
          !f.TryGetPointer(characterEntity, out Transform3D* transform)) {
        return false;
      }

      var attackRange = knifeSettings.Distance > FP._0 ? knifeSettings.Distance : KnifeSettings.Default.Distance;

      UnitAim* unitAim = null;
      f.TryGetPointer(characterEntity, out unitAim);

      bool hasTarget = KnifeHelper.TryFindTarget(
              f,
              characterEntity,
              team,
              transform,
              unitAim,
              attackRange,
              knifeSettings.AttackAngleDegrees,
              out var target,
              out _,
              out var aimPoint);

      FP attackDamage = knifeSettings.Damage > FP._0 ? knifeSettings.Damage : KnifeSettings.Default.Damage;

      if (!hasTarget) {
        return false;
      }

      bool attackApplied = TryDealDamageWithAttackEntity(
              f,
              characterEntity,
              transform,
              attackRange,
              attackDamage,
              target,
              aimPoint,
              knifeSettings.AttackData);

      return attackApplied;
    }

    static bool TryDealDamageWithAttackEntity(
            Frame f,
            EntityRef attacker,
            Transform3D* attackerTransform,
            FP attackRange,
            FP damage,
            EntityRef target,
            FPVector3 aimPoint,
            AssetRef<WeaponBasicAttackData> knifeAttackData) {
      var attackOrigin   = f.GameModeAiming.GetAimOrigin(f, attacker);
      var direction      = aimPoint - attackOrigin;
      var directionSqr   = direction.SqrMagnitude;
      var fallbackDir    = attackerTransform != null ? attackerTransform->Rotation * FPVector3.Forward : FPVector3.Forward;
      var applicator     = HealthApplicator.CreateDamage(damage, EDamageType.Melee);

      if (directionSqr <= FP.Epsilon && fallbackDir.SqrMagnitude > FP.Epsilon) {
        direction    = fallbackDir;
        directionSqr = direction.SqrMagnitude;
      }

      if (directionSqr <= FP.Epsilon) {
        direction = FPVector3.Forward;
      }

      var magnitude = FPMath.Sqrt(directionSqr);
      var normalizedDir = magnitude > FP.Epsilon ? direction / magnitude : FPVector3.Forward;

      if (!knifeAttackData.IsValid) {
        applicator.ApplyOn(f, attacker, target);
        return true;
      }

      var attackRef      = f.Create();
      var attackRotation = FPQuaternion.LookRotation(normalizedDir, FPVector3.Up);

      f.Set(attackRef, Transform3D.Create(attackOrigin, attackRotation));

      f.Set(attackRef, new Attack {
        MaxDistance               = attackRange,
        AttackData                = new AssetRef<AttackData> { Id = knifeAttackData.Id },
        HealthApplicator          = applicator,
        SourceUnitRef             = attacker,
        DistanceDamageMultiplier  = FPBoostedMultiplier.One,
        ProjectileSpeedMultiplier = FPBoostedMultiplier.One,
      });

      return true;
    }
  }
}


