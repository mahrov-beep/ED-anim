namespace Quantum {
  unsafe class InVisibilityUnsetSystem : SystemMainThread {
    public override void Update(Frame f) {

      var unitFilter = f.Filter<Vision>();
      while (unitFilter.NextUnsafe(out _, out var vision)) {
        vision->InvisibilityZone = EntityRef.None;
      }
    }
  }
}