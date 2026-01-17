using System;
using System.Collections.Generic;
using InfimaGames.LowPolyShooterPack;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;

public class EscapeCharacterInventory : InventoryBehaviour {
    [SerializeField, Required]
    private CharacterBehaviour characterBehaviour;

    [SerializeField, Required]
    private Transform weaponsSocket;

    [CanBeNull] private WeaponBehaviour equipped;

    private int equippedIndex = -1;

    private readonly List<WeaponSetup> runtimeWeaponSetups = new List<WeaponSetup>();

    public override void Init(int equippedAtStart = 0) {
    }

    public override WeaponBehaviour Equip(int index) {
        this.UnEquip();

        var equippedSetup = this.runtimeWeaponSetups[index];
        var equippedObj   = Instantiate(equippedSetup.WeaponPrefab, this.weaponsSocket);
        equippedObj.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        var weapon = equippedObj.GetComponent<WeaponBehaviour>();
        weapon.Init(this.characterBehaviour);

        if (weapon.GetAttachmentManager() is var attachmentManager && attachmentManager) {
            attachmentManager.SyncWeaponSetup(equippedSetup);
        }

        this.equipped      = weapon;
        this.equippedIndex = index;

        return this.equipped;
    }

    public void UnEquip() {
        if (this.equipped == null) {
            return;
        }

        Destroy(this.equipped.gameObject);
        this.equipped = null;
        this.equippedIndex = -1;
    }

    public void AssignAndSyncWeaponSetups(List<WeaponSetup> newSetups) {
        this.runtimeWeaponSetups.Clear();

        foreach (var newSetup in newSetups) {
            this.runtimeWeaponSetups.Add(newSetup);
        }

        this.SyncEquippedWeaponSetup();
    }

    public void SyncEquippedWeaponSetup() {
        if (this.equipped == null) {
            return;
        }
        
        if (this.equippedIndex < 0 || this.equippedIndex >= this.runtimeWeaponSetups.Count) {
            return;
        }

        if (this.equipped!.GetAttachmentManager() is var attachmentManager && attachmentManager) {
            attachmentManager.SyncWeaponSetup(this.runtimeWeaponSetups[this.equippedIndex]);
        }
    }

    [CanBeNull] public GameObject GetWeaponPrefab(int index) => index >= 0 && index < this.runtimeWeaponSetups.Count
        ? this.runtimeWeaponSetups[index].WeaponPrefab
        : null;

    public override int GetLastIndex() {
        var newIndex = this.equippedIndex - 1;
        return newIndex < 0 ? this.runtimeWeaponSetups.Count - 1 : newIndex;
    }

    public override int GetNextIndex() {
        var newIndex = this.equippedIndex + 1;
        return newIndex >= this.runtimeWeaponSetups.Count ? 0 : newIndex;
    }

    public override WeaponBehaviour GetEquipped() => this.equipped;

    public override int GetEquippedIndex() => this.equippedIndex;
}