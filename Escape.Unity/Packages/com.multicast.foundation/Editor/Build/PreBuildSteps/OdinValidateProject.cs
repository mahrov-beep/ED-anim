namespace Multicast.Build.PreBuildSteps {
    using System;
    using System.Linq;
    using System.Text;
    using Sirenix.OdinInspector.Editor.Validation;
    using Sirenix.OdinValidator.Editor;
    using UnityEditor;
    using UnityEditor.Build;

    [Serializable]
    public class OdinValidateProject : PreBuildStep {
        public override void PreBuild(BuildContext context) => Execute();

        [MenuItem("Build/Build Step/Odin Validate Project")]
        public static void Execute() {
            var profile = ValidationProfile.MainValidationProfile;

            using var session = new ValidationSession(profile);

            var errors = session
                .ValidateEverythingEnumerator(showProgressBar: true)
                .Where(it => it.ResultType == ValidationResultType.Error);

            var errorCount = 0;
            var errorSb    = new StringBuilder();

            foreach (var result in errors) {
                ++errorCount;

                errorSb.Append(errorCount).Append(": ").Append(result.Message).Append(" at ").Append(result.Path).AppendLine();
            }

            if (errorCount > 0) {
                throw new BuildFailedException($"Odin validation failed:{Environment.NewLine}{errorSb}");
            }
        }
    }
}