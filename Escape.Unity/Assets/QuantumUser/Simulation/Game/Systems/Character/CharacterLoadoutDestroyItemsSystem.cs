namespace Quantum {
  public class CharacterLoadoutDestroyItemsSystem : SystemSignalsOnly, ISignalOnComponentRemoved<CharacterLoadout> {
    public unsafe void OnRemoved(Frame f, EntityRef entity, CharacterLoadout* loadout) {
      loadout->DestroyAllItems(f);
    }
  }
}