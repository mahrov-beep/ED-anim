//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    public abstract class WeaponBehaviour : MonoBehaviour
    {
        #region UNITY

        /// <summary>
        /// Awake.
        /// </summary>
        protected virtual void Awake() {}

        /// <summary>
        /// Start.
        /// </summary>
        protected virtual void Start(){}

        /// <summary>
        /// Update.
        /// </summary>
        protected virtual void Update(){}

        /// <summary>
        /// Late Update.
        /// </summary>
        protected virtual void LateUpdate(){}

        #endregion

        #region GETTERS

        /// <summary>
        /// Returns the holster audio clip.
        /// </summary>
        public abstract AudioClip GetAudioClipHolster();
        /// <summary>
        /// Returns the unholster audio clip.
        /// </summary>
        public abstract AudioClip GetAudioClipUnholster();

        /// <summary>
        /// Returns the reload audio clip.
        /// </summary>
        public abstract AudioClip GetAudioClipReload();
        /// <summary>
        /// Returns the reload empty audio clip.
        /// </summary>
        public abstract AudioClip GetAudioClipReloadEmpty();
        
        /// <summary>
        /// Returns the reload open audio clip.
        /// </summary>
        public abstract AudioClip GetAudioClipReloadOpen();
        /// <summary>
        /// Returns the reload insert audio clip.
        /// </summary>
        public abstract AudioClip GetAudioClipReloadInsert();
        /// <summary>
        /// Returns the reload close audio clip.
        /// </summary>
        public abstract AudioClip GetAudioClipReloadClose();

        /// <summary>
        /// Returns the fire empty audio clip.
        /// </summary>
        public abstract AudioClip GetAudioClipFireEmpty();
        /// <summary>
        /// Returns the bolt action audio clip.
        /// </summary>
        public abstract AudioClip GetAudioClipBoltAction();

        /// <summary>
        /// Returns the fire audio clip.
        /// </summary>
        public abstract AudioClip GetAudioClipFire();

        /// <summary>
        /// Determines if this Weapon reloads in cycles.
        /// </summary>
        public abstract bool HasCycledReload();

        /// <summary>
        /// Returns the Weapon's Animator component.
        /// </summary>
        public abstract Animator GetAnimator();

        /// <summary>
        /// Returns true if this is a bolt-action weapon.
        /// </summary>
        public abstract bool IsBoltAction();

        /// <summary>
        /// Returns the field of view multiplier when aiming.
        /// </summary>
        public abstract float GetFieldOfViewMultiplierAim();
        /// <summary>
        /// Returns the field of view multiplier when aiming for the weapon camera.
        /// </summary>
        public abstract float GetFieldOfViewMultiplierAimWeapon();

        /// <summary>
        /// Returns the RuntimeAnimationController the Character needs to use when this Weapon is equipped!
        /// </summary>
        public abstract RuntimeAnimatorController GetAnimatorController(CharacterTypes type);
        /// <summary>
        /// Returns the weapon's attachment manager component.
        /// </summary>
        public abstract WeaponAttachmentManagerBehaviour GetAttachmentManager();
        
        public abstract ItemAnimationDataBehaviour GetItemAnimationData();

        #endregion

        #region METHODS

        public abstract void Init(CharacterBehaviour owner);

        /// <summary>
        /// Fires the weapon.
        /// </summary>
        /// <param name="fireData">Fire data.</param>
        /// <param name="spreadMultiplier">Value to multiply the weapon's spread by. Very helpful to account for aimed spread multipliers.</param>
        public abstract void Fire(WeaponFireData fireData, float spreadMultiplier = 1.0f);
        /// <summary>
        /// Reloads the weapon.
        /// </summary>
        public abstract void Reload(bool emptyReload);

        /// <summary>
        /// Sets the slide back pose.
        /// </summary>
        public abstract void SetSlideBack(int back);

        /// <summary>
        /// Ejects a casing from the weapon. This is commonly called from animation events, but can be called from anywhere.
        /// </summary>
        public abstract void EjectCasing();

        #endregion
    }
}