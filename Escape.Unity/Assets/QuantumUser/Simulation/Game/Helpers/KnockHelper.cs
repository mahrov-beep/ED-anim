namespace Quantum {
  using Photon.Deterministic;

  public static unsafe class KnockHelper {
    public static KnockSettings ResolveKnockSettings(Frame f) {
      return f.GameMode.GetKnockSettings();
    }

    public static EntityRef ResolveDamageSourceUnitRef(Frame f, EntityRef source) {
      if (source == EntityRef.None) {
        return EntityRef.None;
      }

      if (f.TryGetPointer(source, out Attack* sourceAttack)) {
        return sourceAttack->SourceUnitRef;
      }

      if (f.Has<Unit>(source)) {
        return source;
      }

      return EntityRef.None;
    }

    public static bool HasAliveTeammate(Frame f, EntityRef unitRef) {
      if (!f.TryGet(unitRef, out Team unitTeam)) {
        return false;
      }

      var teamIndex = unitTeam.Index;
      var filter    = f.Filter<Unit, Team, Health>();

      while (filter.NextUnsafe(out EntityRef other, out Unit* _, out Team* team, out Health* health)) {
        if (other == unitRef) {
          continue;
        }

        if (team->Index != teamIndex) {
          continue;
        }

        if (health->CurrentValue <= FP._0) {
          continue;
        }

        if (CharacterFsm.CurrentStateIs<CharacterStateDead>(f, other) || CharacterFsm.CurrentStateIs<CharacterStateKnocked>(f, other)) {
          continue;
        }

        return true;
      }

      return false;
    }

    public static bool TryFindBestRescuer(Frame f,
            EntityRef knockedEntity,
            CharacterStateKnocked* knocked,
            Team* knockedTeam,
            Transform3D* knockedTransform,
            KnockSettings settings,
            out EntityRef bestRescuer,
            out FP bestDistanceSqr) {

      bestRescuer     = EntityRef.None;
      bestDistanceSqr = FP.MaxValue;

      var reviveDistance    = settings.reviveDistance > FP._0 ? settings.reviveDistance : KnockSettings.Default.reviveDistance;
      var reviveDistanceSqr = reviveDistance * reviveDistance;

      var filter = f.Filter<Unit, Team, Health, Transform3D>();
      while (filter.NextUnsafe(out EntityRef candidate, out Unit* _, out Team* candidateTeam, out Health* candidateHealth, out Transform3D* candidateTransform)) {
        if (candidate == knockedEntity) {
          continue;
        }

        if (!IsRescuerValid(f,
                knockedEntity,
                knocked,
                knockedTeam,
                knockedTransform,
                candidate,
                candidateTeam,
                candidateHealth,
                candidateTransform,
                reviveDistanceSqr,
                out var candidateDistanceSqr)) {
          continue;
        }

        if (candidateDistanceSqr >= bestDistanceSqr) {
          continue;
        }

        bestRescuer     = candidate;
        bestDistanceSqr = candidateDistanceSqr;
      }

      return bestRescuer != EntityRef.None;
    }

    public static bool IsRescuerValid(
            Frame f,
            EntityRef knockedEntity,
            CharacterStateKnocked* knocked,
            Team* knockedTeam,
            Transform3D* knockedTransform,
            EntityRef rescuer,
            KnockSettings settings,
            out FP distanceSqr) {

      if (!f.TryGetPointer(rescuer, out Team* rescuerTeam) ||
          !f.TryGetPointer(rescuer, out Health* rescuerHealth) ||
          !f.TryGetPointer(rescuer, out Transform3D* rescuerTransform)) {
        distanceSqr = FP.MaxValue;
        return false;
      }

      var reviveDistance    = settings.reviveDistance > FP._0 ? settings.reviveDistance : KnockSettings.Default.reviveDistance;
      var reviveDistanceSqr = reviveDistance * reviveDistance;

      return IsRescuerValid(f,
              knockedEntity,
              knocked,
              knockedTeam,
              knockedTransform,
              rescuer,
              rescuerTeam,
              rescuerHealth,
              rescuerTransform,
              reviveDistanceSqr,
              out distanceSqr);
    }

    static bool IsRescuerValid(
            Frame f,
            EntityRef knockedEntity,
            CharacterStateKnocked* knocked,
            Team* knockedTeam,
            Transform3D* knockedTransform,
            EntityRef rescuer,
            Team* rescuerTeam,
            Health* rescuerHealth,
            Transform3D* rescuerTransform,
            FP reviveDistanceSqr,
            out FP distanceSqr) {

      distanceSqr = FP.MaxValue;

      if (rescuerHealth == null || rescuerHealth->CurrentValue <= FP._0) {
        return false;
      }

      if (CharacterFsm.CurrentStateIs<CharacterStateDead>(f, rescuer) ||
          CharacterFsm.CurrentStateIs<CharacterStateKnocked>(f, rescuer) ||
          CharacterFsm.CurrentStateIs<CharacterStateHealing>(f, rescuer) ||
          CharacterFsm.CurrentStateIs<CharacterStateKnifeAttack>(f, rescuer)) {
        return false;
      }

      if (rescuerTeam == null || rescuerTeam->Index != knockedTeam->Index) {
        return false;
      }

      if (rescuerTransform == null) {
        return false;
      }

      var delta = rescuerTransform->Position - knockedTransform->Position;
      distanceSqr = delta.SqrMagnitude;
      if (distanceSqr > reviveDistanceSqr) {
        return false;
      }

      if (f.TryGetPointer(rescuer, out CharacterStateReviving* reviving) && reviving->Target != knockedEntity) {
        return false;
      }

      if (knocked->Rescuer != EntityRef.None && knocked->Rescuer != rescuer) {
        return false;
      }

      return true;
    }
  }
}
