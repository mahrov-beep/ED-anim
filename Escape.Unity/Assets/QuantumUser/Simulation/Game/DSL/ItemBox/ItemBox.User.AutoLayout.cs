namespace Quantum {
  using System;
  using System.Collections.Generic;
  using Photon.Deterministic;

  public unsafe partial struct ItemBox {
    public void AutoLayoutItemsInTetris(Frame f, bool useMinPlace = false) {
      var itemRefs = f.ResolveList(this.ItemRefs);
      if (itemRefs.Count == 0) {
        return;
      }

      var items = new List<(EntityRef entity, int width, int height)>();
      var maxWidth = 0;

      var area = 0;

      foreach (var itemRef in itemRefs) {
        if (!f.TryGet(itemRef, out Item item)) {
          continue;
        }

        var itemAsset = f.FindAsset(item.Asset);
        items.Add((itemRef, itemAsset.Width, itemAsset.Height));

        area += itemAsset.Width * itemAsset.Height;
        
        if (maxWidth < itemAsset.Width) {
          maxWidth = itemAsset.Width;
        }
      }

      if (useMinPlace) {
        maxWidth = Math.Max(maxWidth, (int)Math.Sqrt(area));
      }
      
      if (items.Count == 0) {
        return;
      }

      const int maxGridWidth = 5;
      
      this.Width = Math.Min(maxGridWidth, Math.Max(maxWidth, useMinPlace ? maxWidth : maxGridWidth));

      var columnHeights = new int[this.Width];

      items.Sort((a, b) => {
        var areaA = a.width * a.height;
        var areaB = b.width * b.height;

        var cmp = areaB.CompareTo(areaA);

        return cmp != 0 ? cmp : a.entity.Index.CompareTo(b.entity.Index);
      });

      foreach (var (entity, width, height) in items) {
        var placed = TryPlaceItem(f, columnHeights, entity, width, height, rotated: false);

        if (!placed && width != height) {
          placed = TryPlaceItem(f, columnHeights, entity, height, width, rotated: true);
        }

        if (!placed) {
          Log.Error($"AutoLayoutItemsInTetris: Failed to place item {entity}");
        }
      }

      this.Height = 0;
      for (var i = 0; i < this.Width; i++) {
        if (this.Height < columnHeights[i]) {
          this.Height = columnHeights[i];
        }
      }

      if (!useMinPlace && this.Height < 2) {
        this.Height = 2;
      }

      Log.Info($"AutoLayoutItemsInTetris: Arranged {items.Count} items in {this.Width}x{this.Height} grid");
    }

    private bool TryPlaceItem(Frame f, int[] columnHeights, EntityRef itemEntity, int width, int height, bool rotated) {
      if (width > this.Width) {
        return false;
      }

      var bestIndex = -1;
      var bestHeight = int.MaxValue;
      var bestBase = int.MaxValue;

      for (var start = 0; start <= this.Width - width; start++) {
        var baseHeight = 0;

        for (var offset = 0; offset < width; offset++) {
          baseHeight = Math.Max(baseHeight, columnHeights[start + offset]);
        }

        var totalHeight = baseHeight + height;

        if (totalHeight < bestHeight || (totalHeight == bestHeight && baseHeight < bestBase)) {
          bestHeight = totalHeight;
          bestBase = baseHeight;
          bestIndex = start;
        }
      }

      if (bestIndex == -1) {
        return false;
      }

      for (var offset = 0; offset < width; offset++) {
        columnHeights[bestIndex + offset] = bestHeight;
      }

      var item = f.GetPointer<Item>(itemEntity);
      item->IndexI = (byte)bestBase;
      item->IndexJ = (byte)bestIndex;
      item->Rotated = rotated;

      return true;
    }
  }
}

