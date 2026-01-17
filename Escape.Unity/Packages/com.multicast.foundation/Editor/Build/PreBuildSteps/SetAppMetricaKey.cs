namespace Multicast.Build.PreBuildSteps {
    using System;
    using Sirenix.OdinInspector;
    using UnityEngine;

    [Serializable]
    [Obsolete("SetAppMetricaKey is obsolete. Use multiple modules instead", true)]
    public class SetAppMetricaKey : PreBuildStep {
        [SerializeField, Required]
        private string apiKey;

        public override void PreBuild(BuildContext context) {
        }

        public override void Cleanup(BuildContext context) {
        }

        public override object GetInspector() => new Inspector(this);

        public class Inspector {
            public SetAppMetricaKey Step { get; }

            public Inspector(SetAppMetricaKey step) => this.Step = step;

            [InfoBox("SetAppMetricaKey is obsolete. Use multiple modules instead", InfoMessageType.Error)]
            [ShowInInspector, DisplayAsString, EnableGUI]
            public string ApiKey => this.Step.apiKey;
        }
    }
}