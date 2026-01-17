namespace Game.Services.Photon {
    using System;

    /// <summary>
    /// Includes Photon ping regions result used by the Party menu to pre select the best region and encode the region into the party code.
    /// </summary>
    [Serializable, RequireFieldsInit]
    public struct PhotonOnlineRegion {
        /// <summary>
        /// Photon region code.
        /// </summary>
        public string Code;
        /// <summary>
        /// Last ping result.
        /// </summary>
        public int Ping;
    }
}