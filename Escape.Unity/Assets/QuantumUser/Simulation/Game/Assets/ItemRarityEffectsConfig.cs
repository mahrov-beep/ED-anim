namespace Quantum {
using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using static ItemAsset;
public class ItemRarityEffectsConfig : AssetObject {
  public List<RarityEffects> rarityEffects;

  [Serializable]
  public struct RarityEffects {
    public ERarityType                rarity;
    
    [TableList(ShowPaging = false, AlwaysExpanded = true)]
    [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)]
    public List<PersistentItemEffect> itemEffects;
  }

#if UNITY_EDITOR
  [Sirenix.OdinInspector.Button] void FillMissingRarity() {
    UnityEditor.Undo.RecordObject(this, nameof(FillMissingRarity));

    ERarityType[] rarityTypes = Enum.GetValues(typeof(ERarityType)).Cast<ERarityType>().ToArray();
    foreach (ERarityType rarityType in rarityTypes) {
      if (rarityEffects.Any(re => re.rarity == rarityType)) {
        continue;
      }

      rarityEffects.Add(new RarityEffects { rarity = rarityType });
    }
  }
#endif
}
}