//Copyright 2022, Infima Games. All Rights Reserved.

using System;
using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace InfimaGames.LowPolyShooterPack
{
    using Sirenix.OdinInspector;
    using Unity.Cinemachine;
    using Unity.Collections;

    /// <summary>
	/// Main Character Component. This component handles the most important functions of the character, and interfaces
	/// with basically every part of the asset, it is the hub where it all converges.
	/// </summary>
	public sealed partial class Character : CharacterBehaviour {
        [SerializeField] private int  ammunitionCurrent = 10;
        [SerializeField] private int  ammunitionTotal   = 10;
        [SerializeField] private bool isAutomatic       = false;
        [SerializeField] private int  rateOfFire        = 60;

        [SerializeField] private CharacterTypes type = CharacterTypes.LocalView;

        [SerializeField, Required] private string layerActionsName         = "Layer Actions";
        [SerializeField, Required] private string layerOverlayName         = "Layer Overlay";
        [SerializeField, Required] private string layerFullBodyActionsName = "Layer FullBody Actions";

		#region FIELDS SERIALIZED

		[Title("References")]

        [SerializeField, Required]
        private Movement movement;

        [SerializeField]
        private CharacterAudioPlayer audioPlayer;

		[Tooltip("The character's LowerWeapon component.")]
		[SerializeField]
		private LowerWeapon lowerWeapon;

        [Title("Config")]
        [SerializeField, Required]
        private CharacterConfig config;
		
		[Title("Inventory")]
		
		[Tooltip("Determines the index of the weapon to equip when the game starts.")]
		[SerializeField]
		private int weaponIndexEquippedAtStart;
		
		[Tooltip("Inventory.")]
		[SerializeField, Required]
		private InventoryBehaviour inventory;

		[Title("Grenade")]

		[Tooltip("If true, the character's grenades will never run out.")]
		[SerializeField]
		private bool grenadesUnlimited;

		[Tooltip("Total amount of grenades at start.")]
		[SerializeField]
		private int grenadeTotal = 10;
		
		[Tooltip("Grenade spawn offset from the character's camera.")]
		[SerializeField]
		private float grenadeSpawnOffset = 1.0f;
		
		[Tooltip("Grenade Prefab. Spawned when throwing a grenade.")]
		[SerializeField]
		private GameObject grenadePrefab;
		
		[Title("Knife")]
		
		[Tooltip("Knife GameObject.")]
		[SerializeField]
		private GameObject knife;

		[Title("Cameras")]

		[Tooltip("Normal Camera.")]
		[SerializeField]
		private CinemachineCamera cameraWorld;

		[Tooltip("Weapon-Only Camera. Depth.")]
		[SerializeField]
		private Camera cameraDepth;
        
		[Title("Animation Procedural")]
		
		[Tooltip("Character Animator.")]
		[SerializeField]
		private Animator characterAnimator;

		[Title("Input Options")]

		[Tooltip("If true, the running input has to be held to be active.")]
		[SerializeField]
		private bool holdToRun = true;

		[Tooltip("If true, the aiming input has to be held to be active.")]
		[SerializeField]
		private bool holdToAim = true;
		
		#endregion

		#region FIELDS

		/// <summary>
		/// True if the character is aiming.
		/// </summary>
		private bool aiming;
		/// <summary>
		/// Last Frame's Aiming Value.
		/// </summary>
		private bool wasAiming;
		/// <summary>
		/// True if the character is running.
		/// </summary>
		private bool running;
		
		/// <summary>
		/// Last Time.time at which we shot.
		/// </summary>
		private float lastShotTime;
		
		/// <summary>
		/// Overlay Layer Index. Useful for playing things like firing animations.
		/// </summary>
		private int layerOverlay;
		/// <summary>
		/// Actions Layer Index. Used to play actions like reloading.
		/// </summary>
		private int layerActions;
        /// <summary>
        /// FullBodyActions Layer Index. Used to player actions like death, knock, roll.
        /// </summary>
        private int layerFullBodyActions;

		/// <summary>
		/// Alpha Aiming Value. Zero to one value representing aiming. Zero if we're not aiming, and one if we are
		/// fully aiming.
		/// </summary>
		private float aimingAlpha;

		/// <summary>
		/// Crouching Alpha. This value dictates how visible the crouching state is at any given time.
		/// </summary>
		private float crouchingAlpha;
		/// <summary>
		/// Running Alpha. This value dictates how visible the running state is at any given time.
		/// </summary>
		private float runningAlpha;

		/// <summary>
		/// Look Axis Values.
		/// </summary>
		private Vector2 axisLook;
		
		/// <summary>
		/// Look Axis Values.
		/// </summary>
		private Vector2 axisMovement;

		/// <summary>
		/// Current grenades left.
		/// </summary>
		private int grenadeCount;

		/// <summary>
		/// True if the player is holding the aiming button.
		/// </summary>
		private bool holdingButtonAim;
		/// <summary>
		/// True if the player is holding the running button.
		/// </summary>
		private bool holdingButtonRun;
		/// <summary>
		/// True if the player is holding the firing button.
		/// </summary>
		private bool holdingButtonFire;

		/// <summary>
		/// If true, the tutorial text should be visible on screen.
		/// </summary>
		private bool tutorialTextVisible;

		/// <summary>
		/// True if the game cursor is locked! Used when pressing "Escape" to allow developers to more easily access the editor.
		/// </summary>
		private bool cursorLocked;
		/// <summary>
		/// Amount of shots fired in succession. We use this value to increase the spread, and also to apply recoil
		/// </summary>
		private int shotsFired;

		#endregion

		#region UNITY

		/// <summary>
		/// Awake.
		/// </summary>
		protected override void Awake()
		{
			#region Lock Cursor

			//Always make sure that our cursor is locked when the game starts!
			cursorLocked = true;
			//Update the cursor's state.
			UpdateCursorState();

			#endregion

            this.ammunitionCurrent = this.ammunitionTotal;

			//Initialize Inventory.
			inventory.Init(weaponIndexEquippedAtStart);
		}
		/// <summary>
		/// Start.
		/// </summary>
		protected override void Start()
		{
			//Max out the grenades.
			grenadeCount = grenadeTotal;
			
			//Hide knife. We do this so we don't see a giant knife stabbing through the character's hands all the time!
			if (knife != null)
				knife.SetActive(false);
			
			//Cache a reference to the action layer's index.
			layerActions = characterAnimator.GetLayerIndex(this.layerActionsName);
			//Cache a reference to the overlay layer's index.
			layerOverlay = characterAnimator.GetLayerIndex(this.layerOverlayName);

            this.layerFullBodyActions = this.characterAnimator.GetLayerIndex(this.layerFullBodyActionsName);
        }

		/// <summary>
		/// Update.
		/// </summary>
		protected override void Update()
		{
			//Match Aim.
			aiming = holdingButtonAim && CanAim();
			//Match Run.
			running = holdingButtonRun && CanRun();

            {
                if (this.inventory.GetEquipped() is var equipped && equipped &&
                    equipped.GetAttachmentManager().GetEquippedScope() is var equippedWeaponScope && equippedWeaponScope) {
                    //Check if we're aiming.
                    switch (aiming) {
                        //Just Started.
                        case true when !wasAiming:
                            equippedWeaponScope.OnAim();
                            break;
                        //Just Stopped.
                        case false when wasAiming:
                            equippedWeaponScope.OnAimStop();
                            break;
                    }
                }
            }

            //Holding the firing button.
			if (holdingButtonFire)
			{
				//Check.
				if (CanPlayAnimationFire() && HasAmmunition() && IsAutomatic())
				{
					//Has fire rate passed.
					if (Time.time - lastShotTime > 60.0f / GetRateOfFire())
						Fire();
				}
				else
				{
					//Reset fired shots, so recoil/spread does not just stay at max when we've run out
					//of ammo already!
					shotsFired = 0;
				}
			}

			//Update Animator.
			UpdateAnimator();

			//Update Aiming Alpha. We need to get this here because we're using the Animator to interpolate the aiming value.
			aimingAlpha = characterAnimator.GetFloat(AHashes.AimingAlpha);
			
			//Interpolate the crouching alpha. We do this here as a quick and dirty shortcut, but there's definitely better ways to do this.
			crouchingAlpha = Mathf.Lerp(crouchingAlpha, movement.IsCrouching() ? 1.0f : 0.0f, Time.deltaTime * 12.0f);
			//Interpolate the running alpha. We do this here as a quick and dirty shortcut, but there's definitely better ways to do this.
			runningAlpha = Mathf.Lerp(runningAlpha, running ? 1.0f : 0.0f, Time.deltaTime * config.runningInterpolationSpeed);

			//Running Field Of View Multiplier.
			float runningFieldOfView = Mathf.Lerp(1.0f, config.fieldOfViewRunningMultiplier, runningAlpha);

            {
                if (this.inventory.GetEquipped() is var equippedWeapon && equippedWeapon) {
                    //Interpolate the world camera's field of view based on whether we are aiming or not.
                    cameraWorld.Lens.FieldOfView = Mathf.Lerp(config.fieldOfView, config.fieldOfView * equippedWeapon.GetFieldOfViewMultiplierAim(), aimingAlpha) * runningFieldOfView;
                    //Interpolate the depth camera's field of view based on whether we are aiming or not.
                    cameraDepth.fieldOfView = Mathf.Lerp(config.fieldOfViewWeapon, config.fieldOfViewWeapon * equippedWeapon.GetFieldOfViewMultiplierAimWeapon(), aimingAlpha);
                }
            }

            //Save Aiming Value.
			wasAiming = aiming;
		}

		#endregion

		#region GETTERS

        public bool IsAutomatic()   => this.isAutomatic;
        public int  GetRateOfFire() => this.rateOfFire;
        
        public bool HasAmmunition()        => this.ammunitionCurrent > 0;
        public int  GetAmmunitionCurrent() => this.ammunitionCurrent;
        public int  GetAmmunitionTotal()   => this.ammunitionTotal;
        public bool IsAmmunitionFull()     => this.ammunitionCurrent >= this.ammunitionTotal;

        public override CharacterTypes  GetCharacterType() => type;
        public override CharacterConfig GetConfig()        => config;

        public override Animator GetCharacterAnimator() => this.characterAnimator;

        public override CharacterAudioPlayer GetAudioPlayer() => this.audioPlayer;

        /// <summary>
		/// GetShotsFired.
		/// </summary>
		public override int GetShotsFired() => shotsFired;

		/// <summary>
		/// IsLowered.
		/// </summary>
		public override bool IsLowered()
		{
			//Weapons are never lowered if we don't even have a LowerWeapon component.
			if (lowerWeapon == null)
				return false;

			//Return.
			return lowerWeapon.IsLowered();
		}

		/// <summary>
		/// GetCameraWorld.
		/// </summary>
		public override CinemachineCamera GetCameraWorld() => cameraWorld;
		/// <summary>
		/// GetCameraDepth.
		/// </summary>
		/// <returns></returns>
		public override Camera GetCameraDepth() => cameraDepth;

		/// <summary>
		/// GetInventory.
		/// </summary>
		public override InventoryBehaviour GetInventory() => inventory;

		/// <summary>
		/// GetGrenadesCurrent.
		/// </summary>
		public int GetGrenadesCurrent() => grenadeCount;
		/// <summary>
		/// GetGrenadesTotal.
		/// </summary>
		public int GetGrenadesTotal() => grenadeTotal;

		/// <summary>
		/// IsRunning.
		/// </summary>
		/// <returns></returns>
		public override bool IsRunning() => running;

		/// <summary>
		/// Is Crouching.
		/// </summary>
		public override bool IsCrouching() => movement.IsCrouching();

		/// <summary>
		/// IsAiming.
		/// </summary>
		public override bool IsAiming() => aiming;
		/// <summary>
		/// IsCursorLocked.
		/// </summary>
		public bool IsCursorLocked() => cursorLocked;
		
		/// <summary>
		/// IsTutorialTextVisible.
		/// </summary>
		public bool IsTutorialTextVisible() => tutorialTextVisible;
		
		/// <summary>
		/// GetInputMovement.
		/// </summary>
		public override Vector2 GetInputMovement() => axisMovement;
		/// <summary>
		/// GetInputLook.
		/// </summary>
		public override Vector2 GetInputLook() => axisLook;
		
		/// <summary>
		/// IsHoldingButtonFire. 
		/// </summary>
		public bool IsHoldingButtonFire() => holdingButtonFire;

		#endregion

		#region METHODS

		/// <summary>
		/// Updates all the animator properties for this frame.
		/// </summary>
		private void UpdateAnimator()
		{
			#region Reload Stop

			//Check if we're currently reloading cycled.
			const string boolNameReloading = "Reloading";
			if (characterAnimator.GetBool(boolNameReloading))
			{
				//If we only have one more bullet to reload, then we can change the boolean already.
				if (GetAmmunitionTotal() - GetAmmunitionCurrent() < 1)
				{
					//Update the character animator.
					characterAnimator.SetBool(boolNameReloading, false);
					//Update the weapon animator.
                    if (this.inventory.GetEquipped() is var equippedWeapon && equippedWeapon) {
                        equippedWeapon.GetAnimator().SetBool(boolNameReloading, false);
                    }
                }	
			}

			#endregion

			//Leaning. Affects how much the character should apply of the leaning additive animation.
			float leaningValue = Mathf.Clamp01(axisMovement.y);
			characterAnimator.SetFloat(AHashes.LeaningForward, leaningValue, 0.5f, Time.deltaTime);

			//Movement Value. This value affects absolute movement. Aiming movement uses this, as opposed to per-axis movement.
			float movementValue = Mathf.Clamp01(Mathf.Abs(axisMovement.x) + Mathf.Abs(axisMovement.y));
			characterAnimator.SetFloat(AHashes.Movement, movementValue, config.dampTimeLocomotion, Time.deltaTime);

			//Aiming Speed Multiplier.
			characterAnimator.SetFloat(AHashes.AimingSpeedMultiplier, config.aimingSpeedMultiplier);
			
			//Turning Value. This determines how much of the turning animation to play based on our current look rotation.
			characterAnimator.SetFloat(AHashes.Turning, Mathf.Abs(axisLook.x), config.dampTimeTurning, Time.deltaTime);

			//Horizontal Movement Float.
			characterAnimator.SetFloat(AHashes.Horizontal, axisMovement.x, config.dampTimeLocomotion, Time.deltaTime);
			//Vertical Movement Float.
			characterAnimator.SetFloat(AHashes.Vertical, axisMovement.y, config.dampTimeLocomotion, Time.deltaTime);
			
			//Update the aiming value, but use interpolation. This makes sure that things like firing can transition properly.
			characterAnimator.SetFloat(AHashes.AimingAlpha, Convert.ToSingle(aiming), config.dampTimeAiming, Time.deltaTime);

			//Set the locomotion play rate. This basically stops movement from happening while in the air.
			const string playRateLocomotionBool = "Play Rate Locomotion";
			characterAnimator.SetFloat(playRateLocomotionBool, movement.IsGrounded() ? 1.0f : 0.0f, 0.2f, Time.deltaTime);

			#region Movement Play Rates

			//Update Forward Multiplier. This allows us to change the play rate of our animations based on our movement multipliers.
			characterAnimator.SetFloat(AHashes.PlayRateLocomotionForward, config.walkingMultiplierForward, 0.2f, Time.deltaTime);
			//Update Sideways Multiplier. This allows us to change the play rate of our animations based on our movement multipliers.
			characterAnimator.SetFloat(AHashes.PlayRateLocomotionSideways, config.walkingMultiplierSideways, 0.2f, Time.deltaTime);
			//Update Backwards Multiplier. This allows us to change the play rate of our animations based on our movement multipliers.
			characterAnimator.SetFloat(AHashes.PlayRateLocomotionBackwards, config.walkingMultiplierBackwards, 0.2f, Time.deltaTime);

			#endregion
			
			//Update Animator Aiming.
			characterAnimator.SetBool(AHashes.Aim, aiming);
			//Update Animator Running.
			characterAnimator.SetBool(AHashes.Running, running);
			//Update Animator Crouching.
			characterAnimator.SetBool(AHashes.Crouching, movement.IsCrouching());
            
            this.characterAnimator.SetBool(AHashes.Jumping, this.movement.IsJumping());
            
            var equipped = this.GetInventory().GetEquipped();
            if (equipped != null) {
	            // Sync weapon animator with character animator
	            equipped.GetAnimator().SetBool(AHashes.Reloading, this.IsReloading());
            }
		}
		/// <summary>
		/// Plays the inspect animation.
		/// </summary>
		private void Inspect()
		{
			//State.
            this.characterAnimator.SetBool(AHashes.Inspecting, true);
			//Play.
			characterAnimator.CrossFade("Inspect", 0.0f, layerActions, 0);
		}
		/// <summary>
		/// Fires the character's weapon.
		/// </summary>
		private void Fire()
		{
            if (this.inventory.GetEquipped() is var equippedWeapon && equippedWeapon == null) {
                return;
            }

            this.ammunitionCurrent -= 1;
            
			//Increase shots fired. We use this value to increase the spread, and also to apply recoil, so
			//it is very important that we keep it up to date.
			shotsFired++;
			
			//Save the shot time, so we can calculate the fire rate correctly.
			lastShotTime = Time.time;

            var hitCam = Camera.main!.transform;
            var hitPoint = Physics.Raycast(new Ray(hitCam.position, hitCam.forward), out var hit, 100) 
                ? hit.point 
                : Camera.main.transform.TransformPoint(new Vector3(0, 0, 100));
            
			//Fire the weapon! Make sure that we also pass the scope's spread multiplier if we're aiming.
            equippedWeapon.Fire(
                fireData: new WeaponFireData {
                    HasAmmoForNextShot = true,
                    HitPoint = hitPoint,
                    HitNormal = null,
                },
                spreadMultiplier: aiming && equippedWeapon.GetAttachmentManager().GetEquippedScope() is var equippedWeaponScope && equippedWeaponScope
                    ? equippedWeaponScope.GetMultiplierSpread() : 1.0f
            );

			//Play firing animation.
			const string stateName = "Fire";
			characterAnimator.CrossFade(stateName, 0.05f, layerOverlay, 0);

			//Play bolt actioning animation if needed, and if we have ammunition. We don't play this for the last shot.
            if (equippedWeapon.IsBoltAction() && HasAmmunition()) {
                this.characterAnimator.SetBool(AHashes.Bolt, true);
            }
        }
		
		/// <summary>
		/// Plays the reload animation.
		/// </summary>
		private void PlayReloadAnimation()
		{
            if (this.inventory.GetEquipped() is var equippedWeapon && equippedWeapon == null) {
                return;
            }

			#region Animation

			//Get the name of the animation state to play, which depends on weapon settings, and ammunition!
			string stateName = equippedWeapon.HasCycledReload() ? "Reload Open" :
				(HasAmmunition() ? "Reload" : "Reload Empty");
			
			//Play the animation state!
			characterAnimator.Play(stateName, layerActions, 0.0f);

			#endregion

			//Set Reloading Bool. This helps cycled reloads know when they need to stop cycling.
			characterAnimator.SetBool(AHashes.Reloading, true);
			
			//Reload.
			equippedWeapon.Reload(emptyReload: !HasAmmunition());
		}

		/// <summary>
		/// Equip Weapon Coroutine.
		/// </summary>
		private IEnumerator Equip(int index = 0)
		{
			//Only if we're not holstered, holster. If we are already, we don't need to wait.
			if(!this.IsHolstered())
			{
				//Holster.
                characterAnimator.SetBool(AHashes.Holstering, true);
                characterAnimator.SetBool(AHashes.Holstered, true);
				//Wait.
				yield return new WaitUntil(() => this.IsHolstering() == false);
			}
			//Unholster. We do this just in case we were holstered.
            characterAnimator.SetBool(AHashes.Holstered, false);
			
			//Equip The New Weapon.
			inventory.Equip(index);
		}

		private void FireEmpty()
		{
			/*
			 * Save Time. Even though we're not actually firing, we still need this for the fire rate between
			 * empty shots.
			 */
			lastShotTime = Time.time;
			//Play.
			characterAnimator.CrossFade("Fire Empty", 0.05f, layerOverlay, 0);
		}
		/// <summary>
		/// Updates the cursor state based on the value of the cursorLocked variable.
		/// </summary>
		private void UpdateCursorState()
		{
			//Update cursor visibility.
			Cursor.visible = !cursorLocked;
			//Update cursor lock state.
			Cursor.lockState = cursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
		}

		/// <summary>
		/// Plays The Grenade Throwing Animation.
		/// </summary>
		private void PlayGrenadeThrow()
		{
			//Start State.
            this.characterAnimator.SetBool(AHashes.Grenading, true);
			
			//Play Normal.
			characterAnimator.CrossFade("Grenade Throw", 0.15f,
				characterAnimator.GetLayerIndex("Layer Actions Arm Left"), 0.0f);
					
			//Play Additive.
			characterAnimator.CrossFade("Grenade Throw", 0.05f,
				characterAnimator.GetLayerIndex("Layer Actions Arm Right"), 0.0f);
		}
		/// <summary>
		/// Play The Melee Animation.
		/// </summary>
		private void PlayMelee()
		{
			//Start State.
            characterAnimator.SetBool(AHashes.Meleeing, true);
			
			//Play Normal.
			characterAnimator.CrossFade("Knife Attack", 0.05f,
				characterAnimator.GetLayerIndex("Layer Actions Arm Left"), 0.0f);
			
			//Play Additive.
			characterAnimator.CrossFade("Knife Attack", 0.05f,
				characterAnimator.GetLayerIndex("Layer Actions Arm Right"), 0.0f);
		}
		
		#region ACTION CHECKS

		/// <summary>
		/// Can Fire.
		/// </summary>
		private bool CanPlayAnimationFire()
		{
			//Block.
			if (this.IsHolstered() || this.IsHolstering())
				return false;

			//Block.
			if (this.IsMeleeing() || this.IsThrowingGrenade())
				return false;

			//Block.
			if (this.IsReloading() || this.IsBolting())
				return false;

			//Block.
			if (this.IsInspecting())
				return false;

			//Return.
			return true;
		}

		/// <summary>
		/// Determines if we can play the reload animation.
		/// </summary>
		private bool CanPlayAnimationReload()
		{
			//No reloading!
			if (this.IsReloading())
				return false;

			//No meleeing!
			if (this.IsMeleeing())
				return false;

			//Not actioning a bolt.
			if (this.IsBolting())
				return false;

			//Can't reload while throwing a grenade.
			if (this.IsThrowingGrenade())
				return false;

			//Block while inspecting.
			if (this.IsInspecting())
				return false;
			
			//Return.
			return true;
		}
		
		/// <summary>
		/// Returns true if the character is able to throw a grenade.
		/// </summary>
		private bool CanPlayAnimationGrenadeThrow()
		{
			//Block.
			if (this.IsHolstered() || this.IsHolstering())
				return false;

			//Block.
			if (this.IsMeleeing() || this.IsThrowingGrenade())
				return false;

			//Block.
			if (this.IsReloading() || this.IsBolting())
				return false;

			//Block.
			if (this.IsInspecting())
				return false;
			
			//We need to have grenades!
			if (!grenadesUnlimited && grenadeCount == 0)
				return false;
			
			//Return.
			return true;
		}

		/// <summary>
		/// Returns true if the Character is able to melee attack.
		/// </summary>
		private bool CanPlayAnimationMelee()
		{
			//Block.
			if (this.IsHolstered() || this.IsHolstering())
				return false;

			//Block.
			if (this.IsMeleeing() || this.IsThrowingGrenade())
				return false;

			//Block.
			if (this.IsReloading() || this.IsBolting())
				return false;

			//Block.
			if (this.IsInspecting())
				return false;
			
			//Return.
			return true;
		}

		/// <summary>
		/// Returns true if the character is able to holster their weapon.
		/// </summary>
		/// <returns></returns>
		private bool CanPlayAnimationHolster()
		{
			//Block.
			if (this.IsMeleeing() || this.IsThrowingGrenade())
				return false;

			//Block.
			if (this.IsReloading() || this.IsBolting())
				return false;

			//Block.
			if (this.IsInspecting())
				return false;
			
			//Return.
			return true;
		}

		/// <summary>
		/// Returns true if the Character can change their Weapon.
		/// </summary>
		/// <returns></returns>
		private bool CanChangeWeapon()
		{
			//Block.
			if (this.IsHolstering())
				return false;

			//Block.
			if (this.IsMeleeing() || this.IsThrowingGrenade())
				return false;

			//Block.
			if (this.IsReloading() || this.IsBolting())
				return false;

			//Block.
			if (this.IsInspecting())
				return false;
			
			//Return.
			return true;
		}

		/// <summary>
		/// Returns true if the Character can play the Inspect animation.
		/// </summary>
		private bool CanPlayAnimationInspect()
		{
			//Block.
			if (this.IsHolstered() || this.IsHolstering())
				return false;

			//Block.
			if (this.IsMeleeing() || this.IsThrowingGrenade())
				return false;

			//Block.
			if (this.IsReloading() || this.IsBolting())
				return false;

			//Block.
			if (this.IsInspecting())
				return false;
			
			//Return.
			return true;
		}

		/// <summary>
		/// Returns true if the Character can Aim.
		/// </summary>
		/// <returns></returns>
		private bool CanAim()
		{
			//Block.
			if (this.IsHolstered() || this.IsInspecting())
				return false;

			//Block.
			if (this.IsMeleeing() || this.IsThrowingGrenade())
				return false;

			//Block.
			if (this.IsHolstering())
				return false;
			
			//Return.
			return true;
		}
		
		/// <summary>
		/// Returns true if the character can run.
		/// </summary>
		/// <returns></returns>
		private bool CanRun()
		{
			//Block.
			if (this.IsInspecting() || this.IsBolting())
				return false;

			//No running while crouching.
			if (movement.IsCrouching())
				return false;

			//Block.
			if (this.IsMeleeing() || this.IsThrowingGrenade())
				return false;

			//Block.
			if (this.IsReloading() || aiming)
				return false;

			//While trying to fire, we don't want to run. We do this just in case we do fire.
			if (holdingButtonFire && HasAmmunition())
				return false;

			//This blocks running backwards, or while fully moving sideways.
			if (axisMovement.y <= 0 || Math.Abs(Mathf.Abs(axisMovement.x) - 1) < 0.01f)
				return false;
			
			//Return.
			return true;
		}

		#endregion

		#region INPUT

		/// <summary>
		/// Fire.
		/// </summary>
		public void OnTryFire(InputAction.CallbackContext context)
		{
			//Block while the cursor is unlocked.
			if (!cursorLocked)
				return;

			//Switch.
			switch (context)
			{
				//Started.
				case {phase: InputActionPhase.Started}:
					//Hold.
					holdingButtonFire = true;
					
					//Restart the shots.
					shotsFired = 0;
					break;
				//Performed.
				case {phase: InputActionPhase.Performed}:
					//Ignore if we're not allowed to actually fire.
					if (!CanPlayAnimationFire())
						break;
					
					//Check.
					if (HasAmmunition())
					{
						//Check.
						if (IsAutomatic())
						{
							//Reset fired shots, so recoil/spread does not just stay at max when we've run out
							//of ammo already!
							shotsFired = 0;
							
							//Break.
							break;
						}
							
						//Has fire rate passed.
						if (Time.time - lastShotTime > 60.0f / GetRateOfFire())
							Fire();
					}
					//Fire Empty.
					else
						FireEmpty();
					break;
				//Canceled.
				case {phase: InputActionPhase.Canceled}:
					//Stop Hold.
					holdingButtonFire = false;

					//Reset shotsFired.
					shotsFired = 0;
					break;
			}
		}
		/// <summary>
		/// Reload.
		/// </summary>
		public void OnTryPlayReload(InputAction.CallbackContext context)
		{
			//Block while the cursor is unlocked.
			if (!cursorLocked)
				return;
			
			//Block.
			if (!CanPlayAnimationReload())
				return;
			
			//Switch.
			switch (context)
			{
				//Performed.
				case {phase: InputActionPhase.Performed}:
					//Play Animation.
					PlayReloadAnimation();
					break;
			}
		}

		/// <summary>
		/// Inspect.
		/// </summary>
		public void OnTryInspect(InputAction.CallbackContext context)
		{
			//Block while the cursor is unlocked.
			if (!cursorLocked)
				return;
			
			//Block.
			if (!CanPlayAnimationInspect())
				return;
			
			//Switch.
			switch (context)
			{
				//Performed.
				case {phase: InputActionPhase.Performed}:
					//Play Animation.
					Inspect();
					break;
			}
		}
		/// <summary>
		/// Aiming.
		/// </summary>
		public void OnTryAiming(InputAction.CallbackContext context)
		{
			//Block while the cursor is unlocked.
			if (!cursorLocked)
				return;

			//Switch.
			switch (context.phase)
			{
				//Started.
				case InputActionPhase.Started:
					//Started.
					if(holdToAim)
						holdingButtonAim = true;
					break;
				//Performed.
				case InputActionPhase.Performed:
					//Performed.
					if (!holdToAim)
						holdingButtonAim = !holdingButtonAim;
					break;
				//Canceled.
				case InputActionPhase.Canceled:
					//Canceled.
					if(holdToAim)
						holdingButtonAim = false;
					break;
			}
		}

		/// <summary>
		/// Holster.
		/// </summary>
		public void OnTryHolster(InputAction.CallbackContext context)
		{
			//Block while the cursor is unlocked.
			if (!cursorLocked)
				return;

			//Go back if we cannot even play the holster animation.
			if (!CanPlayAnimationHolster())
				return;
			
			//Switch.
			switch (context.phase)
			{
				//Started. This is here so we unholster with a tap, instead of a hold.
				case InputActionPhase.Started:
					//Only if holstered.
					if (this.IsHolstered())
					{
						//Unholster.
                        this.characterAnimator.SetBool(AHashes.Holstered, false);
						//Holstering.
                        this.characterAnimator.SetBool(AHashes.Holstering, true);
					}
					break;
				//Performed.
				case InputActionPhase.Performed:
					//Set.
                    this.characterAnimator.SetBool(AHashes.Holstered, !this.IsHolstered());
					//Holstering.
                    this.characterAnimator.SetBool(AHashes.Holstering, true);
					break;
			}
		}
		/// <summary>
		/// Throw Grenade. 
		/// </summary>
		public void OnTryThrowGrenade(InputAction.CallbackContext context)
		{
			//Block while the cursor is unlocked.
			if (!cursorLocked)
				return;
			
			//Switch.
			switch (context.phase)
			{
				//Performed.
				case InputActionPhase.Performed:
					//Try Play.
					if (CanPlayAnimationGrenadeThrow())
						PlayGrenadeThrow();
					break;
			}
		}
		
		/// <summary>
		/// Melee.
		/// </summary>
		public void OnTryMelee(InputAction.CallbackContext context)
		{
			//Block while the cursor is unlocked.
			if (!cursorLocked)
				return;
			
			//Switch.
			switch (context.phase)
			{
				//Performed.
				case InputActionPhase.Performed:
					//Try Play.
					if (CanPlayAnimationMelee())
						PlayMelee();
					break;
			}
		}
		/// <summary>
		/// Run. 
		/// </summary>
		public void OnTryRun(InputAction.CallbackContext context)
		{
			//Block while the cursor is unlocked.
			if (!cursorLocked)
				return;
			
			//Switch.
			switch (context.phase)
			{
				//Performed.
				case InputActionPhase.Performed:
					//Use this if we're using run toggle.
					if(!holdToRun)
						holdingButtonRun = !holdingButtonRun;
					break;
				//Started.
				case InputActionPhase.Started:
					//Start.
					if(holdToRun)
						holdingButtonRun = true;
					break;
				//Canceled.
				case InputActionPhase.Canceled:
					//Stop.
					if(holdToRun)
						holdingButtonRun = false;
					break;
			}
		}

		/// <summary>
		/// Jump. 
		/// </summary>
		public void OnTryJump(InputAction.CallbackContext context)
		{
			//Block while the cursor is unlocked.
			if (!cursorLocked)
				return;

			//Switch.
			switch (context.phase)
			{
				//Performed.
				case InputActionPhase.Performed:
					//Jump.
					movement.Jump();
					break;
			}
		}
		/// <summary>
		/// Next Inventory Weapon.
		/// </summary>
		public void OnTryInventoryNext(InputAction.CallbackContext context)
		{
			//Block while the cursor is unlocked.
			if (!cursorLocked)
				return;
			
			//Null Check.
			if (inventory == null)
				return;
			
			//Switch.
			switch (context)
			{
				//Performed.
				case {phase: InputActionPhase.Performed}:
					//Get the index increment direction for our inventory using the scroll wheel direction. If we're not
					//actually using one, then just increment by one.
					float scrollValue = context.valueType.IsEquivalentTo(typeof(Vector2)) ? Mathf.Sign(context.ReadValue<Vector2>().y) : 1.0f;
					
					//Get the next index to switch to.
					int indexNext = scrollValue > 0 ? inventory.GetNextIndex() : inventory.GetLastIndex();
					//Get the current weapon's index.
					int indexCurrent = inventory.GetEquippedIndex();
					
					//Make sure we're allowed to change, and also that we're not using the same index, otherwise weird things happen!
					if (CanChangeWeapon() && (indexCurrent != indexNext))
						StartCoroutine(nameof(Equip), indexNext);
					break;
			}
		}
		
		public void OnLockCursor(InputAction.CallbackContext context)
		{
			//Switch.
			switch (context)
			{
				//Performed.
				case {phase: InputActionPhase.Performed}:
					//Toggle the cursor locked value.
					cursorLocked = !cursorLocked;
					//Update the cursor's state.
					UpdateCursorState();
					break;
			}
		}
		
		/// <summary>
		/// Movement.
		/// </summary>
		public void OnMove(InputAction.CallbackContext context)
		{
			//Read.
			axisMovement = cursorLocked ? context.ReadValue<Vector2>() : default;
		}
		/// <summary>
		/// Look.
		/// </summary>
		public void OnLook(InputAction.CallbackContext context)
		{
			//Read.
			axisLook = cursorLocked ? context.ReadValue<Vector2>() : default;

            //Make sure that we have a weapon.
            if (this.inventory.GetEquipped() is var equippedWeapon && equippedWeapon == null) {
                return;
            }

			//Make sure that we have a scope.
            if (equippedWeapon.GetAttachmentManager().GetEquippedScope() is var equippedWeaponScope && equippedWeaponScope == null) {
                return;
            }

			//If we're aiming, multiply by the mouse sensitivity multiplier of the equipped weapon's scope!
			axisLook *= aiming ? equippedWeaponScope.GetMultiplierMouseSensitivity() : 1.0f;
		}

		/// <summary>
		/// Called in order to update the tutorial text value.
		/// </summary>
		public void OnUpdateTutorial(InputAction.CallbackContext context)
		{
			//Switch.
			tutorialTextVisible = context switch
			{
				//Started. Show the tutorial.
				{phase: InputActionPhase.Started} => true,
				//Canceled. Hide the tutorial.
				{phase: InputActionPhase.Canceled} => false,
				//Default.
				_ => tutorialTextVisible
			};
		}

		#endregion

		#region ANIMATION EVENTS

		/// <summary>
		/// EjectCasing.
		/// </summary>
		public override void EjectCasing()
		{
			//Notify the weapon.
            if (this.inventory.GetEquipped() is var equippedWeapon && equippedWeapon) {
                equippedWeapon.EjectCasing();
            }
		}
		/// <summary>
		/// FillAmmunition.
		/// </summary>
		public override void FillAmmunition(int amount) {
            this.ammunitionCurrent = amount != 0 
                ? Mathf.Clamp(this.ammunitionCurrent += amount, 0, this.ammunitionTotal) 
                : this.GetAmmunitionTotal();
        }
		/// <summary>
		/// Grenade.
		/// </summary>
		public override void Grenade()
		{
			//Make sure that the grenade is valid, otherwise we'll get errors.
			if (grenadePrefab == null)
				return;

			//Make sure we have a camera!
			if (cameraWorld == null)
				return;
			
			//Remove Grenade.
			if(!grenadesUnlimited)
				grenadeCount--;
			
			//Get Camera Transform.
			Transform cTransform = cameraWorld.transform;
			//Calculate the throwing location.
			Vector3 position = cTransform.position;
			position += cTransform.forward * grenadeSpawnOffset;
			//Throw.
			Instantiate(grenadePrefab, position, cTransform.rotation);
		}
		/// <summary>
		/// SetActiveMagazine.
		/// </summary>
		public override void SetActiveMagazine(int active)
		{
			//Set magazine gameObject active.
            if (this.inventory.GetEquipped() is var equipped && equipped &&
                equipped.GetAttachmentManager().GetEquippedMagazine() is var equippedWeaponMagazine && equippedWeaponMagazine) {
                equippedWeaponMagazine.gameObject.SetActive(active != 0);
            }
		}

		/// <summary>
		/// SetSlideBack.
		/// </summary>
		public override void SetSlideBack(int back)
		{
			//Set slide back.
            if (this.inventory.GetEquipped() is var equippedWeapon && equippedWeapon) {
                equippedWeapon.SetSlideBack(back);
            }
		}

		/// <summary>
		/// SetActiveKnife.
		/// </summary>
		public override void SetActiveKnife(int active)
		{
			//Set Active.
			knife.SetActive(active != 0);
		}

		#endregion

		#endregion
	}
}