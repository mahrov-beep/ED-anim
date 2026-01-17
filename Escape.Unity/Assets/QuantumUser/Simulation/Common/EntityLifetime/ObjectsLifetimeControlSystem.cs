namespace Quantum {
  public unsafe class ObjectsLifetimeControlSystem : SystemMainThread {
    public struct Filter {
      public EntityRef Entity;

      public ObjectLifetime* Lifetime;
    }

    public override void Update(Frame f) {
      var filter = f.FilterStruct(out Filter filterTTL);

      while (filter.Next(&filterTTL)) {
        filterTTL.Lifetime->TTL.Tick(f.DeltaTime);

        if (filterTTL.Lifetime->TTL.IsDone) {
          f.Destroy(filterTTL.Entity);
        }
      }
    }
  }
}