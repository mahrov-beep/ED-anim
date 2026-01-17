namespace Quantum {
  using Commands;
  using Photon.Deterministic;

  public unsafe class MoveWeaponAttachmentFromSlotToSlotLoadoutCommand : CharacterCommandBase {
    public CharacterLoadoutSlots OldSlotType;
    public WeaponAttachmentSlots OldWeaponSlotType;
    public CharacterLoadoutSlots NewSlotType;
    public WeaponAttachmentSlots NewWeaponSlotType;
    public EntityRef             ItemEntity;

    public override void Serialize(BitStream stream) {
      stream.Serialize(ref OldSlotType);
      stream.Serialize(ref OldWeaponSlotType);
      stream.Serialize(ref NewSlotType);
      stream.Serialize(ref NewWeaponSlotType);
      stream.Serialize(ref ItemEntity);
    }

    public override void Execute(Frame f, EntityRef characterEntity) {
      if (!f.Unsafe.TryGetPointer<CharacterLoadout>(characterEntity, out var loadout)) {
        Log.Error("MoveWeaponAttachmentFromSlotToSlotLoadoutCommand: loadout not exist");
        return;
      }

      if (!f.TryGetPointer<WeaponItem>(loadout->ItemAtSlot(this.OldSlotType), out var oldWeaponItem)) {
        Log.Error("MoveWeaponAttachmentFromSlotToSlotLoadoutCommand: item at old slot is not WeaponItem");
        return;
      }

      if (!f.TryGetPointer<WeaponItem>(loadout->ItemAtSlot(this.NewSlotType), out var newWeaponItem)) {
        Log.Error("MoveWeaponAttachmentFromSlotToSlotLoadoutCommand: item at new slot is not WeaponItem");
        return;
      }

      if (!oldWeaponItem->UnassignAttachmentFromSlot(f, this.OldWeaponSlotType, this.ItemEntity)) {
        Log.Error("MoveWeaponAttachmentFromSlotToSlotLoadoutCommand: failed to unassign weapon attachment from old weapon slot");
        return;
      }

      if (newWeaponItem->HasAttachmentAtSlot(this.NewWeaponSlotType)) {
        var newAttachmentEntity = newWeaponItem->AttachmentAtSlot(this.NewWeaponSlotType);

        if (!newWeaponItem->UnassignAttachmentFromSlot(f, this.NewWeaponSlotType, newAttachmentEntity)) {
          Log.Error("MoveWeaponAttachmentFromSlotToSlotLoadoutCommand: failed to unassign weapon attachment from new weapon slot");
          return;
        }

        if (!oldWeaponItem->AssignAttachmentToSlot(f, this.NewWeaponSlotType, newAttachmentEntity)) {
          Log.Error("MoveWeaponAttachmentFromSlotToSlotLoadoutCommand: failed to assign weapon attachment to old weapon slot");
 
          newWeaponItem->AssignAttachmentToSlot(f, this.NewWeaponSlotType, newAttachmentEntity);

          return;
        }
      }

      if (!newWeaponItem->AssignAttachmentToSlot(f, this.NewWeaponSlotType, this.ItemEntity)) {
        Log.Error("MoveWeaponAttachmentFromSlotToSlotLoadoutCommand: failed to assign weapon attachment to new weapon slot");

        oldWeaponItem->AssignAttachmentToSlot(f, this.OldWeaponSlotType, this.ItemEntity); // возвращаем обратно если ошибка
        return;
      }
    }
  }
}