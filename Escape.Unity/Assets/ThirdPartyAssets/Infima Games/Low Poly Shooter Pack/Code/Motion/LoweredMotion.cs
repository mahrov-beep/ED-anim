//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    using Sirenix.OdinInspector;

    /// <summary>
    /// LoweredMotion. This class drives the procedural offsets that lower a weapon.
    /// </summary>
    public class LoweredMotion : Motion
    {
        #region FIELDS SERIALIZED

        [Title("References Character")]
        
        [Tooltip("The character's CharacterBehaviour component.")]
        [SerializeField, Required]
        private CharacterBehaviour characterBehaviour;

        #endregion
        
        #region FIELDS
        
        /// <summary>
        /// Lowered Spring Location. Used to get the GameObject into a changed lowered
        /// pose.
        /// </summary>
        private readonly Spring loweredSpringLocation = new Spring();
        /// <summary>
        /// Recoil Spring Rotation. Used to get the GameObject into a changed lowered
        /// pose.
        /// </summary>
        private readonly Spring loweredSpringRotation = new Spring();

        /// <summary>
        /// LowerData for the current equipped weapon. If there's none, then there's no lowering, I guess.
        /// </summary>
        private LowerData lowerData;
        
        #endregion
        
        #region METHODS

        /// <summary>
        /// Tick.
        /// </summary>
        public override void Tick()
        {
            //Check References.
            if (characterBehaviour == null)
            {
                //ReferenceError.
                Log.ReferenceError(this, gameObject);

                //Return.
                return;
            }

            var equippedWeapon = this.characterBehaviour.GetInventory().GetEquipped();
            if (equippedWeapon == null) {
                return;
            }

            //Get ItemAnimationDataBehaviour.
            var animationData = equippedWeapon.GetComponent<ItemAnimationDataBehaviour>();
            if (animationData == null)
                return;
            
            //Get LowerData.
            lowerData = animationData.GetLowerData();
            if (lowerData == null)
                return;

            //Update Location Value.
            loweredSpringLocation.UpdateEndValue(characterBehaviour.IsLowered() ? lowerData.LocationOffset : default);
            //Update Rotation Value.
            loweredSpringRotation.UpdateEndValue(characterBehaviour.IsLowered() ? lowerData.RotationOffset : default);
        }
        
        #endregion
        
        #region FUNCTIONS

        /// <summary>
        /// GetLocation.
        /// </summary>
        public override Vector3 GetLocation()
        {
            if (lowerData == null)
            {
                return default;
            }
            
            //Return.
            return loweredSpringLocation.Evaluate(lowerData.Interpolation);
        }
        /// <summary>
        /// GetEulerAngles.
        /// </summary>
        public override Vector3 GetEulerAngles()
        {
            if (lowerData == null)
            {
                return default;
            }
            
            //Return.
            return loweredSpringRotation.Evaluate(lowerData.Interpolation);
        }
        
        #endregion
    }
}