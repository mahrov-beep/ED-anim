namespace Multicast.Build.PreBuildSteps {
    using System;
    using Sirenix.OdinInspector;
    using UnityEngine;

    [Serializable]
    public class LunarConsoleSetEnabled : PreBuildStep {
        [SerializeField]
        private bool enabled;

        public override void PreBuild(BuildContext context) {
#if LUNAR_CONSOLE
            LunarConsoleEditorInternal.Installer.SetLunarConsoleEnabled(this.enabled);
#else
            Debug.LogError("Lunar Console plugin not exist in project. Suggestion: Add LUNAR_CONSOLE define");
#endif
        }

        public override void Cleanup(BuildContext context) {
#if LUNAR_CONSOLE
            LunarConsoleEditorInternal.Installer.SetLunarConsoleEnabled(true);
#endif
        }

        public override object GetInspector() => new Inspector(this);

        private class Inspector {
            private readonly LunarConsoleSetEnabled step;

            public Inspector(LunarConsoleSetEnabled step) => this.step = step;

            [ShowInInspector, DisplayAsString, EnableGUI]
            public bool LunarConsoleEnabled => this.step.enabled;
        }
    }
}