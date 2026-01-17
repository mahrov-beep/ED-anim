namespace Multicast.Build.PreBuildSteps {
    using System;
    using Sirenix.OdinInspector;
    using UnityEditor;
    using UnityEngine;

    [Serializable]
    public class SetBuildVersion : PreBuildStep {
        private const string ENV_VAR_BUILD_VERSION = "SBP_BUILD_VERSION";

        [SerializeField] private int buildCodeOffset = 0;

        public override void PreBuild(BuildContext context) {
            var buildVersion = Environment.GetEnvironmentVariable(ENV_VAR_BUILD_VERSION) ?? PlayerSettings.bundleVersion;
            var buildCode    = context.BuildCode + this.buildCodeOffset;

            PlayerSettings.bundleVersion             = buildVersion;
            PlayerSettings.Android.bundleVersionCode = buildCode;
            PlayerSettings.iOS.buildNumber           = buildCode.ToString();
        }

        public override object GetInspector() => new Inspector(this);

        public class Inspector {
            public SetBuildVersion Step { get; }

            public Inspector(SetBuildVersion step) => this.Step = step;

            [ShowInInspector, DisplayAsString, EnableGUI]
            public int BuildCodeOffset => this.Step.buildCodeOffset;
        }
    }
}