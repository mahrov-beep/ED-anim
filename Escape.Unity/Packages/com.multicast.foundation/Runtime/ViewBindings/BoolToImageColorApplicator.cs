namespace Multicast {
    using CodeWriter.ViewBinding;
    using TriInspector;
    using UnityEngine;
    using UnityEngine.UI;

    [DrawWithTriInspector]
    [AddComponentMenu("View Binding/UI/[Binding] Bool To Image Color Applicator")]
    public class BoolToImageColorApplicator : ApplicatorBase {
        [SerializeField] private Color trueColor;
        [SerializeField] private Color falseColor;

        [Required, SerializeField] private Image target;

        [SerializeField] private ViewVariableBool source;

        protected override void Apply() {
            var currentValue = this.source.Value;
            this.target.color = currentValue ? this.trueColor : this.falseColor;
        }
    }
}