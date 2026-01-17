namespace Multicast {
    using CodeWriter.ViewBinding;
    using TriInspector;
    using UnityEngine;

    [DrawWithTriInspector]
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("View Binding/UI/[Binding] Rect Transform Delta Size Applicator")]
    public class RectTransformDeltaSizeApplicator : ApplicatorBase {
        [Required, SerializeField] private RectTransform target;
        [Required, SerializeField] private RectTransform targetParent;

        [SerializeField] private ViewVariableFloat source;

        [SerializeField] private bool    scaleWidth;
        [SerializeField] private bool    scaleHeight;
        [SerializeField] private Vector2 minDeltaSize;

        protected override void Apply() {
            this.UpdateSize();
        }

        private void UpdateSize() {
            var currentValue = this.source.Value;

            var sizeDelta = this.target.sizeDelta;

            var rect      = this.targetParent.rect;
            var maxHeight = rect.height;
            var maxWidth  = rect.width;

            var width  = this.scaleWidth ? Mathf.Max(maxWidth * currentValue, this.minDeltaSize.x) : sizeDelta.x;
            var height = this.scaleHeight ? Mathf.Max(maxHeight * currentValue, this.minDeltaSize.y) : sizeDelta.y;

            this.target.sizeDelta = new Vector2(width, height);
        }
    }
}