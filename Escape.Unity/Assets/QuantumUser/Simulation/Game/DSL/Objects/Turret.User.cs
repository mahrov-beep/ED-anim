namespace Quantum {
  public unsafe partial struct Turret {
    public static ComponentHandler<Turret> InitTriggerArea => static (f, e, c) => {
      c->TriggerAreaEntity = f.Create(c->TriggerAreaPrototype);
      f.Set(c->TriggerAreaEntity, new ParentEntityLink {
              ParentRef = e, SyncPosition = true,
      });
      
      if (f.TryGetPointer<PhysicsCollider3D>(e, out var turretCollider)) {
        if (f.TryGetPointer<PhysicsCollider3D>(c->TriggerAreaEntity, out var areaCollider)) {
          areaCollider->Layer = turretCollider->Layer;
        }
      }
    };
  }
}