namespace Game.Services.Photon {
    using System;
    using System.Collections.Generic;
    using global::Photon.Client;
    using global::Photon.Realtime;
    using Quantum;

    /// <summary>
    /// Quantum specific arguments to start the Quantum simulation after connecting to Photon.
    /// </summary>
    [Serializable]
    [RequireFieldsInit(Required = new[] {
        nameof(AuthValues), nameof(RuntimeConfig), nameof(RuntimePlayer),
        nameof(CustomRoomProperties), nameof(CustomLobbyProperties)
    })]
    public class PhotonGameSetupArgs {
        /// <summary>
        /// The values for user authentication.
        /// </summary>
        public AuthenticationValues AuthValues;

        /// <summary>
        /// The runtime config of the Quantum simulation. Every client sends theirs to the server.
        /// </summary>
        public RuntimeConfig RuntimeConfig;

        /// <summary>
        /// The RuntimePlayer which are automatically added to the simulation after is started.
        /// </summary>
        public RuntimePlayer RuntimePlayer;

        public PhotonHashtable CustomRoomProperties;

        public string[] CustomLobbyProperties;

        /// <summary>
        /// The app version used for the Photon connection.
        /// </summary>
        public string AppVersion;

        /// <summary>
        /// The max player count that the user selected in the menu.
        /// </summary>
        public int MaxPlayerCount;

        /// <summary>
        /// Start Quantum game in recording mode.
        /// </summary>
        public RecordingFlags RecordingFlags = RecordingFlags.None;

        /// <summary>
        /// How to update the session using <see cref="SimulationUpdateTime"/>. 
        /// Default is EngineDeltaTime.
        /// </summary>
        public SimulationUpdateTime DeltaTimeType = SimulationUpdateTime.EngineDeltaTime;

        /// <summary>
        /// A client timeout for the Quantum start game protocol, measured in seconds.
        /// Large snapshots and/or slow webhooks could make this go above the default value of 10 sec. Configure this value appropriately.
        /// </summary>
        public float StartGameTimeoutInSeconds = SessionRunner.Arguments.DefaultStartGameTimeoutInSeconds;

        /// <summary>
        /// Manual configuration of <see cref="SessionRunner.Arguments.GameFlags"/> used when starting the Quantum simulation.
        /// </summary>
        public int GameFlags;
    }
}