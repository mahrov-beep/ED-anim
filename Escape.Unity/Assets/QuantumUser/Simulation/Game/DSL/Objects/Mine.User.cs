namespace Quantum {
  public unsafe partial struct Mine {  
    public static ComponentHandler<Mine> InitTriggerArea => static (f, e, c) => {
      if (c->TriggerAreaPrototype.IsValid) {        
        c->TriggerAreaEntity = f.Create(c->TriggerAreaPrototype);
              
        f.Set(c->TriggerAreaEntity, new ParentEntityLink {
          ParentRef = e,
          SyncPosition = true,
        });        
      
        int layer = 0;
        if (f.TryGetPointer<PhysicsCollider3D>(e, out var mineCollider)) {
          layer = mineCollider->Layer;
          if (f.TryGetPointer<PhysicsCollider3D>(c->TriggerAreaEntity, out var areaCollider)) {
            areaCollider->Layer = layer;
          }
        }
      } 
    };
  }
}

