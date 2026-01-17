namespace Quantum.ItemBoxes {
  public unsafe class ItemBoxCloseSystem : SystemMainThread {
    public struct Filter {
      public EntityRef    Entity;
      public ItemBox*     ItemBox;
      public TriggerArea* TriggerArea;

      public OpenedItemBoxMarker* Only_OpenedItemBoxMarker;
    }

    public override void Update(Frame f) {
      var itemBoxes = f.FilterStruct(out Filter filter);

      while (itemBoxes.Next(&filter)) {
        var itemBox   = filter.ItemBox;
        var openerRef = itemBox->OpenerUnitRef;
        if (openerRef == EntityRef.None) {
          continue;
        }

        var nearbyEntities = f.ResolveHashSet(filter.TriggerArea->EntitiesInside);
        if (nearbyEntities.Contains(openerRef)) {
          continue;
        }

        itemBox->OpenerUnitRef = EntityRef.None;

        // захотели чтобы сундук оставался открытым если его хотя бы раз открыли
        /*var itemBoxRef = filter.Entity;

        f.Remove<OpenedItemBoxMarker>(itemBoxRef);
        
        if (itemBox->ItemRefs.IsEmpty(f)) {
          return;
        }
        
        f.Events.CloseItemBox(itemBoxRef);*/
      }
    }
  }
}