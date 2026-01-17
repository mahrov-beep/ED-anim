using Quantum;
public unsafe class DestroyDeadUnitSystem : SystemMainThread {
  public override void Update(Frame f) {
    var filter = f.Filter<CharacterStateDead, UnitDestroyOnDead>();
    while (filter.NextUnsafe(out var e, out _, out var destroyOnDead)) {
      if (destroyOnDead->Timer.ProcessTimer(f.DeltaTime)) {
        f.Destroy(e);
      }
    }
  }
}