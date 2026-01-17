namespace Multicast.Build {
    using System;
    using Sirenix.OdinInspector;
    using UnityEditor;
    using UnityEngine;

    [Serializable]
    public abstract class BuildStep {
        [SerializeField]
        [ToggleLeft, HorizontalGroup("BuildStepHeader")]
        protected bool buildStepEnabled = true;

        [SerializeField]
        [LabelWidth(40), LabelText("Mode"), HorizontalGroup("BuildStepHeader")]
        protected BuildStepMode buildStepMode = BuildStepMode.Replace;

        [SerializeField]
        protected float buildStepOrder = 0;

        public virtual string Name => ObjectNames.NicifyVariableName(this.GetType().Name);

        public bool IsBuildStepEnabled => this.buildStepEnabled;

        public BuildStepMode StepMode => this.buildStepMode;

        public float? StepOrder => Mathf.Approximately(this.buildStepOrder, 0f) ? null : this.buildStepOrder;

        public virtual object GetInspector() => null;
    }

    [Serializable]
    public abstract class PreBuildStep : BuildStep {
        public abstract void PreBuild(BuildContext context);

        public virtual void Cleanup(BuildContext context) {
        }
    }

    [Serializable]
    public abstract class PostBuildStep : BuildStep {
        public abstract void PostBuild(BuildContext context);
    }

    [Serializable]
    public class BuildScriptingDefineSymbol {
        [SerializeField]
        [HorizontalGroup("Symbol", 10), HideLabel]
        public bool enabled;

        [SerializeField]
        [HorizontalGroup("Symbol"), HideLabel]
        public string symbol;
    }

    public enum BuildStepMode {
        Append  = 0,
        Replace = 1,
    }

    public class BuildContext {
        public BuildContext(BuildTarget buildTarget, int buildCode) {
            this.BuildTarget      = buildTarget;
            this.BuildCode        = buildCode;
            this.BuildTargetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
        }

        public BuildTarget      BuildTarget      { get; }
        public BuildTargetGroup BuildTargetGroup { get; }
        public int              BuildCode        { get; }

        public bool BuildFailed { get; internal set; }
    }
}