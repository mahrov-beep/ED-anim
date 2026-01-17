//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    using Sirenix.OdinInspector;

    /// <summary>
    /// ItemOffsets. Contains data on how an item should be offset in different states.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_IO_Default", menuName = "Infima Games/Low Poly Shooter Pack/Item Offsets", order = 0)]
    public class ItemOffsets : ScriptableObject
    {
        /// <summary>
        /// Standing Location.
        /// </summary>
        public Vector3 StandingLocation => standingLocation;
        /// <summary>
        /// Standing Rotation.
        /// </summary>
        public Vector3 StandingRotation => standingRotation;
        
        /// <summary>
        /// Aiming Location.
        /// </summary>
        public Vector3 AimingLocation => aimingLocation;
        /// <summary>
        /// Aiming Rotation.
        /// </summary>
        public Vector3 AimingRotation => aimingRotation;
        
        /// <summary>
        /// Running Location.
        /// </summary>
        public Vector3 RunningLocation => runningLocation;
        /// <summary>
        /// Running Rotation.
        /// </summary>
        public Vector3 RunningRotation => runningRotation;
        
        /// <summary>
        /// Crouching Location.
        /// </summary>
        public Vector3 CrouchingLocation => crouchingLocation;
        /// <summary>
        /// Crouching Rotation.
        /// </summary>
        public Vector3 CrouchingRotation => crouchingRotation;
        
        /// <summary>
        /// Action Location.
        /// </summary>
        public Vector3 ActionLocation => actionLocation;
        /// <summary>
        /// Action Rotation.
        /// </summary>
        public Vector3 ActionRotation => actionRotation;

        public bool EnableReloadingOffset => this.enableReloadingOffset;
        
        public Vector3 ReloadingLocation => this.reloadingLocation;
        public Vector3 ReloadingRotation => this.reloadingRotation;

        public bool EnableRollOffset => this.enableRollOffset;
        
        public Vector3 RollLocation => this.rollLocation;
        public Vector3 RollRotation => this.rollRotation;
        
        [Title("Standing Offset")]
        
        [Tooltip("Weapon bone location offset while standing.")]
        [SerializeField]
        private Vector3 standingLocation;
        
        [Tooltip("Weapon bone rotation offset while standing.")]
        [SerializeField]
        private Vector3 standingRotation;

        [Title("Aiming Offset")]
        
        [Tooltip("Weapon bone location offset while aiming.")]
        [SerializeField]
        private Vector3 aimingLocation;
        
        [Tooltip("Weapon bone rotation offset while aiming.")]
        [SerializeField]
        private Vector3 aimingRotation;
        
        [Title("Running Offset")]
        
        [Tooltip("Weapon bone location offset while running.")]
        [SerializeField]
        private Vector3 runningLocation;
        
        [Tooltip("Weapon bone rotation offset while running.")]
        [SerializeField]
        private Vector3 runningRotation;
        
        [Title("Crouching Offset")]
        
        [Tooltip("Weapon bone location offset while crouching.")]
        [SerializeField]
        private Vector3 crouchingLocation;
        
        [Tooltip("Weapon bone rotation offset while crouching.")]
        [SerializeField]
        private Vector3 crouchingRotation;
        
        [Title("Action Offset")]
        
        [Tooltip("Weapon bone location offset while performing an action (grenade, melee).")]
        [SerializeField]
        private Vector3 actionLocation;
        
        [Tooltip("Weapon bone rotation offset while performing an action (grenade, melee).")]
        [SerializeField]
        private Vector3 actionRotation;

        [Title("Reloading Offset")]

        [SerializeField]
        private bool enableReloadingOffset;
        
        [SerializeField]
        [ShowIf(nameof(enableReloadingOffset))]
        private Vector3 reloadingLocation;

        [SerializeField]
        [ShowIf(nameof(enableReloadingOffset))]
        private Vector3 reloadingRotation;

        [Title("Roll Offset")]

        [SerializeField]
        private bool enableRollOffset;

        [SerializeField]
        [ShowIf(nameof(enableRollOffset))]
        private Vector3 rollLocation;

        [SerializeField]
        [ShowIf(nameof(enableRollOffset))]
        private Vector3 rollRotation;
    }
}