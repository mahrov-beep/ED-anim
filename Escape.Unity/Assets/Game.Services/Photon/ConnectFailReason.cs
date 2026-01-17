namespace Game.Services.Photon {
    /// <summary>
    /// Is used to convey some information about a connection error back to the caller.
    /// </summary>
    public enum ConnectFailReason {
        /// <summary>
        /// No reason code available.
        /// </summary>
        None = 0,
        /// <summary>
        /// User requested cancellation or disconnect.
        /// </summary>
        UserRequest = 1,
        /// <summary>
        /// App or Editor closed
        /// </summary>
        ApplicationQuit = 2,
        /// <summary>
        /// Connection disconnected.
        /// </summary>
        Disconnect = 3,

        /// <summary>
        /// The connection to Photon servers failed.
        /// </summary>
        ConnectingFailed = 10,
        /// <summary>
        /// The Quantum map asset was not found.
        /// </summary>
        MapNotFound = 11,
        /// <summary>
        /// The scene loading failed.
        /// </summary>
        LoadingFailed = 12,
        /// <summary>
        /// Starting the runner failed.
        /// </summary>
        RunnerFailed = 13,
        /// <summary>
        /// Plugin disconnected.
        /// </summary>
        PluginError = 14,
        /// <summary>
        /// AppId not set.
        /// </summary>
        NoAppId = 15,
        /// <summary>
        /// AuthValues is not set.
        /// </summary>
        NoAuthValues = 101,
        /// <summary>
        /// RuntimePlayer is not set.
        /// </summary>
        NoRuntimePlayer = 103,
        // RuntimeConfig is not set.
        NoRuntimeConfig = 104,
        /// <summary>
        /// UserId returned by server is null.
        /// </summary>
        UserIdRejectedByServer = 120,
    }
}