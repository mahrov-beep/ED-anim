namespace Quantum {
  using System;

  // ReSharper disable once InconsistentNaming - это нейминг от фотона
  public unsafe partial struct _globals_ {
    public EntityRef CreatePlayerCharacter(Frame f, GameSnapshotLoadout loadoutSnapshot, out SpawnPoint spawnPoint) {
      var characterRef = f.Create(TryGetCharacterPrototype(f, loadoutSnapshot, out var characterPrototype)
              ? characterPrototype
              : throw new InvalidOperationException("Cannot create character without skin"));

      var unit = f.GetPointer<Unit>(characterRef);
      unit->ActiveWeaponRef = unit->ValidWeaponRef;

      SetSpawnPoint(f, characterRef, out spawnPoint);
      unit->TargetExitZone = spawnPoint.exitZoneRef;

      f.Add(characterRef, new CharacterLoadout(), out var characterLoadout);
      characterLoadout->SelfUnitEntity = characterRef;
      characterLoadout->CreateItemsFromRuntimeLoadout(f, loadoutSnapshot);

      f.Add(characterRef, new LagCompensationTarget());

      var unitConfig = f.FindAsset(unit->Asset);
      if (unitConfig.IsSprintEnabled) {
        f.Set(characterRef, new UnitFeatureSprintWithStamina {
          current = unitConfig.sprintSettings.maxStamina,
        });
      }

      f.Add(characterRef, new CharacterFsm {
        SelfEntity = characterRef,
      }, out var characterFsm);
      characterFsm->TryEnterState(f, new CharacterStateIdle());

      f.Signals.OnUnitSpawn(characterRef);

      return characterRef;
    }

    bool TryGetCharacterPrototype(Frame f, GameSnapshotLoadout loadoutSnapshot, out AssetRef<EntityPrototype> characterPrototype) {
      var skinSnapshot = loadoutSnapshot?.SlotItems?[CharacterLoadoutSlots.Skin.ToInt()];

      if (skinSnapshot == null) {
        characterPrototype = default;
        return false;
      }

      var skinAsset = ItemAssetCreationData.FindAssetByItemKey(f, skinSnapshot.ItemKey);

      if (skinAsset is not SkinItemAsset skinItemAsset) {
        characterPrototype = default;
        return false;
      }

      characterPrototype = skinItemAsset.characterPrototype;
      return true;
    }

    void SetSpawnPoint(Frame f, EntityRef unitRef, out SpawnPoint spawnPoint) {
      var initializeData = f.Unsafe.GetOrAddSingletonPointer<InitializeData>();
      var spawnPointRef  = initializeData->NotUsedPlayerSpawnPoints.Pop(f);

      spawnPoint = f.Get<SpawnPoint>(spawnPointRef);

      TransformHelper.CopyPositionAndRotation(f, spawnPointRef, unitRef);

      f.LogTrace(unitRef, $"Spawned on {spawnPointRef}");
    }
  }
}