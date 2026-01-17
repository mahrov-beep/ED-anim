namespace Game.Services.Photon {
    using System;
    using global::Photon.Realtime;

    /// <summary>
    /// Arguments to connect to Photon.
    /// </summary>
    [Serializable]
    public struct PhotonGameConnectArgs {
        /// <summary>
        /// The session that the client wants to join. Is not persisted. Use ReconnectionInformation instead to recover it between application shutdowns.
        /// </summary>
        public string Session;

        /// <summary>
        /// The actual region that the client will connect to.
        /// </summary>
        public string Region;

        /// <summary>
        /// Toggle to create or join-only game sessions/rooms.
        /// </summary>
        public bool Creating;

        /// <summary>
        /// Set to true to try to perform a reconnect. <see cref="ReconnectInformation"/> must be available then.
        /// </summary>
        public bool Reconnecting;
    }
}