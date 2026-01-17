namespace Quantum {
  using System;

  public partial struct CharacterLoadout {
    public bool CanMerge(Frame f, EntityRef sourceItemRef, EntityRef targetItemRef) {
      return MergeInternal(f, sourceItemRef, targetItemRef, realRun: false, maxUsagesToMerge: int.MaxValue);
    }

    public bool Merge(Frame f, EntityRef sourceItemRef, EntityRef targetItemRef, int maxUsagesToMerge = int.MaxValue, byte source = 0) {
      return MergeInternal(f, sourceItemRef, targetItemRef, realRun: true, maxUsagesToMerge);
    }

    unsafe bool MergeInternal(Frame f, EntityRef sourceItemRef, EntityRef targetItemRef, bool realRun, int maxUsagesToMerge, byte source = 0) {
      if (sourceItemRef == EntityRef.None || targetItemRef == EntityRef.None) {
        if (realRun) {
          Log.Error("Trying to merge Entity.None");
        }

        return false;
      }

      if (sourceItemRef == targetItemRef) {
        if (realRun) {
          Log.Error("Cannot merge ite with self");
        }

        return false;
      }

      var sourceItem = f.GetPointer<Item>(sourceItemRef);
      var targetItem = f.GetPointer<Item>(targetItemRef);

      if (sourceItem->Asset != targetItem->Asset) {
        if (realRun) {
          Log.Error("Cannot merge item with different asset types");
        }

        return false;
      }

      var itemAsset = f.FindAsset(sourceItem->Asset);

      if (!itemAsset.Def.AllowMerge) {
        if (realRun) {
          Log.Error("Cannot merge item with AllowMerge is false");
        }

        return false;
      }

      if (realRun) {
        var sourceRemainingUsages = Item.GetRemainingUsages(f, sourceItemRef);
        var targetUsedUsages      = Item.GetUsedUsages(f, targetItemRef);

        var amountToTransfer = (ushort)Math.Min(sourceRemainingUsages, targetUsedUsages);

        amountToTransfer = (ushort)Math.Min(amountToTransfer, Math.Max(0, maxUsagesToMerge));

        Item.UseItem(f, sourceItemRef, amountToTransfer);
        Item.DeUseItem(f, targetItemRef, amountToTransfer);

        var ownerEntity = (TetrisSource)source == TetrisSource.Storage ? this.StorageEntity : this.SelfUnitEntity;
        
        NotifyLoadoutModified(f, sourceItemRef, ownerEntity);
        NotifyLoadoutModified(f, targetItemRef, ownerEntity);
      }

      return true;
    }
  }
}