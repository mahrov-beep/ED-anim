namespace Quantum {
  using Photon.Deterministic;

  public static class BitStreamUser {
    public static void Serialize(this BitStream stream, ref WeaponAttachmentSlots slot) {
      if (stream.Writing) {
        stream.WriteInt((int)slot);
      }
      else {
        slot = (WeaponAttachmentSlots)stream.ReadInt();
      }
    }

    public static void Serialize(this BitStream stream, ref CharacterLoadoutSlots slot) {
      if (stream.Writing) {
        stream.WriteInt((int)slot);
      }
      else {
        slot = (CharacterLoadoutSlots)stream.ReadInt();
      }
    }

    public static void Serialize(this BitStream stream, ref GameRules gameModes) {
      if (stream.Writing) {
        stream.WriteInt((int)gameModes);
      }
      else {
        gameModes = (GameRules)stream.ReadInt();
      }
    }
  }
}