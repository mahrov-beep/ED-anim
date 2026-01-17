//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    using Sirenix.OdinInspector;

    /// <summary>
    /// Grip.
    /// </summary>
    public class Grip : GripBehaviour
    {
        #region FIELDS SERIALIZED

        [Title("Settings")]

        [Tooltip("Sprite. Displayed on the player's interface.")]
        [SerializeField]
        private Sprite sprite;

        #endregion

        #region GETTERS

        public override Sprite GetSprite() => sprite;

        #endregion
    }
}