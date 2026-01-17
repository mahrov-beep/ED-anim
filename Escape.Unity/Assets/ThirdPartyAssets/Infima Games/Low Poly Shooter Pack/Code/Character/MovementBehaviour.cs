//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    /// <summary>
    /// Abstract movement class. Handles interactions with the main movement component.
    /// </summary>
    public abstract class MovementBehaviour : MonoBehaviour
    {
        #region UNITY

        /// <summary>
        /// Awake.
        /// </summary>
        protected virtual void Awake(){}

        /// <summary>
        /// Start.
        /// </summary>
        protected virtual void Start(){}

        /// <summary>
        /// Update.
        /// </summary>
        protected virtual void Update(){}

        /// <summary>
        /// Fixed Update.
        /// </summary>
        protected virtual void FixedUpdate(){}

        /// <summary>
        /// Late Update.
        /// </summary>
        protected virtual void LateUpdate(){}

        #endregion

        #region GETTERS

        /// <summary>
        /// Returns the last Time.time value at which the character jumped.
        /// </summary>
        public abstract float GetLastJumpTime();

        /// <summary>
        /// Returns the character's current velocity.
        /// </summary>
        public abstract Vector3 GetVelocity();
        /// <summary>
        /// Returns true if the character is grounded.
        /// </summary>
        public abstract bool IsGrounded();
        /// <summary>
        /// Returns last frame's IsGrounded value.
        /// </summary>
        public abstract bool WasGrounded();
        
        /// <summary>
        /// Returns true if the character is jumping.
        /// </summary>
        public abstract bool IsJumping();

        /// <summary>
        /// Returns true if the character is crouching.
        /// </summary>
        public abstract bool IsCrouching();

        #endregion
    }
}