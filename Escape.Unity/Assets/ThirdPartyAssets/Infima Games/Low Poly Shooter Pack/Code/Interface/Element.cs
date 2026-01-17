//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.LowPolyShooterPack.Interface
{
    /// <summary>
    /// Interface Element.
    /// </summary>
    public abstract class Element : MonoBehaviour
    {
        #region FIELDS
        
        /// <summary>
        /// Player Character.
        /// </summary>
        protected Character characterBehaviour;
        /// <summary>
        /// Player Character Inventory.
        /// </summary>
        protected InventoryBehaviour inventoryBehaviour;

        /// <summary>
        /// Equipped Weapon.
        /// </summary>
        protected WeaponBehaviour equippedWeaponBehaviour;
        
        #endregion

        #region UNITY

        /// <summary>
        /// Awake.
        /// </summary>
        protected virtual void Awake()
        {
            //Get Player Character.
            characterBehaviour = FindObjectOfType<Character>();
            //Get Player Character Inventory.
            inventoryBehaviour = characterBehaviour.GetInventory();
        }
        
        /// <summary>
        /// Update.
        /// </summary>
        private void Update()
        {
            //Ignore if we don't have an Inventory.
            if (Equals(inventoryBehaviour, null))
                return;

            //Get Equipped Weapon.
            equippedWeaponBehaviour = inventoryBehaviour.GetEquipped();
            
            //Tick.
            Tick();
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Tick.
        /// </summary>
        protected virtual void Tick() {}

        #endregion
    }
}