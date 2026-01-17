namespace Quantum {
  using Photon.Deterministic;

  public unsafe class ReconEffectSystem : SystemMainThreadFilter<ReconEffectSystem.Filter> {
    public struct Filter {
      public EntityRef EntityRef;
      public ReconEffect* Effect;
      public Team* OwnerTeam;
    }

    public override void Update(Frame f, ref Filter filter) {
      var effect = filter.Effect;

      Team ownerTeamValue = default;
      var hasOwnerTeam = false;
      if (filter.OwnerTeam != null) {
        ownerTeamValue = *filter.OwnerTeam;
        hasOwnerTeam = true;
      } else if (f.TryGet(effect->OwnerRef, out ownerTeamValue)) {
        hasOwnerTeam = true;
      }

      var unitsFilter = f.Filter<Unit, Transform3D, Team>();
      
      while (unitsFilter.NextUnsafe(out var unitRef, out var _, out var transform, out var team)) {
        if (CharacterFsm.CurrentStateIs<CharacterStateDead>(f, unitRef)) {
          continue;
        }

        if (hasOwnerTeam && team->Equals(ownerTeamValue)) {
          continue;
        }

        if (!effect->IsEntityInRange(transform->Position)) {
          continue;
        }

        EAttributeType.Set_ReconDetected.ChangeAttribute(
          f,
          unitRef,
          EModifierAppliance.Temporary,
          EModifierOperation.Add,
          FP._1,
          effect->Duration
        );
      }

      f.Destroy(filter.EntityRef);
    }
  }
}
