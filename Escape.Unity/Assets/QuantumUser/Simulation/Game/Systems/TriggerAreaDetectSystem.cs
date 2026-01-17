namespace Quantum {
  using UnityEngine;
  using UnityEngine.Scripting;

  [Preserve]
  public unsafe class TriggerAreaDetectSystem : SystemMainThread, ISignalOnTriggerEnter3D, ISignalOnTriggerExit3D,
    ISignalOnComponentAdded<TriggerArea> {
    public void OnAdded(Frame f, EntityRef entity, TriggerArea* triggerArea) {
      triggerArea->SelfEntity = entity;
    }

    public void OnTriggerEnter3D(Frame f, TriggerInfo3D info) {
      if (!f.Unsafe.TryGetPointer(info.Entity, out TriggerArea* triggerArea)) {
        return;
      }

      var entitiesSet = f.ResolveHashSet(triggerArea->EntitiesInside);

      if (entitiesSet.Count == 0) {
        f.Add<TriggerAreaWithEntities>(info.Entity);
      }

      entitiesSet.Add(info.Other);
    }

    public void OnTriggerExit3D(Frame f, ExitInfo3D info) {
      if (!f.Unsafe.TryGetPointer(info.Entity, out TriggerArea* triggerArea)) {
        return;
      }

      var entitiesSet = f.ResolveHashSet(triggerArea->EntitiesInside);
      entitiesSet.Remove(info.Other);

      if (entitiesSet.Count == 0) {
        f.Remove<TriggerAreaWithEntities>(info.Entity);
      }
    }

    public override void Update(Frame f) {
      foreach (var (entity, _) in f.GetComponentIterator<TriggerAreaWithEntities>()) {
        var area = f.Unsafe.GetPointer<TriggerArea>(entity);

        var entitiesInside = f.ResolveHashSet(area->EntitiesInside);

        entitiesInside.RemoveAll(f, static (f, it) => !f.Exists(it));
      }
    }
  }
}