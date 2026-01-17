namespace Quantum.ItemBoxes {
  public unsafe class ItemBoxClearUnitSystem : SystemMainThreadFilter<ItemBoxClearUnitSystem.Filter> {
    public struct Filter {
      public EntityRef         Entity;
      public Unit*             Unit;
      public CharacterLoadout* Loadout;
    }

    public override void Update(Frame f, ref Filter filter) {
      if (f.GameMode.rule is not GameRules.MainMenuStorage) {
        if (filter.Loadout->StorageEntity != filter.Unit->NearbyBackpack && filter.Loadout->StorageEntity != filter.Unit->NearbyItemBox) {
          filter.Loadout->StorageEntity = EntityRef.None;
        }
      }

      filter.Unit->NearbyItemBox  = EntityRef.None;
      filter.Unit->NearbyBackpack = EntityRef.None;
    }
  }
}