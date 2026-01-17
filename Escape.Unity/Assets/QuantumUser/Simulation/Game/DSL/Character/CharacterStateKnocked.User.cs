namespace Quantum {
  using Photon.Deterministic;

  public unsafe partial struct CharacterStateKnocked {
    public bool IsBeingRevived => Rescuer != EntityRef.None;
    public bool HasReviveCandidate => CandidateRescuer != EntityRef.None;

    public FP KnockProgress {
      get {
        if (KnockDuration <= FP._0) {
          return FP._0;
        }

        return KnockTimer / KnockDuration;
      }
    }

    public void TickDamageImmunity(FP deltaTime) {
      if (DamageImmunityTimer <= FP._0) {
        return;
      }

      DamageImmunityTimer = FPMath.Max(FP._0, DamageImmunityTimer - deltaTime);
    }

    public bool HasDamageImmunity => DamageImmunityTimer > FP._0;

    public FP ReviveProgress {
      get {
        if (ReviveDuration <= FP._0) {
          return FP._0;
        }

        return (ReviveDuration - ReviveTimer) / ReviveDuration;
      }
    }

    public void ResetRescue() {
      Rescuer     = EntityRef.None;
      ReviveTimer = ReviveDuration;
      ClearCandidate();
    }

    public void SetCandidate(EntityRef rescuer, FP distanceSqr) {
      CandidateRescuer     = rescuer;
      CandidateDistanceSqr = distanceSqr;
    }

    public void ClearCandidate() {
      CandidateRescuer     = EntityRef.None;
      CandidateDistanceSqr = FP.MaxValue;
    }
  }
}
