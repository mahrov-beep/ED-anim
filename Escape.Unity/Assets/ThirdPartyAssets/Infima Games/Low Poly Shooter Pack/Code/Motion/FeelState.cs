//Copyright 2022, Infima Games. All Rights Reserved.

using System;
using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    using Sirenix.OdinInspector;

    /// <summary>
    /// FeelState. Contains information on different things that happen in a single state.
    /// </summary>
    [Serializable]
    public struct FeelState
    {
        #region PROPERTIES

        /// <summary>
        /// Offset.
        /// </summary>
        public FeelStateOffset Offset => offset;
        /// <summary>
        /// SwayData.
        /// </summary>
        public SwayData SwayData => swayData;

        /// <summary>
        /// JumpingCurves.
        /// </summary>
        public ACurves JumpingCurves => jumpingCurves;
        /// <summary>
        /// FallingCurves.
        /// </summary>
        public ACurves FallingCurves => fallingCurves;
        /// <summary>
        /// LandingCurves.
        /// </summary>
        public ACurves LandingCurves => landingCurves;
        
        #endregion
        
        #region FIELDS SERIALIZED
        
        [Title("Offset")]
        
        [Tooltip("Offset.")]
        [SerializeField, InlineEditor]
        public FeelStateOffset offset;
        
        [Title("Sway Data")]
        
        [Tooltip("Settings relating to sway.")]
        [SerializeField, InlineEditor]
        public SwayData swayData;
        
        [Title("Jumping Curves")]

        [Tooltip("Animation curves played when the character jumps.")]
        [SerializeField, InlineEditor]
        public ACurves jumpingCurves;
        
        [Title("Falling Curves")]
        
        [Tooltip("Animation curves played when the character falls.")]
        [SerializeField, InlineEditor]
        public ACurves fallingCurves;
        
        [Title("Landing Curves")]

        [Tooltip("Animation curves played when the character lands.")]
        [SerializeField, InlineEditor]
        public ACurves landingCurves;
        
        #endregion
    }
}