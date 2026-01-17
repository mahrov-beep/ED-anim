namespace Quantum {
  using Collections;
  using Photon.Deterministic;

  public unsafe class InteractiveZoneSystem : SystemMainThreadFilter<InteractiveZoneSystem.Filter>, ISignalOnTriggerExit3D {
    public struct Filter {
      public EntityRef Entity;

      public InteractiveZone* Zone;
      public TriggerArea*     TriggerArea;
    }

    public override void Update(Frame f, ref Filter filter) {
      EntityRef zoneEntity = filter.Entity;

      var zone        = filter.Zone;
      var triggerArea = filter.TriggerArea;
      var zoneAsset   = f.FindAsset(zone->Asset);

      var ignoredEntities = f.ResolveHashSet(zone->IgnoredEntities);
      var entitiesInside  = f.ResolveHashSet(triggerArea->EntitiesInside);

      if (f.Global->GameState != EGameStates.Game) {
        foreach (var entity in entitiesInside) {
          if (f.Unsafe.TryGetPointer<Unit>(entity, out var unit)) {
            ResetExitTimer(unit);
          }
        }
        return;
      }

      foreach (var entity in entitiesInside) {
        if (!f.Unsafe.TryGetPointer<Unit>(entity, out var unit)) {
          continue;
        }

        if (CharacterFsm.CurrentStateIs<CharacterStateDead>(f, entity) ||
            f.Has<UnitExited>(entity)) {
          ResetExitTimer(unit);
          continue;
        }

        if (!f.TryGetPointer(entity, out Health* health) || health->IsDead) {
          ResetExitTimer(unit);
          continue;
        }

        if (!zoneAsset.CanInteract(f, zoneEntity, entity)) {
          ResetExitTimer(unit);
          continue;
        }

        if (ignoredEntities.Contains(entity)) {
          continue;
        }

        if (!unit->ExitZoneTimer.IsStarted) {
          unit->ExitZoneTimer.Start(zoneAsset.interactionTime);
        }

        unit->ExitZoneTimer.Tick(f.DeltaTime);

        if (f.IsVerified && unit->ExitZoneTimer.IsDone) {
          ignoredEntities.Add(entity);

          zoneAsset.OnInteractComplete(f, zoneEntity, entity);
          f.Events.OnInteractComplete(zoneAsset, zoneEntity, entity);
        }
      }
    }

    public void OnTriggerExit3D(Frame f, ExitInfo3D info) {
      if (!f.TryGetPointer(info.Entity, out InteractiveZone* zone)) {
        return;
      }

      if (f.Unsafe.TryGetPointer<Unit>(info.Other, out var unit)) {
        ResetExitTimer(unit);
      }

      var ignoredEntities = f.ResolveHashSet(zone->IgnoredEntities);
      ignoredEntities.Remove(info.Other);
    }

    static void ResetExitTimer(Unit* unit) {
      unit->ExitZoneTimer.Reset();
    }
  }
}
