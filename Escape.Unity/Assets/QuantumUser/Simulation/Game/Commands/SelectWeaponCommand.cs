namespace Quantum.Commands {
using Photon.Deterministic;
using UnityEngine;
using BitStream = Photon.Deterministic.BitStream;
public class SelectWeaponCommand : CharacterCommandBase {
  public CharacterLoadoutSlots SlotType;

  public override void Serialize(BitStream stream) {
    stream.Serialize(ref SlotType);
  }

  public override unsafe void Execute(Frame f, EntityRef characterEntity) {
    var unit = f.GetPointer<Unit>(characterEntity);

    if (unit->IsWeaponChanging) {
      return;
    }

    switch (SlotType) {
      case CharacterLoadoutSlots.MeleeWeapon:
        unit->TryChangeWeapon(f, unit->MeleeWeapon);
        break;
      case CharacterLoadoutSlots.SecondaryWeapon:
        unit->TryChangeWeapon(f, unit->SecondaryWeapon);
        break;
      case CharacterLoadoutSlots.PrimaryWeapon:
        unit->TryChangeWeapon(f, unit->PrimaryWeapon);
        break;
    }
  }
}
}