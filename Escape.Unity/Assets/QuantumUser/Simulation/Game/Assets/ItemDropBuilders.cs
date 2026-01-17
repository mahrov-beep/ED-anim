// ReSharper disable FieldCanBeMadeReadOnly.Local

namespace Quantum {
  using System;
  using System.Collections.Generic;
  using Photon.Deterministic;
  using Sirenix.OdinInspector;
  using UnityEngine;
  using UnityEngine.Pool;

  public static class ItemDropBuilders {
    public sealed class AllItems : ItemDropBuilderBase {
      [SerializeField]
      [Help("Из генератора выпадают все предметы из списка")]
      LoadoutConfig.ItemConfig[] items = Array.Empty<LoadoutConfig.ItemConfig>();

      public override void Build(Frame f, List<GameSnapshotLoadoutItem> result, ItemDropBuildContext context) {
        foreach (var itemConfig in items) {
          if (f.FindAsset(itemConfig.asset).excludeFromDropBuilders) {
            continue;
          }

          result.Add(itemConfig.BuildItem(f));
        }
      }
    }

    public sealed class Asset : ItemDropBuilderBase {
      [SerializeField]
      [Required]
      [InlineEditor]
      [Help("Внешний ассет который будет использован для генерации предметов")]
      ItemDropBuilderAsset asset;

      public override void Build(Frame f, List<GameSnapshotLoadoutItem> result, ItemDropBuildContext context) {
        asset.Build(f, result, context);
      }
    }

    public sealed class ProbabilityItems : ItemDropBuilderBase {
      [SerializeField]
      [Help("вероятность того что из генератора выпадет первый предмет (100 - выпадает всегда)")]
      [RangeEx(0, 100, UseSlider = true)]
      FP dropProbability = 100;

      [SerializeField]
      [Help("на сколько процентов понижается вероятность выпадения второго предмета и далее (100 - полное понижение до нуля)")]
      [RangeEx(0, 100, UseSlider = true)]
      FP dropProbabilityDecrease = 0;

      [SerializeField]
      [Help("предметы которые могут выпасть и относительная вероятность выпадения для каждого предмета. " +
               "Каждый предмет может выпасть не более одного раза")]
      [TableList(ShowPaging = false, AlwaysExpanded = true)]
      ItemWithProbability[] items = Array.Empty<ItemWithProbability>();

      [Serializable]
      struct ItemWithProbability {
        public LoadoutConfig.ItemConfig item;

        [LabelText(" "), LabelWidth(1)] // без этого внутри TableList не работает RangeEx O_o
        [Tooltip("относительная вероятность выпадения для каждого предмета")]
        [RangeEx(0, 100, UseSlider = true)]
        public FP probability;
      }

      public override unsafe void Build(Frame f, List<GameSnapshotLoadoutItem> result, ItemDropBuildContext context) {
        using var _ = ListPool<ItemWithProbability>.Get(out var remainingDrops);

        foreach (var item in items) {
          if (f.FindAsset(item.item.asset).excludeFromDropBuilders) {
            continue;
          }

          remainingDrops.Add(item);
        }

        var currentProbability01 = dropProbability * FP._0_01;

        while (remainingDrops.Count > 0 && context.RNG->Roll01(currentProbability01)) {
          var randomDropIndex = context.RNG->GetRandomElementIndexWithIntervals(remainingDrops, static it => it.probability);
          var randomDrop      = remainingDrops[randomDropIndex];
          remainingDrops.RemoveAt(randomDropIndex);

          result.Add(randomDrop.item.BuildItem(f));

          currentProbability01 *= 1 - dropProbabilityDecrease * FP._0_01;
        }
      }
    }
  }
}