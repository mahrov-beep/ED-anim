namespace Quantum {
  using System;
  using Photon.Deterministic;

  public unsafe partial struct TriggerArea {
    public bool HasEntityInside(Frame f, EntityRef entity) {
      var entitiesSet = f.ResolveHashSet(EntitiesInside);
      return entitiesSet.Contains(entity);
    }

    public EntityRef FindClosestEntityInside(Frame f) {
      return SearchClosestEntityInside<object>(f, null, null);
    }

    public EntityRef SearchClosestEntityInside<TSTate>(Frame f, Func<Frame, SearchData, TSTate, bool> filter, TSTate state) {
      var entitiesSet     = f.ResolveHashSet(EntitiesInside);
      var closestEntity   = EntityRef.None;
      var closestDistance = FP.MaxValue;

      foreach (var otherEntity in entitiesSet) {
        var transformZone = f.GetPointer<Transform3D>(SelfEntity);
        var transformUnit = f.GetPointer<Transform3D>(otherEntity);

        var distance = TransformHelper.DistanceSquared(transformUnit, transformZone);
        if (distance >= closestDistance) {
          continue;
        }

        if (filter != null && !filter(f, new SearchData { ZoneEntity = SelfEntity, OtherEntity = otherEntity }, state)) {
          continue;
        }

        closestDistance = distance;
        closestEntity   = otherEntity;
      }

      return closestEntity;
    }

    public EntityRef SearchAnyEntityInside(Frame f) {
      return SearchAnyEntityInside<object>(f, null, null);
    }

    public EntityRef SearchAnyEntityInside<TState>(Frame f, Func<Frame, SearchData, TState, bool> filter, TState state) {
      var entitiesSet = f.ResolveHashSet(EntitiesInside);

      foreach (var otherEntity in entitiesSet) {
        if (f.Exists(otherEntity) == false) {
          continue;
        }

        if (filter != null && !filter.Invoke(f, new SearchData { ZoneEntity = SelfEntity, OtherEntity = otherEntity }, state)) {
          continue;
        }

        return otherEntity;
      }

      return EntityRef.None;
    }

    public struct SearchData {
      public EntityRef ZoneEntity;
      public EntityRef OtherEntity;
    }
  }
}