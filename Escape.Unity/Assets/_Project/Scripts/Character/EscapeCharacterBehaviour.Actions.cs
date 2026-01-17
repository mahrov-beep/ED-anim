using System.Collections.Generic;
using System.Diagnostics;
using InfimaGames.LowPolyShooterPack;
using Multicast;
using UnityEngine;
using Debug = UnityEngine.Debug;

public partial class EscapeCharacterBehaviour {
    public void AssignWeapons(List<WeaponSetup> newWeaponSetups, int? selectedIndexNullable) {
        if (this.IsHolstering()) {
            Debug.LogError($"[{nameof(EscapeCharacterBehaviour)}] AssignWeapons must not be called in Holstering state");
            return;
        }

        bool hasWeaponPrefabs = HasAnyWeaponPrefab(newWeaponSetups);

        // если оружия в руках нет, то запускаем анимацию убирания оружия (...и ждем её завершения)
        if (selectedIndexNullable is not { } selectedIndex) {
            if (!this.IsHolstered()) {
                this.LogLocal("оружие было убрано из рук, запускаю анимацию holstering");

                if (hasWeaponPrefabs) {
                    this.characterAnimator.SetBool(AHashes.Holstering, true);
                    this.characterAnimator.SetBool(AHashes.Holstered, true);
                }
            }

            // оружие убрано, пытаемся сделать UnEquip
            if (this.inventory.GetEquipped() != null) {
                this.LogLocal("оружия не должно быть в руках и оружие holstered, делаю UnEquip");
                this.inventory.UnEquip();
            }

            this.inventory.AssignAndSyncWeaponSetups(newWeaponSetups);
            return;
        }

        // ДАЛЕЕ: оружие точно есть в руках в Quantum, нужно синхронизировать его с View

        // если поменялось только второстепенное оружие, то никакие анимации не нужны
        var equippedIndex = this.inventory.GetEquippedIndex();
        if (this.inventory.GetEquipped() != null &&
            equippedIndex == selectedIndex &&
            this.inventory.GetWeaponPrefab(equippedIndex) is var oldEquippedWeaponPrefab &&
            oldEquippedWeaponPrefab == newWeaponSetups[selectedIndex].WeaponPrefab) {
            this.inventory.AssignAndSyncWeaponSetups(newWeaponSetups);
            return;
        }

        // сменилось видимое оружие, так что
        // перед тем как менять оружие нужно запустить анимацию смены оружия

        if (!this.IsHolstered()) {
            this.LogLocal("было изменено основное оружие в руках, запускаю анимацию holstering");

            this.characterAnimator.SetBool(AHashes.Holstering, true);
            this.characterAnimator.SetBool(AHashes.Holstered, true);
            return;
        }

        // по завершению анимации будет вызван коллбек которые проставит holstering=false
        // и только когда мы попадем в это место выполнения кода
        
        this.LogLocal($"Должно быть выбрано новое оружие и оружие holstered, меняю оружие на {newWeaponSetups[selectedIndex].WeaponPrefab.name}");

        this.inventory.AssignAndSyncWeaponSetups(newWeaponSetups);

        this.characterAnimator.SetBool(AHashes.Holstered, false);

        this.inventory.UnEquip();
        this.inventory.Equip(selectedIndex);
    }

    static bool HasAnyWeaponPrefab(List<WeaponSetup> setups) {
        if (setups == null) {
            return false;
        }

        for (int i = 0; i < setups.Count; i++) {
            if (setups[i].WeaponPrefab != null) {
                return true;
            }
        }

        return false;
    }

    [Conditional("UNITY_EDITOR")]
    private void LogLocal(string message) {
        if (this.GetCharacterType() == CharacterTypes.LocalView) {
            Debug.Log("[EscapeCharacter(LOCAL)] " + message);
        }
    }

    /// <summary>
    /// Визуально включает/выключает текущее оружие без анимаций.
    /// </summary>
    public void SetEquippedWeaponActive(bool isActive) {
        var equippedWeapon = this.inventory.GetEquipped();
        if (equippedWeapon != null && equippedWeapon.gameObject.activeSelf != isActive) {
            equippedWeapon.gameObject.SetActive(isActive);
        }
    }

    /// <summary>
    /// Plays the inspect animation.
    /// </summary>
    public void Inspect() {
        this.characterAnimator.SetBool(AHashes.Inspecting, true);
        this.characterAnimator.CrossFade("Inspect", 0.0f, this.layerActions, 0);
    }

    /// <summary>
    /// Fires the character's weapon.
    /// </summary>
    public void Fire(WeaponFireData fireData) {
        if (this.inventory.GetEquipped() is var equippedWeapon && equippedWeapon == null) {
            return;
        }
        
        //Fire the weapon! Make sure that we also pass the scope's spread multiplier if we're aiming.
        equippedWeapon.Fire(fireData,
            this.aiming && equippedWeapon.GetAttachmentManager().GetEquippedScope() is var equippedScope && equippedScope
                ? equippedScope.GetMultiplierSpread()
                : 1.0f);

        //Play firing animation.
        const string stateName = "Fire";
        this.characterAnimator.CrossFade(stateName, 0.05f, this.layerOverlay, 0);

        //Play bolt actioning animation if needed, and if we have ammunition. We don't play this for the last shot.
        if (equippedWeapon.IsBoltAction() && fireData.HasAmmoForNextShot) {
            this.characterAnimator.SetBool(AHashes.Bolt, true);
        }
    }

    /// <summary>
    /// Plays the reload animation.
    /// </summary>
    public void PlayReloadAnimation(bool emptyReload) {
        if (this.inventory.GetEquipped() is var equippedWeapon && equippedWeapon == null) {
            return;
        }

        //Get the name of the animation state to play, which depends on weapon settings, and ammunition!
        var stateName = equippedWeapon.HasCycledReload() ? "Reload Open" : (!emptyReload ? "Reload" : "Reload Empty");

        //Play the animation state!
        this.characterAnimator.Play(stateName, this.layerActions, 0.0f);

        //Set Reloading Bool. This helps cycled reloads know when they need to stop cycling.
        this.characterAnimator.SetBool(AHashes.Reloading, true);

        //Reload.
        equippedWeapon.Reload(emptyReload);
    }

    /// <summary>
    /// Plays the healing animation similarly to reload (direct state jump).
    /// </summary>
    public void PlayHealingAnimation() {
       /// this.characterAnimator.Play("Healing", this.layerActions, 0.0f);
        this.characterAnimator.SetBool(AHashes.Healing, true);
        this.characterAnimator.SetBool(AHashes.Holstered, true);
    }
    
    public void FireEmpty() {
        this.characterAnimator.CrossFade("Fire Empty", 0.05f, this.layerOverlay, 0);
    }

    /// <summary>
    /// Plays The Grenade Throwing Animation.
    /// </summary>
    public void PlayGrenadeThrow() {
        this.characterAnimator.SetBool(AHashes.Grenading, true);
        this.characterAnimator.CrossFade("Grenade Throw", 0.15f, this.characterAnimator.GetLayerIndex("Layer Actions Arm Left"), 0.0f);
        this.characterAnimator.CrossFade("Grenade Throw", 0.05f, this.characterAnimator.GetLayerIndex("Layer Actions Arm Right"), 0.0f);
    }

    /// <summary>
    /// Play The Melee Animation.
    /// </summary>
    public void PlayMelee() {
        this.characterAnimator.SetBool(AHashes.Meleeing, true);
        this.characterAnimator.CrossFade("Knife Attack", 0.05f, this.characterAnimator.GetLayerIndex("Layer Actions Arm Left"), 0.0f);
        this.characterAnimator.CrossFade("Knife Attack", 0.05f, this.characterAnimator.GetLayerIndex("Layer Actions Arm Right"), 0.0f);
    }

    public void PlayFullBody(CharacterFullBodyStates state) {
        this.characterAnimator.CrossFade(EnumNames<CharacterFullBodyStates>.GetName(state), 0.2f, this.layerFullBodyActions);

        if (state == CharacterFullBodyStates.Died) {
            this.GetAudioPlayer().PlayOneShot(CharacterAudioLayers.Voice, this.config.audioClipsDied);
        }
    }
}
