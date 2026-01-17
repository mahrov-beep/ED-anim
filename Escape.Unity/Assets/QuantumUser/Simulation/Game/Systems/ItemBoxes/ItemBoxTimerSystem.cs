namespace Quantum.ItemBoxes {
  public unsafe class ItemBoxTimerSystem : SystemMainThreadFilter<ItemBoxTimerSystem.Filter> {
    public struct Filter {
      public EntityRef Entity;
      public ItemBox*  ItemBox;

      public TimerItemBoxMarker* Only_TimerItemBox;
    }
    
    public override void Update(Frame f, ref Filter filter) {
      var itemBox = filter.ItemBox;
      
      itemBox->TimerToOpen -= f.DeltaTime;

      if (filter.ItemBox->TimerToOpen > 0) {
        return;
      }

      itemBox->TimerToOpen = 0;
      
      f.Remove<TimerItemBoxMarker>(filter.Entity);
      
      f.Set(itemBox->SelfItemBoxEntity, new OpenedItemBoxMarker());
      
      f.Events.OpenItemBox(itemBox->SelfItemBoxEntity);
    }
  }
}