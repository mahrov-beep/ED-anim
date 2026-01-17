namespace Quantum {
  using Commands;
  using Photon.Deterministic;

  public unsafe class MoveWeaponAttachmentFromSlotToTetrisLoadoutCommand : CharacterCommandBase {
    public CharacterLoadoutSlots OldSlotType;
    public WeaponAttachmentSlots OldWeaponSlotType;
    public EntityRef             ItemEntity;
    public int                   IndexInTrashI;
    public int                   IndexInTrashJ;
    public bool                  Rotated;
    public byte                  DestinationSource;

    public override void Serialize(BitStream stream) {
      stream.Serialize(ref OldSlotType);
      stream.Serialize(ref OldWeaponSlotType);
      stream.Serialize(ref ItemEntity);
      stream.Serialize(ref IndexInTrashI);
      stream.Serialize(ref IndexInTrashJ);
      stream.Serialize(ref Rotated);
      stream.Serialize(ref DestinationSource);
    }

    public override void Execute(Frame f, EntityRef characterEntity) {
      if (!f.Unsafe.TryGetPointer<CharacterLoadout>(characterEntity, out var loadout)) {
        Log.Error("MoveWeaponAttachmentFromSlotToTrashLoadoutCommand: loadout not exist");
        return;
      }

      if (!f.TryGetPointer<WeaponItem>(loadout->ItemAtSlot(this.OldSlotType), out var weaponItem)) {
        Log.Error("MoveWeaponAttachmentFromSlotToTrashLoadoutCommand: item at slot is not WeaponItem");
        return;
      }

      if (!weaponItem->UnassignAttachmentFromSlot(f, this.OldWeaponSlotType, this.ItemEntity)) {
        Log.Error("MoveWeaponAttachmentFromSlotToTrashLoadoutCommand: failed to unassign weapon attachment from old weapon slot");
        return;
      }

      if (!loadout->AddItemToTrash(f, this.ItemEntity, this.IndexInTrashI, this.IndexInTrashJ, this.Rotated, null, this.DestinationSource)) {
        Log.Error("MoveWeaponAttachmentFromSlotToTrashLoadoutCommand: failed to add item to trash");

        weaponItem->AssignAttachmentToSlot(f, this.OldWeaponSlotType, this.ItemEntity); // возвращаем обратно если ошибка
        return;
      }
    }
  }
}