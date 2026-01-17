#if UNITY_EDITOR && ODIN_INSPECTOR

using Sirenix.OdinInspector.Editor.Validation;
using Multicast.Numerics;

[assembly: RegisterValidator(typeof(ProtectedIntValidator))]

namespace Multicast.Numerics {
    using Sirenix.OdinInspector.Editor.Validation;

    public class ProtectedIntValidator : ValueValidator<ProtectedInt> {
        protected override void Validate(ValidationResult result) {
            if (this.Value.IsValid) {
                return;
            }

            result.ResultType = ValidationResultType.Error;
            result.Message    = "ProtectedInt is corrupted";
        }
    }
}

#endif