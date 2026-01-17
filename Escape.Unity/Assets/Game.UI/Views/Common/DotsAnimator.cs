namespace Game.UI.Views.Common {
    using UniMob;
    using UnityEngine;

    public sealed class DotsAnimator {
        private static readonly string[] DotsSuffixes = new[] { "", ".", "..", "..." };

        private readonly MutableAtom<string> dotsAtom = Atom.Value("");
        private double startTime;
        private bool wasActive;

        public string Value => this.dotsAtom.Value;

        public void Activate(bool isActive) {
            this.startTime = Time.unscaledTimeAsDouble;
            this.wasActive = isActive;

            if (!isActive) {
                this.dotsAtom.Value = "";
            }
        }

        public void Update(bool isActive) {
            if (isActive) {
                if (!this.wasActive) {
                    this.startTime = Time.unscaledTimeAsDouble;
                }

                const float animationSpeed = 4f;
                this.dotsAtom.Value = DotsSuffixes[((int)((Time.unscaledTimeAsDouble - this.startTime) * animationSpeed)) % DotsSuffixes.Length];
            } else {
                this.dotsAtom.Value = "";
            }

            this.wasActive = isActive;
        }
    }
}
