//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    using Sirenix.OdinInspector;

    /// <summary>
	/// Handles all the animation events that come from the character in the asset.
	/// </summary>
	public class CharacterAnimationEventHandler : MonoBehaviour {
        [SerializeField, Required] private CharacterBehaviour playerCharacter;

		#region ANIMATION
        /// <summary>
		/// Ejects a casing from the character's equipped weapon. This function is called from an Animation Event.
		/// </summary>
		private void OnEjectCasing()
		{
			//Notify the character.
			playerCharacter.EjectCasing();
		}

		/// <summary>
		/// Fills the character's equipped weapon's ammunition by a certain amount, or fully if set to 0. This function is called
		/// from a Animation Event.
		/// </summary>
		private void OnAmmunitionFill(int amount = 0)
		{
			//Notify the character.
			playerCharacter.FillAmmunition(amount);
		}
		/// <summary>
		/// Sets the character's knife active value. This function is called from an Animation Event.
		/// </summary>
		private void OnSetActiveKnife(int active)
		{
			//Notify the character.
			playerCharacter.SetActiveKnife(active);
		}
		
		/// <summary>
		/// Spawns a grenade at the correct location. This function is called from an Animation Event.
		/// </summary>
		private void OnGrenade()
		{
			//Notify the character.
			playerCharacter.Grenade();
		}
		/// <summary>
		/// Sets the equipped weapon's magazine to be active or inactive! This function is called from an Animation Event.
		/// </summary>
		private void OnSetActiveMagazine(int active)
		{
			//Notify the character.
			playerCharacter.SetActiveMagazine(active);
		}

		/// <summary>
		/// Bolt Animation Ended. This function is called from an Animation Event.
		/// </summary>
		private void OnAnimationEndedBolt()
		{
		}
		/// <summary>
		/// Reload Animation Ended. This function is called from an Animation Event.
		/// </summary>
		private void OnAnimationEndedReload()
		{
		}

		/// <summary>
		/// Grenade Throw Animation Ended. This function is called from an Animation Event.
		/// </summary>
		private void OnAnimationEndedGrenadeThrow()
		{
		}
		/// <summary>
		/// Melee Animation Ended. This function is called from an Animation Event.
		/// </summary>
		private void OnAnimationEndedMelee()
		{
		}

		/// <summary>
		/// Inspect Animation Ended. This function is called from an Animation Event.
		/// </summary>
		private void OnAnimationEndedInspect()
		{
		}
		/// <summary>
		/// Holster Animation Ended. This function is called from an Animation Event.
		/// </summary>
		private void OnAnimationEndedHolster()
		{
		}

		/// <summary>
		/// Sets the character's equipped weapon's slide back pose. This function is called from an Animation Event.
		/// </summary>
		private void OnSlideBack(int back)
		{
			//Notify the character.
			playerCharacter.SetSlideBack(back);
		}

        private void OnDropMagazine()
        {
        }

        private void EquipWeapon() 
        {
        }

        private void EquipRifle() 
        {
        }


        private void HideWeapon()
        {
        }

        private void HolsterRifle() 
        {
        }


        #endregion
    }   
}