namespace Quantum {
  using System.Collections.Generic;
  
  public partial struct ItemBox {
    public unsafe GameSnapshotStorage LoadToRuntimeStorage(Frame f) {
      var itemRefs = f.ResolveList(this.ItemRefs);
      var items    = new List<GameSnapshotLoadoutItem>();
      
      foreach (var itemRef in itemRefs) {
        if (f.Exists(itemRef)) {
          items.Add(GameSnapshotHelper.MakeItem(f, itemRef));
        }
      }
      
      return new GameSnapshotStorage {
        items = items.ToArray(),
      };
    }
  }
}