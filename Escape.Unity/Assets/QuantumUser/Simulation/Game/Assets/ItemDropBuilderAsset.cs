namespace Quantum {
  using System;
  using System.Collections.Generic;
  using Photon.Deterministic;
  using Sirenix.OdinInspector;
  using UnityEngine;

  public class ItemDropBuilderAsset : AssetObject {
    [SerializeField]
    [Help("Список генераторов, награда последовательно генерируется из КАЖДОГО генератора")]
    internal ItemDropBuilderWrapper[] itemDropBuilders;

    [Serializable]
    public struct ItemDropBuilderWrapper {
      [SerializeReference]
      [HideLabel]
      [Required]
      public ItemDropBuilderBase builder;
    }

    public void Build(Frame f, List<GameSnapshotLoadoutItem> items, ItemDropBuildContext context) {
      foreach (var wrapper in itemDropBuilders) {
        wrapper.builder.Build(f, items, context);
      }
    }
  }

  public abstract class ItemDropBuilderBase {
    public abstract void Build(Frame f, List<GameSnapshotLoadoutItem> result, ItemDropBuildContext context);
  }

  public unsafe struct ItemDropBuildContext {
    public RNGSession* RNG;
  }
}