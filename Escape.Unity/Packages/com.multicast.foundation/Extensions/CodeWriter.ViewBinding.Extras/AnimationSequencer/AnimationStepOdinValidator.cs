#if UNITY_EDITOR

using Multicast;
using Sirenix.OdinInspector.Editor.Validation;

[assembly: RegisterValidator(typeof(AnimationStepOdinValidator))]

namespace Multicast {
    using System;
    using System.Reflection;
    using BrunoMikoski.AnimationSequencer;
    using CodeWriter.ViewBinding;
    using Sirenix.OdinInspector.Editor.Validation;

    public class AnimationStepOdinValidator : RootObjectValidator<AnimationSequencerController> {
        private static readonly FieldInfo StepsField = typeof(AnimationSequencerController)
            .GetField("animationSteps", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        private static readonly MethodInfo GetErrorMethod = typeof(ViewEntry)
            .GetMethod("GetErrorMessage", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        protected override void Validate(ValidationResult result) {
            var controller = this.ValueEntry.SmartValue;

            var steps = (AnimationStepBase[]) StepsField.GetValue(controller);

            for (var stepIndex = 0; stepIndex < steps.Length; stepIndex++) {
                var step = steps[stepIndex];
                switch (step) {
                    case InvokeViewEventVoidAnimationStep invokeViewEventVoidAnimationStep:
                        ValidateEntry(ref result, step, stepIndex, invokeViewEventVoidAnimationStep.ViewEvent);
                        break;

                    case SyncViewVariableAnimationStepBase syncViewVariableAnimationStep:
                        ValidateEntry(ref result, step, stepIndex, syncViewVariableAnimationStep.ViewVariable);
                        break;

                    case FillImageVariableAnimationStep fillImageVariableAnimationStep:
                        ValidateEntry(ref result, step, stepIndex, fillImageVariableAnimationStep.FillAmountFrom);
                        ValidateEntry(ref result, step, stepIndex, fillImageVariableAnimationStep.FillAmountTo);
                        break;
                }
            }
        }

        private static void ValidateEntry(ref ValidationResult result, AnimationStepBase step, int stepIndex, ViewEntry viewEntry) {
            var error = (string) GetErrorMethod.Invoke(viewEntry, Array.Empty<object>());

            if (string.IsNullOrEmpty(error)) {
                return;
            }

            result.AddError($"{step.GetDisplayNameForEditor(stepIndex)}: {error}");
        }
    }
}

#endif