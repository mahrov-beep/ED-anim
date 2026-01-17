//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    using Unity.Cinemachine;

    /// <summary>
    /// Character Abstract Behaviour.
    /// </summary>
    public abstract class CharacterBehaviour : MonoBehaviour
    {
        #region UNITY

        /// <summary>
        /// Awake.
        /// </summary>
        protected virtual void Awake(){}
        /// <summary>
        /// Start.
        /// </summary>
        protected virtual void Start(){}

        /// <summary>
        /// Update.
        /// </summary>
        protected virtual void Update(){}
        /// <summary>
        /// LateUpdate.
        /// </summary>
        protected virtual void LateUpdate(){}

        #endregion
        
        #region GETTERS

        /// <summary>
        /// Returns character type.
        /// </summary>
        /// <returns></returns>
        public abstract CharacterTypes GetCharacterType();

        public abstract CharacterConfig GetConfig();

        public abstract Animator GetCharacterAnimator();

        public abstract CharacterAudioPlayer GetAudioPlayer();

        /// <summary>
        /// This function should return the amount of shots that the character has fired in succession.
        /// Using this value for applying recoil, and for modifying spread is what we have this function for.
        /// </summary>
        /// <returns></returns>
        public abstract int GetShotsFired();
        /// <summary>
        /// Returns true when the character's weapons are lowered.
        /// </summary>
        public abstract bool IsLowered();

        /// <summary>
        /// Returns the player character's main camera.
        /// </summary>
        public abstract CinemachineCamera GetCameraWorld();
        /// <summary>
        /// Returns the player character's weapon camera.
        /// </summary>
        /// <returns></returns>
        public abstract Camera GetCameraDepth();
        
        /// <summary>
        /// Returns a reference to the Inventory component.
        /// </summary>
        public abstract InventoryBehaviour GetInventory();

        /// <summary>
        /// Returns true if the character is running.
        /// </summary>
        public abstract bool IsRunning();
        /// <summary>
        /// Returns true if the character has a weapon that is holstered in their hands.
        /// </summary>
        public bool IsHolstered() => this.GetCharacterAnimator().GetBool(AHashes.Holstered);

        public bool IsHolstering() => this.GetCharacterAnimator().GetBool(AHashes.Holstering);

        /// <summary>
        /// Returns true if the character is crouching.
        /// </summary>
        public abstract bool IsCrouching();

        /// <summary>
        /// Returns true if the character is currently healing.
        /// </summary>
        public virtual bool IsHealing() => false;

        /// <summary>
        /// Returns healing progress normalized [0..1].
        /// </summary>
        public virtual float GetHealingProgress01() => 0f;

        /// <summary>
        /// Returns true if the character is reloading.
        /// </summary>
        public bool IsReloading() => this.GetCharacterAnimator().GetBool(AHashes.Reloading);

        public bool IsBolting() => this.GetCharacterAnimator().GetBool(AHashes.Bolt);

        /// <summary>
        /// Returns true if the character is throwing a grenade.
        /// </summary>
        public bool IsThrowingGrenade() => this.GetCharacterAnimator().GetBool(AHashes.Grenading);

        /// <summary>
        /// Returns true if the character is meleeing.
        /// </summary>
        public bool IsMeleeing() => this.GetCharacterAnimator().GetBool(AHashes.Meleeing);

        // Состояние полностью контролируется из SMB в аниматоре,
        // из кода происходит только получение текущего значения
        public CharacterFullBodyStates GetFullBodyState() => (CharacterFullBodyStates)this.GetCharacterAnimator().GetInteger(AHashes.FUllBodyState);
        
        /// <summary>
        /// Returns true if the character is aiming.
        /// </summary>
        public abstract bool IsAiming();

        /// <summary>
        /// Returns the Movement Input.
        /// </summary>
        public abstract Vector2 GetInputMovement();
        /// <summary>
        /// Returns the Look Input.
        /// </summary>
        public abstract Vector2 GetInputLook();

        /// <summary>
        /// Returns true if the character is inspecting.
        /// </summary>
        public bool IsInspecting() => this.GetCharacterAnimator().GetBool(AHashes.Inspecting);
        
        #endregion

        #region ANIMATION

        /// <summary>
        /// Ejects a casing from the equipped weapon.
        /// </summary>
        public abstract void EjectCasing();
        /// <summary>
        /// Fills the character's equipped weapon's ammunition by a certain amount, or fully if set to -1.
        /// </summary>
        public abstract void FillAmmunition(int amount);

        /// <summary>
        /// Throws a grenade.
        /// </summary>
        public abstract void Grenade();
        /// <summary>
        /// Sets the equipped weapon's magazine to be active or inactive!
        /// </summary>
        public abstract void SetActiveMagazine(int active);

        /// <summary>
        /// Sets the equipped weapon's slide back pose.
        /// </summary>
        public abstract void SetSlideBack(int back);

        public abstract void SetActiveKnife(int active);

        #endregion
    }
}