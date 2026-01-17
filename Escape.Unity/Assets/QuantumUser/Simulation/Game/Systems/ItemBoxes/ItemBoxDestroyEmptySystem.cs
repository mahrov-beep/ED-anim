namespace Quantum.ItemBoxes {
  public unsafe class ItemBoxDestroyEmptySystem : SystemMainThreadFilter<ItemBoxDestroyEmptySystem.Filter> {
    public struct Filter {
      public EntityRef EntityRef;
      public ItemBox*  ItemBox;
    }

    public override ComponentSet Without { get; } = ComponentSet.Create<ItemBoxKeepAliveWithoutItems>();

    public override void Update(Frame f, ref Filter filter) {
      if (f.IsPredicted) {
        return;
      }
      
      var items = f.ResolveList(filter.ItemBox->ItemRefs);

      if (items.Count > 0) {
        return;
      }

      f.Destroy(filter.EntityRef);
    }
  }
}