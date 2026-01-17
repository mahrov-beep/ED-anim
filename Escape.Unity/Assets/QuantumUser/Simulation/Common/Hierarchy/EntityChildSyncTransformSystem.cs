namespace Quantum {
  /// <summary>
  /// Тут синкаем позиции дочерних энтити
  /// </summary>
  public unsafe class EntityChildSyncTransform3DSystem : SystemMainThread {
    public override void Update(Frame f) {
      var iterator = f.Unsafe.GetComponentBlockIterator<ParentEntityLink>();
      foreach (var (child, component) in iterator) {
        var parentTransform = f.GetPointer<Transform3D>(component->ParentRef);
        var childTransform  = f.GetPointer<Transform3D>(child);

        if (component->SyncPosition) {
          childTransform->Position = parentTransform->Position;
        }

        if (component->SyncRotation) {
          childTransform->Rotation = parentTransform->Rotation;
        }
      }
    }
  }
}