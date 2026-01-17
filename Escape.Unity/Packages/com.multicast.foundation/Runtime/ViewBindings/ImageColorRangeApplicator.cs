namespace Multicast {
    using System;
    using CodeWriter.ViewBinding;
    using TriInspector;
    using UnityEngine;
    using UnityEngine.UI;

    [DrawWithTriInspector]
    [AddComponentMenu("View Binding/UI/[Binding] Image Color Range Applicator")]
    public class ImageColorRangeApplicator : ApplicatorBase {
        [SerializeField, TableList] private ColorValuePair[] colorValuePairs;

        [SerializeField, Required] private Image target;

        [SerializeField] private ViewVariableFloat source;

        private void Start() {
            if (this.colorValuePairs.Length > 0) {
                this.target.color = this.colorValuePairs[^1].color;
            }
        }

        protected override void Apply() {
            var currentValue = this.source.Value;

            for (var i = 0; i < this.colorValuePairs.Length - 1; i++) {
                if (currentValue <= this.colorValuePairs[i + 1].rangeValue && currentValue >= this.colorValuePairs[i].rangeValue) {
                    var progress = currentValue - this.colorValuePairs[i].rangeValue;
                    var range    = this.colorValuePairs[i + 1].rangeValue - this.colorValuePairs[i].rangeValue;
                    var f        = progress / range;
                    this.target.color = Color.Lerp(this.colorValuePairs[i].color, this.colorValuePairs[i + 1].color, f);
                    return;
                }
            }
        }
    }

    [Serializable]
    internal class ColorValuePair {
        public float rangeValue;
        public Color color;
    }
}