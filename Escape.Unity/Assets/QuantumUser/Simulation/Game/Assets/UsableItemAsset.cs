namespace Quantum {
  using System;
  using System.Collections.Generic;
  using Photon.Deterministic;
  using Sirenix.OdinInspector;

  [Serializable]
  public abstract class UsableItemAsset : ItemAsset {
    public FP useDurationSeconds = 1;

    [TableList(AlwaysExpanded = true, ShowPaging = false)]
    public Effect[] onUseEffects = Array.Empty<Effect>();

    public virtual bool CanBeUsed(Frame f, EntityRef itemEntity) {
      if (f.GameMode.rule is GameRules.MainMenuStorage or GameRules.MainMenuGameResults) {
        return false;
      }

      if (Item.GetRemainingUsages(f, itemEntity) == 0) {
        return false;
      }

      return true;
    }

    public virtual void UseItem(Frame f, EntityRef itemEntity, EntityRef unitEntity) {
      onUseEffects.ApplyAll(f, unitEntity);

      Item.UseItem(f, itemEntity, 1);
    }
  }
}