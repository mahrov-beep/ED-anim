namespace Scripts.Editor.PreBuildSteps {
    using System;
    using Multicast.Build;
    using Photon.Deterministic;
    using Quantum;
    using Quantum.Allocator;
    using Sirenix.OdinInspector;
    using UnityEditor;
    using UnityEngine;

    [Serializable]
    public class QuantumSetup : PreBuildStep {
        [SerializeField]
        private SimulationConfig[] simulationConfigs = Array.Empty<SimulationConfig>();

        [SerializeField]
        private QuantumDeterministicSessionConfigAsset[] sessionConfigs = Array.Empty<QuantumDeterministicSessionConfigAsset>();

        [SerializeField, InlineProperty, HideLabel]
        private Settings settings = new Settings();

        [Serializable]
        private class Settings {
            /// <summary>
            /// How often we should send checksums of the frame state to the server for verification (useful during development, set to zero for release). Defined in frames.
            /// </summary>
            public int ChecksumInterval = 60;

            /// <summary>
            /// This allows Quantum frame checksumming to be deterministic across different runtime platforms, however it comes with quite a cost and should only be used during debugging.
            /// </summary>
            public bool ChecksumCrossPlatformDeterminism;

            /// <summary>
            /// How long to store checksumed verified frames. The are used to generate a frame dump in case of a checksum error happening. Not used in Replay and Local mode. Default is 3.
            /// </summary>
            public float ChecksumSnapshotHistoryLengthSeconds = 3;

            /// <summary>
            /// Additional options for checksum dumps, if the default settings don't provide a clear picture. 
            /// </summary>
            public SimulationConfigChecksumErrorDumpOptions ChecksumErrorDumpOptions;

            /// <summary>
            /// If and to which extent allocations in the Frame Heap should be tracked when in Debug mode.
            /// Recommended modes for development is `DetectLeaks`.
            /// While actively debugging a memory leak,`TraceAllocations` mode can be enabled (warning: tracing is very slow).
            /// </summary>
            public HeapTrackingMode HeapTrackingMode = HeapTrackingMode.DetectLeaks;
        }

        public override void PreBuild(BuildContext context) {
            this.Execute();
        }

        [Button]
        private void Execute() {
            foreach (var simulationConfig in this.simulationConfigs) {
                simulationConfig.ChecksumSnapshotHistoryLengthSeconds = FP.FromFloat_UNSAFE(this.settings.ChecksumSnapshotHistoryLengthSeconds);
                simulationConfig.ChecksumErrorDumpOptions             = this.settings.ChecksumErrorDumpOptions;
                simulationConfig.HeapTrackingMode                     = this.settings.HeapTrackingMode;

                EditorUtility.SetDirty(simulationConfig);
            }

            foreach (var sessionConfig in this.sessionConfigs) {
                sessionConfig.Config.ChecksumInterval                 = this.settings.ChecksumInterval;
                sessionConfig.Config.ChecksumCrossPlatformDeterminism = this.settings.ChecksumCrossPlatformDeterminism;

                EditorUtility.SetDirty(sessionConfig);
            }
        }

        public override object GetInspector() => new Inspector(this);

        private class Inspector {
            private readonly QuantumSetup step;

            public Inspector(QuantumSetup step) => this.step = step;

            [ShowInInspector, ListDrawerSettings(ShowFoldout = false, IsReadOnly = true)]
            public SimulationConfig[] SimulationConfigs => this.step.simulationConfigs;

            [ShowInInspector, ListDrawerSettings(ShowFoldout = false, IsReadOnly = true)]
            public QuantumDeterministicSessionConfigAsset[] SessionConfigs => this.step.sessionConfigs;

            [ShowInInspector, InlineProperty(LabelWidth = 300), HideLabel]
            public Settings Settings => this.step.settings;
        }
    }
}