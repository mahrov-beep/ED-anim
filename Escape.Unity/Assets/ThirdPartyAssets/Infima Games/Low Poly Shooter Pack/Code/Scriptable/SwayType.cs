//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    using Sirenix.OdinInspector;

    /// <summary>
    /// SwayType. Holds information on horizontal, vertical SwayDirection values for SwayMotion components to use.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_ST_Default", menuName = "Infima Games/Low Poly Shooter Pack/Sway Type")]
    public class SwayType : ScriptableObject
    {
        #region PROPERTIES

        /// <summary>
        /// Horizontal.
        /// </summary>
        public SwayDirection Horizontal => horizontal;
        /// <summary>
        /// Vertical.
        /// </summary>
        public SwayDirection Vertical => vertical;
        
        #endregion
        
        #region FIELDS SERIALIZED
        
        [Title("Horizontal")]
        
        [Tooltip("Horizontal Sway.")]
        [SerializeField]
        private SwayDirection horizontal;

        [Title("Vertical")]
        
        [Tooltip("Vertical Sway.")]
        [SerializeField]
        private SwayDirection vertical;
        
        #endregion
    }
}