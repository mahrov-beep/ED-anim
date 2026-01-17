namespace Quantum {
  using System;
  using Photon.Deterministic;
  using Sirenix.OdinInspector;
  using UnityEngine;

  [Serializable]
  public class WeaponAttachmentItemAsset : ItemAsset {
    [ShowInInspector]
    [PreviewField(100, ObjectFieldAlignment.Left), HideLabel, PropertyOrder(-900)]
    GameObject InspectorPrefabForPreview => this.visualPrefab;

    [PropertySpace(0, 10), PropertyOrder(-100)]
    public string inspectorShortName;

    public WeaponAttachmentSlots[] validWeaponSlots = Array.Empty<WeaponAttachmentSlots>();

    [Help("Префаб обвеса")]
    public GameObject visualPrefab;

    public override ItemTypes ItemType => ItemTypes.WeaponAttachment;

    public override ItemAssetGrouping Grouping => ItemAssetGrouping.WeaponAttachments;

    public override bool CanBeAssignedToSlot(Frame f, EntityRef targetEntity, EntityRef itemEntity,
      CharacterLoadoutSlots slot, WeaponAttachmentSlots weaponSlot, out ItemAssetAssignFailReason reason) {
      if (slot != CharacterLoadoutSlots.Invalid || weaponSlot == WeaponAttachmentSlots.Invalid) {
        reason = ItemAssetAssignFailReason.SlotNotValidForItem;
        return false;
      }

      if (Array.IndexOf(this.validWeaponSlots, weaponSlot) == -1) {
        reason = ItemAssetAssignFailReason.SlotNotValidForItem;
        return false;
      }

      reason = ItemAssetAssignFailReason.None;
      return true;
    }
  }
}