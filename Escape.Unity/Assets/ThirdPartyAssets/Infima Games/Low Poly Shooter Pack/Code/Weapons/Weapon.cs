//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    using Legacy;
    using Multicast.Pools;
    using Sirenix.OdinInspector;

    /// <summary>
    /// Weapon. This class handles most of the things that weapons need.
    /// </summary>
    public class Weapon : WeaponBehaviour
    {
        #region FIELDS SERIALIZED
        
        [Title("Firing")]
        
        [Tooltip("Is this weapon bolt-action? If yes, then a bolt-action animation will play after every shot.")]
        [SerializeField]
        private bool boltAction;

        [Title("Reloading")]
        
        [Tooltip("Determines if this weapon reloads in cycles, meaning that it inserts one bullet at a time, or not.")]
        [SerializeField]
        private bool cycledReload;

        [Title("Animation")]

        [Tooltip("Transform that represents the weapon's ejection port, meaning the part of the weapon that casings shoot from.")]
        [SerializeField]
        private Transform socketEjection;

        [Title("Resources")]

        [Tooltip("Casing Prefab.")]
        [SerializeField]
        private GameObject prefabCasing;
        
        [Tooltip("Projectile Prefab. This is the prefab spawned when the weapon shoots.")]
        [SerializeField]
        private GameObject prefabProjectile;
        
        [Tooltip("The AnimatorController a player character needs to use while wielding this weapon.")]
        [SerializeField, Required] 
        public RuntimeAnimatorController controller;
        
        [SerializeField, Required]
        public RuntimeAnimatorController controllerThirdPerson;
        
        [Title("Audio Clips Holster")]

        [Tooltip("Holster Audio Clip.")]
        [SerializeField]
        private AudioClip audioClipHolster;

        [Tooltip("Unholster Audio Clip.")]
        [SerializeField]
        private AudioClip audioClipUnholster;
        
        [Title("Audio Clips Reloads")]

        [Tooltip("Reload Audio Clip.")]
        [SerializeField]
        private AudioClip audioClipReload;
        
        [Tooltip("Reload Empty Audio Clip.")]
        [SerializeField]
        private AudioClip audioClipReloadEmpty;
        
        [Title("Audio Clips Reloads Cycled")]
        
        [Tooltip("Reload Open Audio Clip.")]
        [SerializeField]
        private AudioClip audioClipReloadOpen;
        
        [Tooltip("Reload Insert Audio Clip.")]
        [SerializeField]
        private AudioClip audioClipReloadInsert;
        
        [Tooltip("Reload Close Audio Clip.")]
        [SerializeField]
        private AudioClip audioClipReloadClose;
        
        [Title("Audio Clips Other")]

        [Tooltip("AudioClip played when this weapon is fired without any ammunition.")]
        [SerializeField]
        private AudioClip audioClipFireEmpty;
        
        [Tooltip("")]
        [SerializeField]
        private AudioClip audioClipBoltAction;

        #endregion

        #region FIELDS

        private ItemAnimationDataBehaviour itemAnimationDataBehaviour;

        /// <summary>
        /// Weapon Animator.
        /// </summary>
        private Animator animator;
        /// <summary>
        /// Attachment Manager.
        /// </summary>
        private WeaponAttachmentManagerBehaviour attachmentManager;

        /// <summary>
        /// The main player character behaviour component.
        /// </summary>
        private CharacterBehaviour characterBehaviour;

        /// <summary>
        /// The player character's camera.
        /// </summary>
        private Transform playerCamera;
        
        #endregion

        #region UNITY
        
        protected override void Awake()
        {
            this.itemAnimationDataBehaviour = this.GetComponent<ItemAnimationDataBehaviour>();
            //Get Animator.
            animator = GetComponent<Animator>();
            //Get Attachment Manager.
            attachmentManager = GetComponent<WeaponAttachmentManagerBehaviour>();
        }

        #endregion

        #region GETTERS

        public override ItemAnimationDataBehaviour GetItemAnimationData() => this.itemAnimationDataBehaviour;

        /// <summary>
        /// GetFieldOfViewMultiplierAim.
        /// </summary>
        public override float GetFieldOfViewMultiplierAim()
        {
            //Make sure we don't have any issues even with a broken setup!
            if (this.attachmentManager.GetEquippedScope() is var scopeBehaviour && scopeBehaviour) {
                return scopeBehaviour.GetFieldOfViewMultiplierAim();
            }
            
            //Error.
            Debug.LogError("Weapon has no scope equipped!");
  
            //Return.
            return 1.0f;
        }
        /// <summary>
        /// GetFieldOfViewMultiplierAimWeapon.
        /// </summary>
        public override float GetFieldOfViewMultiplierAimWeapon()
        {
            //Make sure we don't have any issues even with a broken setup!
            if (this.attachmentManager.GetEquippedScope() is var scopeBehaviour && scopeBehaviour) {
                return scopeBehaviour.GetFieldOfViewMultiplierAimWeapon();
            }

            //Error.
            Debug.LogError("Weapon has no scope equipped!");
  
            //Return.
            return 1.0f;
        }
        
        /// <summary>
        /// GetAnimator.
        /// </summary>
        public override Animator GetAnimator() => animator;

        /// <summary>
        /// GetAudioClipHolster.
        /// </summary>
        public override AudioClip GetAudioClipHolster() => audioClipHolster;
        /// <summary>
        /// GetAudioClipUnholster.
        /// </summary>
        public override AudioClip GetAudioClipUnholster() => audioClipUnholster;

        /// <summary>
        /// GetAudioClipReload.
        /// </summary>
        public override AudioClip GetAudioClipReload() => audioClipReload;
        /// <summary>
        /// GetAudioClipReloadEmpty.
        /// </summary>
        public override AudioClip GetAudioClipReloadEmpty() => audioClipReloadEmpty;
        
        /// <summary>
        /// GetAudioClipReloadOpen.
        /// </summary>
        public override AudioClip GetAudioClipReloadOpen() => audioClipReloadOpen;
        /// <summary>
        /// GetAudioClipReloadInsert.
        /// </summary>
        public override AudioClip GetAudioClipReloadInsert() => audioClipReloadInsert;
        /// <summary>
        /// GetAudioClipReloadClose.
        /// </summary>
        public override AudioClip GetAudioClipReloadClose() => audioClipReloadClose;

        /// <summary>
        /// GetAudioClipFireEmpty.
        /// </summary>
        public override AudioClip GetAudioClipFireEmpty() => audioClipFireEmpty;
        /// <summary>
        /// GetAudioClipBoltAction.
        /// </summary>
        public override AudioClip GetAudioClipBoltAction() => audioClipBoltAction;

        /// <summary>
        /// GetAudioClipFire.
        /// </summary>
        public override AudioClip GetAudioClipFire() => this.attachmentManager.GetEquippedMuzzle() is var muzzleBehaviour && muzzleBehaviour
            ? muzzleBehaviour.GetAudioClipFire()
            : null;

        /// <summary>
        /// HasCycledReload.
        /// </summary>
        public override bool HasCycledReload() => cycledReload;

        /// <summary>
        /// IsBoltAction.
        /// </summary>
        public override bool IsBoltAction() => boltAction;

        /// <summary>
        /// GetAnimatorController.
        /// </summary>
        public override RuntimeAnimatorController GetAnimatorController(CharacterTypes type) => type == CharacterTypes.RemotePlayer 
            ? this.controllerThirdPerson 
            : controller;
        /// <summary>
        /// GetAttachmentManager.
        /// </summary>
        public override WeaponAttachmentManagerBehaviour GetAttachmentManager() => attachmentManager;

        #endregion

        #region METHODS

        public override void Init(CharacterBehaviour owner) {
            //Cache the player character.
            characterBehaviour = owner;

            //Cache the world camera. We use this in line traces.
            playerCamera = characterBehaviour.GetCameraWorld().transform;
            
            owner.GetCharacterAnimator().runtimeAnimatorController = this.GetAnimatorController(owner.GetCharacterType());
        }

        /// <summary>
        /// Reload.
        /// </summary>
        public override void Reload(bool emptyReload)
        {
            //Set Reloading Bool. This helps cycled reloads know when they need to stop cycling.
            const string boolName = "Reloading";
            animator.SetBool(boolName, true);

            this.characterBehaviour.GetAudioPlayer().PlayOneShot(CharacterAudioLayers.Reload,
                clip: !emptyReload ? this.GetAudioClipReload() : this.GetAudioClipReloadEmpty()
            );

            //Play Reload Animation.
            animator.Play(cycledReload ? "Reload Open" : (!emptyReload ? "Reload" : "Reload Empty"), 0, 0.0f);
        }
        /// <summary>
        /// Fire.
        /// </summary>
        public override void Fire(WeaponFireData fireData, float spreadMultiplier = 1.0f)
        {
            //Play the firing animation.
            const string stateName = "Fire";
            animator.Play(stateName, 0, 0.0f);

            this.characterBehaviour.GetAudioPlayer().PlayOneShot(
                CharacterAudioLayers.Fire,
                clip: this.GetAudioClipFire()
            );

            //Play all muzzle effects.
            if (this.attachmentManager.GetEquippedMuzzle() is var muzzleBehaviour && muzzleBehaviour) {
                muzzleBehaviour.Effect();
            }

            if (this.prefabProjectile && this.prefabProjectile.TryGetComponent<Projectile>(out _)) {
                var fromOrigin = this.attachmentManager.GetEquippedMuzzle() is var muzzle && muzzle
                    ? muzzle.GetSocket()
                    : this.attachmentManager.GetMuzzleSocket();
                var projectileObject = GameObjectPool.Instantiate(this.prefabProjectile, fromOrigin.position, fromOrigin.rotation);
                var projectile       = projectileObject.GetComponent<Projectile>();
                var shotOrigin = fireData.FromPosition != default ? fireData.FromPosition : fromOrigin.position;
                projectile.Setup(shotOrigin, fireData.HitPoint, fireData.HitNormal);
            }
            else {
                Debug.LogError($"Projectile '{this.prefabProjectile}' has not Projectile component!", this.prefabProjectile);
            }

            // //Spawn as many projectiles as we need.
            // for (var i = 0; i < shotCount; i++)
            // {
            //     //Determine a random spread value using all of our multipliers.
            //     Vector3 spreadValue = Random.insideUnitSphere * (spread * spreadMultiplier);
            //     //Remove the forward spread component, since locally this would go inside the object we're shooting!
            //     spreadValue.z = 0;
            //     //Convert to world space.
            //     spreadValue = playerCamera.TransformDirection(spreadValue);
            //
            //     //Spawn projectile from the projectile spawn point.
            //     GameObject projectile = Instantiate(prefabProjectile, playerCamera.position, Quaternion.Euler(playerCamera.eulerAngles + spreadValue));
            //     
            //     // Link projectile to owner.
            //     projectile.GetComponent<Projectile>().Init(characterBehaviour);
            //
            //     if (this.characterBehaviour.GetComponentInParent<Collider>() is {} ownerCollider && 
            //         ownerCollider != null &&
            //         projectile.TryGetComponent(out Collider projectileCollider)) {
            //         //Ignore the main player character's collision. A little hacky, but it should work.
            //         Physics.IgnoreCollision(ownerCollider, projectileCollider);
            //     }
            //
            //     //Add velocity to the projectile.
            //     projectile.GetComponent<Rigidbody>().linearVelocity = projectile.transform.forward * projectileImpulse;
            // }
        }

        /// <summary>
        /// SetSlideBack.
        /// </summary>
        public override void SetSlideBack(int back)
        {
            //Set the slide back bool.
            const string boolName = "Slide Back";
            animator.SetBool(boolName, back != 0);
        }

        /// <summary>
        /// EjectCasing.
        /// </summary>
        public override void EjectCasing()
        {
            //Spawn casing prefab at spawn point.
            if(prefabCasing != null && socketEjection != null)
                Instantiate(prefabCasing, socketEjection.position, socketEjection.rotation);
        }

        #endregion
    }
}