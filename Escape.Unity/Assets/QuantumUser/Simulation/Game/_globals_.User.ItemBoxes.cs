namespace Quantum {
  using Photon.Deterministic;

  // ReSharper disable once InconsistentNaming - это нейминг от фотона
  public unsafe partial struct _globals_ {
    public EntityRef CreateItemBox(Frame f, FPVector3 position,
      AssetRef<EntityPrototype> customItemBoxPrototype = default,
      bool autoUnpackNestedItems = true, 
      bool keelAliveWithoutItems = false,
      bool isThrowAwayFeatureLocked = false,
      int tetrisWidth = 0, int tetrisHeight = 0) {
      var itemBoxEntity = f.Create(customItemBoxPrototype.IsValid
        ? customItemBoxPrototype
        : f.GameMode.ItemBoxPrototype);

      var transform3d = f.Unsafe.GetPointer<Transform3D>(itemBoxEntity);
      transform3d->Position = position;
      // transform3d->Rotation = FPQuaternion.RadianAxis(f.RNG->Next(0, FP.PiTimes2), FPVector3.Up);

      var itemBox = f.Unsafe.GetPointer<ItemBox>(itemBoxEntity);
      itemBox->SelfItemBoxEntity        = itemBoxEntity;
      itemBox->AutoUnpackNestedItems    = autoUnpackNestedItems;
      itemBox->IsThrowAwayFeatureLocked = isThrowAwayFeatureLocked;
      
      // Если переданы размеры тетрис-сетки, устанавливаем их
      if (tetrisWidth > 0 && tetrisHeight > 0) {
        itemBox->Width  = tetrisWidth;
        itemBox->Height = tetrisHeight;
      }

      if (keelAliveWithoutItems) {
        f.Set(itemBoxEntity, new ItemBoxKeepAliveWithoutItems());
      }

      return itemBoxEntity;
    }
  }
}