namespace Multicast.Build.PreBuildSteps {
    using System;
    using System.Collections.Generic;
    using System.Text;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.TestTools.TestRunner.Api;
    using UnityEngine;
    using Object = UnityEngine.Object;

    [Serializable]
    public class UnitTestsRun : PreBuildStep {
        public override void PreBuild(BuildContext context) => Execute();

        [MenuItem("Build/Build Step/Unit Tests Run")]
        public static void Execute() {
            var runner    = ScriptableObject.CreateInstance<TestRunnerApi>();
            var callbacks = new TestRunnerResultCallbacks();
            try {
                runner.RegisterCallbacks(callbacks);
                runner.Execute(new ExecutionSettings {
                    runSynchronously = true,
                    filters = new[] {
                        new Filter {
                            testMode = TestMode.EditMode,
                        },
                    },
                });
            }
            finally {
                Object.DestroyImmediate(runner);
            }

            if (callbacks.FinishResult.FailCount > 0) {
                var errorSb = new StringBuilder();

                for (var index = 0; index < callbacks.Fails.Count; index++) {
                    var item = callbacks.Fails[index];
                    var num  = index + 1;

                    errorSb.Append(num).Append(": ").Append(item.FullName).Append(" - ").Append(item.Message).AppendLine();
                }

                throw new BuildFailedException($"UnitTests failed:{Environment.NewLine}{errorSb}");
            }
        }

        private class TestRunnerResultCallbacks : ICallbacks {
            public List<ITestResultAdaptor> Fails { get; } = new();

            public ITestResultAdaptor FinishResult { get; private set; }

            public void RunStarted(ITestAdaptor testsToRun) {
            }

            public void RunFinished(ITestResultAdaptor result) {
                this.FinishResult = result;
            }

            public void TestStarted(ITestAdaptor test) {
            }

            public void TestFinished(ITestResultAdaptor result) {
                if (result.FailCount > 0 && !result.HasChildren) {
                    this.Fails.Add(result);
                }
            }
        }
    }
}