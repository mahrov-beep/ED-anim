//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    using Sirenix.OdinInspector;

    /// <summary>
    /// Audio Settings used to interact with the AudioManagerService.
    /// </summary>
    [System.Serializable]
    [InlineProperty(LabelWidth = 130)]
    public struct AudioSettings
    {
        /// <summary>
        /// Volume Getter.
        /// </summary>
        public float Volume => volume;

        [Tooltip("Volume.")]
        [Range(0.0f, 1.0f)]
        [SerializeField]
        private float volume;

        /// <summary>
        /// Constructor.
        /// </summary>
        public AudioSettings(float volume = 1.0f)
        {
            //Volume.
            this.volume = volume;
        }
    }
}