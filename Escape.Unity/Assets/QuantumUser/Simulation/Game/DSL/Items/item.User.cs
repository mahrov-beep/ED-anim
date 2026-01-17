namespace Quantum {
  using System;
  using Photon.Deterministic;

  public unsafe partial struct Item {
    // сколько раз этот предмет уже использовали
    public static int GetUsedUsages(Frame f, EntityRef itemEntity) {
      if (itemEntity == EntityRef.None) {
        return 0;
      }

      var item      = f.GetPointer<Item>(itemEntity);
      var itemAsset = f.FindAsset(item->Asset);

      return Math.Clamp(item->Used, 0, itemAsset.MaxUsages);
    }

    // сколько раз этот предмет еще можно использовать
    public static int GetRemainingUsages(Frame f, EntityRef itemEntity) {
      if (itemEntity == EntityRef.None) {
        return 0;
      }
      
      var item      = f.GetPointer<Item>(itemEntity);
      var itemAsset = f.FindAsset(item->Asset);

      return GetRemainingUsages(item->Used, itemAsset.MaxUsages);
    }
    
    public static int GetRemainingUsages(int used, int maxUsages) {
      return Math.Clamp(maxUsages - used, 0, maxUsages);
    }

    public static void DeUseItem(Frame f, EntityRef itemEntity, int amount = 1) {
      var item      = f.GetPointer<Item>(itemEntity);
      var itemAsset = f.FindAsset(item->Asset);

      item->Used = (ushort)(Item.GetUsedUsages(f, itemEntity) - amount);

      if (item->Used < itemAsset.MaxUsages) {
        f.Remove<ItemOutOfUses>(itemEntity);
      }
    }
    
    public static void UseItem(Frame f, EntityRef itemEntity, int amount) {
      var item      = f.GetPointer<Item>(itemEntity);
      var itemAsset = f.FindAsset(item->Asset);

      item->Used = (ushort)(Item.GetUsedUsages(f, itemEntity) + amount);

      if (item->Used >= itemAsset.MaxUsages) {
        f.Add(itemEntity, new ItemOutOfUses());
      }
    }

    public static FP GetItemWeight(Frame f, EntityRef itemEntity) {
      if (itemEntity == EntityRef.None) {
        return FP._0;
      }

      var item      = f.GetPointer<Item>(itemEntity);
      var itemAsset = f.FindAsset(item->Asset);

      var weight = itemAsset.weight;

      if (f.TryGetPointer(itemEntity, out WeaponItem* weaponItem)) {
        foreach (var weaponSlot in WeaponAttachmentSlotsExtension.AllValidSlots) {
          weight += GetItemWeight(f, weaponItem->AttachmentAtSlot(weaponSlot));
        }
      }

      return weight;
    }

    public static CellsRange GetItemTetris(Frame f, EntityRef itemEntity, bool withRotation) {
      if (itemEntity == EntityRef.None) {
        return CellsRange.Empty;
      }

      var item      = f.GetPointer<Item>(itemEntity);
      var itemAsset = f.FindAsset(item->Asset);

      var rotate = withRotation && item->Rotated;

      return CellsRange.FromIJWH(
        i: item->IndexI,
        j: item->IndexJ,
        width: rotate ? itemAsset.Height : itemAsset.Width,
        height: rotate ? itemAsset.Width : itemAsset.Height,
        rotated: rotate
      );
    }
  }
}