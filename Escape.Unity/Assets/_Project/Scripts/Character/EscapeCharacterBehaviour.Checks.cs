using System;
using UnityEngine;

public partial class EscapeCharacterBehaviour {
    public bool CanAssignWeapons() {        
        //Block.
        if (this.IsHolstering()) {
            return false;
        }

        //Block.
        if (this.IsMeleeing() || this.IsThrowingGrenade()) {
            return false;
        }

        //Block.
        if (this.IsReloading() || this.IsBolting()) {
            return false;
        }

        //Block.
        if (this.IsInspecting()) {
            return false;
        }

        //Return.
        return true;
    }
    
    /// <summary>
    /// Can Fire.
    /// </summary>
    public bool CanPlayAnimationFire() {
        if (this.inventory.GetEquipped() == null) {
            return false;
        }

        if (this.IsHealing()) {
            return false;
        }

        
        //Block.
        if (this.IsHolstered() || this.IsHolstering()) {
            return false;
        }

        //Block.
        if (this.IsMeleeing() || this.IsThrowingGrenade()) {
            return false;
        }

        //Block.
        if (this.IsReloading() || this.IsBolting()) {
            return false;
        }

        //Block.
        if (this.IsInspecting()) {
            return false;
        }

        //Return.
        return true;
    }

    /// <summary>
    /// Determines if we can play the reload animation.
    /// </summary>
    public bool CanPlayAnimationReload() {
        if (this.inventory.GetEquipped() == null) {
            return false;
        }

        if (this.IsHealing()) {
            return false;
        }


        if (this.IsHolstered() || this.IsHolstering()) {
            return false;
        }

        //No reloading!
        if (this.IsReloading()) {
            return false;
        }

        //No meleeing!
        if (this.IsMeleeing()) {
            return false;
        }

        //Not actioning a bolt.
        if (this.IsBolting()) {
            return false;
        }

        //Can't reload while throwing a grenade.
        if (this.IsThrowingGrenade()) {
            return false;
        }

        //Block while inspecting.
        if (this.IsInspecting()) {
            return false;
        }

        //Return.
        return true;
    }

    /// <summary>
    /// Returns true if the character is able to throw a grenade.
    /// </summary>
    public bool CanPlayAnimationGrenadeThrow() {
        if (this.IsHealing()) {
            return false;
        }

        //Block.
        if (this.IsHolstered() || this.IsHolstering()) {
            return false;
        }

        //Block.
        if (this.IsMeleeing() || this.IsThrowingGrenade()) {
            return false;
        }

        //Block.
        if (this.IsReloading() || this.IsBolting()) {
            return false;
        }

        //Block.
        if (this.IsInspecting()) {
            return false;
        }

        // //We need to have grenades!
        // if (!grenadesUnlimited && grenadeCount == 0) {
        //     return false;
        // }

        //Return.
        return true;
    }

    /// <summary>
    /// Returns true if the Character is able to melee attack.
    /// </summary>
    public bool CanPlayAnimationMelee() {
        if (this.IsHealing()) {
            return false;
        }

        //Block.
        if (this.IsHolstered() || this.IsHolstering()) {
            return false;
        }

        //Block.
        if (this.IsMeleeing() || this.IsThrowingGrenade()) {
            return false;
        }

        //Block.
        if (this.IsReloading() || this.IsBolting()) {
            return false;
        }

        //Block.
        if (this.IsInspecting()) {
            return false;
        }

        //Return.
        return true;
    }

    /// <summary>
    /// Returns true if the character is able to holster their weapon.
    /// </summary>
    /// <returns></returns>
    public bool CanPlayAnimationHolster() {
        if (this.IsHealing()) {
            return false;
        }

        //Block.
        if (this.IsMeleeing() || this.IsThrowingGrenade()) {
            return false;
        }

        //Block.
        if (this.IsReloading() || this.IsBolting()) {
            return false;
        }

        //Block.
        if (this.IsInspecting()) {
            return false;
        }

        //Return.
        return true;
    }

    /// <summary>
    /// Returns true if the Character can change their Weapon.
    /// </summary>
    /// <returns></returns>
    public bool CanChangeWeapon() {
        if (this.IsHealing()) {
            return false;
        }

        //Block.
        if (this.IsHolstering()) {
            return false;
        }

        //Block.
        if (this.IsMeleeing() || this.IsThrowingGrenade()) {
            return false;
        }

        //Block.
        if (this.IsReloading() || this.IsBolting()) {
            return false;
        }

        //Block.
        if (this.IsInspecting()) {
            return false;
        }

        //Return.
        return true;
    }

    /// <summary>
    /// Returns true if the Character can play the Inspect animation.
    /// </summary>
    public bool CanPlayAnimationInspect() {
        if (this.IsHealing()) {
            return false;
        }

        //Block.
        if (this.IsHolstered() || this.IsHolstering()) {
            return false;
        }

        //Block.
        if (this.IsMeleeing() || this.IsThrowingGrenade()) {
            return false;
        }

        //Block.
        if (this.IsReloading() || this.IsBolting()) {
            return false;
        }

        //Block.
        if (this.IsInspecting()) {
            return false;
        }

        //Return.
        return true;
    }

    /// <summary>
    /// Returns true if the Character can Aim.
    /// </summary>
    /// <returns></returns>
    public bool CanAim() {
        if (this.IsHealing()) {
            return false;
        }

        //Block.
        if (this.IsHolstered() || this.IsInspecting()) {
            return false;
        }

        //Block.
        if (this.IsMeleeing() || this.IsThrowingGrenade()) {
            return false;
        }

        //Block.
        if (this.IsHolstering()) {
            return false;
        }

        //Return.
        return true;
    }

    /// <summary>
    /// Returns true if the character can run.
    /// </summary>
    /// <returns></returns>
    public bool CanRun() {
        if (this.IsHealing()) {
            return false;
        }

        //Block.
        if (this.IsInspecting() || this.IsBolting()) {
            return false;
        }

        //No running while crouching.
        if (this.movement.IsCrouching()) {
            return false;
        }

        //Block.
        if (this.IsMeleeing() || this.IsThrowingGrenade()) {
            return false;
        }

        //Block.
        if (this.IsReloading() || this.aiming) {
            return false;
        }

        // //While trying to fire, we don't want to run. We do this just in case we do fire.
        // if (holdingButtonFire && this.equippedWeapon.HasAmmunition()) {
        //     return false;
        // }

        //This blocks running backwards, or while fully moving sideways.
        if (this.axisMovement.y <= 0 || Math.Abs(Mathf.Abs(this.axisMovement.x) - 1) < 0.01f) {
            return false;
        }

        //Return.
        return true;
    }
}