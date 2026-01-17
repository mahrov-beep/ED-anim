namespace Quantum {
  public unsafe class CharacterLoadoutModifiedEventSystem : SystemMainThreadFilter<CharacterLoadoutModifiedEventSystem.Filter> {
    public struct Filter {
      public EntityRef Entity;

      public Unit*                           Unit;
      public CharacterLoadout*               Loadout;
      public CharacterLoadoutModifiedMarker* Marker;
    }

    public override void OnInit(Frame f) {
      base.OnInit(f);

      f.Global->CharacterLoadoutModificationEventEnabled = true;
    }

    // за один кадр может быть отправлено несколько событий,
    // например, когда перекладываем предмет между слотами,
    // поэтому откладываем отправку на один кадр и
    // вместо множества событий отправляем только одно за кадр
    public override void Update(Frame f, ref Filter filter) {
      var entity  = filter.Entity;
      var unit    = filter.Unit;
      var loadout = filter.Loadout;
      var marker  = filter.Marker;

      // не вызываем событие изменения при начальном создании лодаута
      if (marker->ModificationFrame != loadout->CreationFrame) {
        f.Events.LocalCharacterLoadoutModified(f, unit->PlayerRef, entity, marker->ModifiedItems);
      }

      f.Remove<CharacterLoadoutModifiedMarker>(entity);
    }
  }
}