namespace CodeWriter.ViewBinding.Applicators.UI {
    using TriInspector;
    using UnityEngine;
    using UnityEngine.UI;

    [DrawWithTriInspector]
    [AddComponentMenu("View Binding/UI/[Binding] Image Color Lerp Applicator")]
    public class ImageColorLerpApplicator : ComponentApplicatorBase<Image, ViewVariableFloat> {
        [SerializeField] private Color leftColor  = Color.black;
        [SerializeField] private Color rightColor = Color.white;

        [SerializeField] private float leftValue  = 0;
        [SerializeField] private float rightValue = 1;

        protected override void Apply(Image target, ViewVariableFloat source) {
            var lerp = Mathf.InverseLerp(this.leftValue, this.rightValue, source.Value);

            target.color = Color.Lerp(this.leftColor, this.rightColor, lerp);
        }
    }
}