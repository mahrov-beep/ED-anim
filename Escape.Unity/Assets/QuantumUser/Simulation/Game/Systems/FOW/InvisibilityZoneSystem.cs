namespace Quantum {

public unsafe class InvisibilityZoneSystem : SystemMainThread {
    public override void Update(Frame f) {
      var zones = f.Filter<InvisibilityZone, TriggerArea>();
      while (zones.NextUnsafe(out var zoneEntity, out var zone, out var triggerArea)) {
        if (zone->delayBeforeActivation > 0) {
          zone->delayBeforeActivation -= f.DeltaTime;
          continue;
        }

        var entitiesInZone = f.ResolveHashSet(triggerArea->EntitiesInside);
        foreach (var entityInZone in entitiesInZone) {
          if (!f.Unsafe.TryGetPointer(entityInZone, out Vision* vision)) {
            continue;
          }

          vision->InvisibilityZone = zoneEntity;
        }
      }
    }
  }
}