using System.Collections.Generic;
using System.Linq;
using InfimaGames.LowPolyShooterPack;
using TriInspector;
using UnityEngine;

public class EscapeCharacterInventoryTest : EscapeCharacterInventory {
    [SerializeField, Required] private List<GameObject> weaponPrefabs;

    public override void Init(int equippedAtStart = 0) {
        base.Init(equippedAtStart);

        this.AssignAndSyncWeaponSetups(this.weaponPrefabs.Select(it => new WeaponSetup { WeaponPrefab = it }).ToList());
        this.Equip(equippedAtStart);
    }
}