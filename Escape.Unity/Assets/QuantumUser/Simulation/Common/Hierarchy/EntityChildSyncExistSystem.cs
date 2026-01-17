namespace Quantum {
  /// <summary>
  /// Система дестроит детей если родительская энтити учничтожена,
  /// эта система должна выполняться первой
  /// </summary>
  public unsafe class EntityChildSyncExistSystem : SystemMainThread {
    public override void Update(Frame f) {
      var iterator = f.Unsafe.GetComponentBlockIterator<ParentEntityLink>();
      foreach (var (child, component) in iterator) {
        if (f.Exists(component->ParentRef)) {
          continue;
        }

        f.Destroy(child);
      }
    }
  }
}