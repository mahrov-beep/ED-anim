namespace Quantum {
  using System.Collections.Generic;
  using Collections;

  public partial class EventLocalCharacterLoadoutModified {
    public readonly List<string> ModifiedItems = new List<string>();
  }

  partial class Frame {
    partial struct FrameEvents {
      public EventLocalCharacterLoadoutModified LocalCharacterLoadoutModified(
        Frame f, PlayerRef localPlayer, EntityRef characterEntity, QHashSetPtr<QGuid> modifiedItems) {
        var evt = LocalCharacterLoadoutModified(localPlayer, characterEntity);

        if (evt == null) {
          return null;
        }

        evt.ModifiedItems.Clear();

        foreach (var itemGuid in f.ResolveHashSet(modifiedItems)) {
          evt.ModifiedItems.Add(itemGuid.ToString());
        }

        return evt;
      }
    }
  }
}