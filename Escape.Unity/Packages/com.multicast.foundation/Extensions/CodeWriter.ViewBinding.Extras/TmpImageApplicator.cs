#if TPM_IMAGE

namespace CodeWriter.ViewBinding.Applicators.UI {
    using CodeWriter.UI;
    using TriInspector;
    using UnityEngine;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(TMPImage))]
    [AddComponentMenu("View Binding/UI/[Binding] TmpImage Applicator")]
    public class TmpImageApplicator : ComponentApplicatorBase<TMPImage, ViewVariableString> {
        [SerializeField, Required] private string spriteNameFormat = "{0}";

        protected override void Apply(TMPImage target, ViewVariableString source) {
            var spriteName = string.Format(this.spriteNameFormat, source.Value);
            target.spriteName = spriteName;
        }
    }
}

#endif