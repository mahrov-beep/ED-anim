namespace Multicast.Build.PreBuildSteps {
    using System;
    using Sirenix.OdinInspector;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEngine;

    [Serializable]
    public class SetIl2CppSettings : PreBuildStep {
        [SerializeField]
        private Il2CppCodeGeneration codeGeneration = Il2CppCodeGeneration.OptimizeSpeed;

#if UNITY_2022_3_OR_NEWER
        [SerializeField]
        private Il2CppCompilerConfiguration configuration = Il2CppCompilerConfiguration.Release;
#endif

        public override void PreBuild(BuildContext context) {
#if UNITY_2022_3_OR_NEWER
            var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(context.BuildTargetGroup);
            PlayerSettings.SetIl2CppCodeGeneration(namedBuildTarget, this.codeGeneration);
            PlayerSettings.SetIl2CppCompilerConfiguration(namedBuildTarget, this.configuration);
#else
            EditorUserBuildSettings.il2CppCodeGeneration = this.codeGeneration;
#endif
        }

        public override string Name => "Set IL2CPP Settings";

        public override object GetInspector() => new Inspector(this);

        private class Inspector {
            private readonly SetIl2CppSettings step;

            public Inspector(SetIl2CppSettings step) => this.step = step;

            [ShowInInspector, DisplayAsString, EnableGUI]
            public Il2CppCodeGeneration CodeGeneration => this.step.codeGeneration;

#if UNITY_2022_3_OR_NEWER
            [ShowInInspector, DisplayAsString, EnableGUI]
            public Il2CppCompilerConfiguration Configuration => this.step.configuration;
#endif
        }
    }
}