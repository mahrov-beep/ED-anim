namespace Multicast.Build.PreBuildSteps {
    using System;
    using Sirenix.OdinInspector;
    using UnityEditor;
    using UnityEngine;

    [Serializable]
    public class SetProductName : PreBuildStep {
        [SerializeField, Required] private string productName;

        public override void PreBuild(BuildContext context) {
            PlayerSettings.productName = this.productName;
        }

        public override object GetInspector() => new Inspector(this);

        public class Inspector {
            public SetProductName Step { get; }

            public Inspector(SetProductName step) => this.Step = step;

            [ShowInInspector, DisplayAsString, EnableGUI]
            public string ProductName => this.Step.productName;
        }
    }
}