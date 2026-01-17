namespace Game.Services.Photon {
    using System.Threading.Tasks;
    using global::Photon.Realtime;
    using JetBrains.Annotations;

    /// <summary>
    /// Connection result info object.
    /// </summary>
    public class ConnectResult {
        /// <summary>
        /// Is successful
        /// </summary>
        public bool Success;
        /// <summary>
        /// The fail reason code
        /// </summary>
        public ConnectFailReason FailReason;
        /// <summary>
        /// Another custom code that can be filled by out by RealtimeClient.DisconnectCause for example.
        /// </summary>
        public DisconnectCause DisconnectCause;
        /// <summary>
        /// A debug message.
        /// </summary>
        public string DebugMessage;
        /// <summary>
        /// An optional task to signal the menu to wait until cleanup operation have completed (e.g. level unloading).
        /// </summary>
        public Task WaitForCleanup;

        /// <summary>
        /// Creates a successful result.
        /// </summary>
        /// <returns>Initialized result object</returns>
        [PublicAPI, MustUseReturnValue]
        public static ConnectResult Ok() {
            return new ConnectResult { Success = true };
        }

        /// <summary>
        /// Create a failed result.
        /// </summary>
        /// <param name="failReason">Fail reason <see cref="FailReason"/></param>
        /// <param name="debugMessage">Debug message</param>
        /// <param name="waitForCleanup">Should the receiving code wait until the connection or the scene has been cleaned up.</param>
        /// <returns></returns>
        [PublicAPI, MustUseReturnValue]
        public static ConnectResult Fail(ConnectFailReason failReason, string debugMessage = null, Task waitForCleanup = null) {
            return new ConnectResult {
                Success        = false,
                FailReason     = failReason,
                DebugMessage   = debugMessage,
                WaitForCleanup = waitForCleanup
            };
        }
    }
}