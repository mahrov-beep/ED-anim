using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using InfimaGames.LowPolyShooterPack;
using Quantum;
using UnityEngine;

public class MenuCharacterEquipmentApplier {
    private readonly EscapeCharacterRemotePlayer character;
    
    private bool isApplyingEquipment;
    private string lastPrimaryWeaponKey;
    private string lastSecondaryWeaponKey;

    public MenuCharacterEquipmentApplier(EscapeCharacterRemotePlayer character) {
        this.character = character;
    }

    public async void ApplyEquipment(GameSnapshotLoadout loadout) {
        if (loadout?.SlotItems == null || this.character == null || this.isApplyingEquipment) {
            return;
        }

        var primaryWeaponItem = CharacterLoadoutSlots.PrimaryWeapon.ToInt() < loadout.SlotItems.Length
            ? loadout.SlotItems[CharacterLoadoutSlots.PrimaryWeapon.ToInt()]
            : null;

        var secondaryWeaponItem = CharacterLoadoutSlots.SecondaryWeapon.ToInt() < loadout.SlotItems.Length
            ? loadout.SlotItems[CharacterLoadoutSlots.SecondaryWeapon.ToInt()]
            : null;

        var currentPrimaryKey   = primaryWeaponItem?.ItemKey ?? "";
        var currentSecondaryKey = secondaryWeaponItem?.ItemKey ?? "";

        if (this.lastPrimaryWeaponKey == currentPrimaryKey && this.lastSecondaryWeaponKey == currentSecondaryKey) {
            return;
        }

        this.lastPrimaryWeaponKey   = currentPrimaryKey;
        this.lastSecondaryWeaponKey = currentSecondaryKey;

        var weaponSetups      = new List<WeaponSetup>();
        int? activeWeaponIndex = null;

        if (primaryWeaponItem != null && !string.IsNullOrEmpty(primaryWeaponItem.ItemKey)) {
            var setup = CreateWeaponSetup(primaryWeaponItem);
            
            if (setup.HasValue) {
                weaponSetups.Add(setup.Value);
                activeWeaponIndex = 0;
            }
        }

        if (secondaryWeaponItem != null && !string.IsNullOrEmpty(secondaryWeaponItem.ItemKey)) {
            var setup = CreateWeaponSetup(secondaryWeaponItem);
            
            if (setup.HasValue) {
                weaponSetups.Add(setup.Value);
                if (!activeWeaponIndex.HasValue) {
                    activeWeaponIndex = weaponSetups.Count - 1;
                }
            }
        }

        await ApplyEquipmentAsync(weaponSetups, activeWeaponIndex);
    }

    private async UniTask ApplyEquipmentAsync(List<WeaponSetup> weaponSetups, int? activeWeaponIndex) {
        this.isApplyingEquipment = true;

        var animator = this.character.GetCharacterAnimator();
        var inventory = this.character.GetInventory();

        if (inventory.GetEquipped() != null) {
            animator.SetBool(AHashes.Holstering, true);
            animator.SetBool(AHashes.Holstered, true);
            
            await UniTask.Delay(300);
        }
        
        animator.SetBool(AHashes.Holstering, false);
        animator.SetBool(AHashes.Holstered, true);
        
        await UniTask.Yield();

        this.character.AssignWeapons(weaponSetups, activeWeaponIndex);

        await UniTask.Yield();

        animator.SetBool(AHashes.Holstered, false);

        this.character.OnUpdateView(new EscapeCharacterState {
            IsAiming            = false,
            IsRunning           = false,
            IsHealing           = false,
            HealingProgress     = 0,
            AxisLookDelta       = Vector2.zero,
            AxisMovement        = Vector2.zero,
            IsCrouching         = false,
            IsGrounded          = true,
            IsJumping           = false,
            CameraRotationAngle = 0,
        });

        this.isApplyingEquipment = false;
    }

    private static WeaponSetup? CreateWeaponSetup(GameSnapshotLoadoutItem weaponItem) {
        var weaponAssetPath = ItemAssetCreationData.GetItemAssetPath(weaponItem.ItemKey);
        var itemAsset       = QuantumUnityDB.Global.GetRequiredAsset<ItemAsset>(weaponAssetPath);

        if (itemAsset is not WeaponItemAsset weaponAsset || weaponAsset.visualPrefab == null) {
            return null;
        }

        var setup = new WeaponSetup {
            WeaponPrefab = weaponAsset.visualPrefab,
        };

        if (weaponItem.WeaponAttachments != null) {
            setup.ScopePrefab    = GetAttachmentPrefab(weaponItem.WeaponAttachments, WeaponAttachmentSlots.Scope);
            setup.GripPrefab     = GetAttachmentPrefab(weaponItem.WeaponAttachments, WeaponAttachmentSlots.Grip);
            setup.MuzzlePrefab   = GetAttachmentPrefab(weaponItem.WeaponAttachments, WeaponAttachmentSlots.Muzzle);
            setup.MagazinePrefab = GetAttachmentPrefab(weaponItem.WeaponAttachments, WeaponAttachmentSlots.Magazine);
            setup.StockPrefab    = GetAttachmentPrefab(weaponItem.WeaponAttachments, WeaponAttachmentSlots.Stock);
            setup.LaserPrefab    = GetAttachmentPrefab(weaponItem.WeaponAttachments, WeaponAttachmentSlots.Laser);
        }

        return setup;
    }

    private static GameObject GetAttachmentPrefab(GameSnapshotLoadoutWeaponAttachment[] attachments, WeaponAttachmentSlots slot) {
        var slotIndex = slot.ToInt();
        
        if (slotIndex >= attachments.Length) {
            return null;
        }

        var attachment = attachments[slotIndex];
       
        if (attachment == null || string.IsNullOrEmpty(attachment.ItemKey)) {
            return null;
        }

        var attachmentAssetPath = ItemAssetCreationData.GetItemAssetPath(attachment.ItemKey);
        var itemAsset           = QuantumUnityDB.Global.GetRequiredAsset<ItemAsset>(attachmentAssetPath);

        if (itemAsset is not WeaponAttachmentItemAsset attachmentAsset) {
            return null;
        }

        return attachmentAsset.visualPrefab;
    }
}


