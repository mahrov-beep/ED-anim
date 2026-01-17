namespace Quantum.ItemBoxes {
  public unsafe class ItemBoxNearbySystem : SystemMainThreadFilter<ItemBoxNearbySystem.Filter> {
    public struct Filter {
      public EntityRef    Entity;
      public ItemBox*     ItemBox;
      public TriggerArea* TriggerArea;

      public TriggerAreaWithEntities* TriggerAreaWithEntities; // skip areas without entities for performance
    }

    public override void Update(Frame f, ref Filter filter) {
      var nearbyEntities = f.ResolveHashSet(filter.TriggerArea->EntitiesInside);

      if (f.Has<TimerItemBoxMarker>(filter.Entity)) {
        return;
      }
      
      foreach (var e in nearbyEntities) {
        if (!f.Unsafe.TryGetPointer<Unit>(e, out var nearbyUnit)) {
          continue;
        }

        if (CharacterFsm.CurrentStateIs<CharacterStateDead>(f, e) || f.Has<UnitExited>(e)) {
          continue;
        }

        var itemBox = f.Get<ItemBox>(filter.Entity);

        if (itemBox.IsBackpack) {
          nearbyUnit->NearbyBackpack = filter.Entity;
        }
        else {
          nearbyUnit->NearbyItemBox = filter.Entity;
        }
      }
    }
  }
}