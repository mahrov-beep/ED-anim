namespace Quantum {
  public unsafe class InteractiveZoneUnitNearbySystem : SystemMainThreadFilter<InteractiveZoneUnitNearbySystem.Filter> {
    public struct Filter {
      public EntityRef Entity;

      public TriggerArea* TriggerArea;
    }

    public override void Update(Frame f, ref Filter filter) {
      var newNearbyEntities = f.ResolveHashSet(filter.TriggerArea->EntitiesInside);

      foreach (var newNearbyEntity in newNearbyEntities) {
        if (!f.Unsafe.TryGetPointer<Unit>(newNearbyEntity, out var nearbyUnit)) {
          continue;
        }

        if (CharacterFsm.CurrentStateIs<CharacterStateDead>(f, newNearbyEntity) || f.Has<UnitExited>(newNearbyEntity)) {
          continue;
        }

        nearbyUnit->NearbyInteractiveZone = filter.Entity;
      }
    }
  }
}