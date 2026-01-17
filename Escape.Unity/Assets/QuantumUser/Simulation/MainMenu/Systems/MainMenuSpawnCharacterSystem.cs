namespace Quantum {
  public unsafe class MainMenuSpawnCharacterSystem : SystemSignalsOnly, ISignalOnPlayerAdded {
    public void OnPlayerAdded(Frame f, PlayerRef player, bool firstTime) {
      var runtimePlayerData = f.GetPlayerData(player);

      var playerCharacter = f.Global->CreatePlayerCharacter(f,
              loadoutSnapshot: runtimePlayerData.Loadout, out _);

      f.GetPointer<Unit>(playerCharacter)->PlayerRef = player;

      var itemBoxEntity = f.Global->CreateItemBox(f, f.Get<Transform3D>(playerCharacter).Position,
              autoUnpackNestedItems: false,
              keelAliveWithoutItems: true
      );

      var itemBox = f.GetPointer<ItemBox>(itemBoxEntity);
      itemBox->Width  = runtimePlayerData.StorageWidth;
      itemBox->Height = runtimePlayerData.StorageHeight;
      
      itemBox->CreateFromRuntimeStorage(f, runtimePlayerData.Storage);
      
      var itemRefs = f.ResolveList(itemBox->ItemRefs);
      Log.Info($"MainMenuSpawnCharacterSystem: Storage ItemBox created with {itemBox->Width}x{itemBox->Height} grid and {itemRefs.Count} items");

      itemBox->OpenerUnitRef = playerCharacter;
      
      var unit = f.GetPointer<Unit>(playerCharacter);
      unit->NearbyItemBox = itemBoxEntity;
      
      if (f.Unsafe.TryGetPointer<CharacterLoadout>(playerCharacter, out var loadout)) {
        loadout->LoadStorageItems(f, itemBoxEntity);
        Log.Info($"MainMenuSpawnCharacterSystem: Loaded {loadout->GetStorageItems(f).Count} items from ItemBox to StorageItems");
      }
    }
  }
}