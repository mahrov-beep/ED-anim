namespace Game.UI.Views.World {
    using UniMob.UI;
    using Multicast;
    using Sirenix.OdinInspector;
    using UnityEngine;
    using UnityEngine.UI;

    public class DynamicAimView : AutoView<IDynamicAimState> {
        [SerializeField, Required] private WorldView targetAimWorldView;
        [SerializeField, Required] private WorldView forwardAimWorldView;
        [SerializeField, Required] private RectTransform aimFocusParent;
        [SerializeField] private GameObject healingIndicator;
        [SerializeField] private Image healingIndicatorFill;
        [SerializeField] private bool hideAimFocusWhileHealing = true;
        
        [SerializeField] private int aimFocusMinSize = 100;
        [SerializeField] private Vector3 worldOffset = Vector3.zero;
        [SerializeField, Min(0f)] private float worldSpreadScale = 1f;
        private float pixelsPerWorldUnit = 8f; //2 - base  все что выше это подкрутил
        private float aimPercentVisualMult = 4f;
        private float aimFocusBaseWidth;
        private float aimFocusMinScale;
        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("bullets", () => this.State.Bullets, 10),
            this.Variable("max_bullets", () => this.State.MaxBullets, 100),
            this.Variable("shooting_spread", () => this.State.ShootingSpread, 4f),
            this.Variable("aim_percent", () => this.State.AimPercent, 0.7f),
            this.Variable("has_target", () => this.State.HasTarget, false),
            this.Variable("is_reloading", () => this.State.IsReloading, false),
            this.Variable("is_target_blocked", () => this.State.IsTargetBlocked, false),
            this.Variable("deactivated", () => this.State.Deactivated),
            this.Variable("items_quality", () => this.State.ItemsQuality),
            this.Variable("items_quality_key", () => this.State.ItemsQualityKey),
        };

        protected override void Activate() {
            base.Activate();

            this.aimFocusParent.localScale = Vector3.one;
            this.aimFocusBaseWidth = this.aimFocusParent.rect.width;
            this.aimFocusMinScale = this.aimFocusBaseWidth > Mathf.Epsilon
                ? this.aimFocusMinSize / this.aimFocusBaseWidth
                : 0f;

            if (this.State.NeedToSetAimPosition) {
                this.targetAimWorldView.SetTarget(() => this.State.TargetAimWorldPos + this.worldOffset);
                this.forwardAimWorldView.SetTarget(() => this.State.ForwardAimWorldPos + this.worldOffset);
            }
        }

        protected override void Render() {
            base.Render();

            var isHealing = this.State.IsHealing;            
            if (this.healingIndicator != null) {
                this.healingIndicator.SetActive(isHealing);                
                if (isHealing && this.healingIndicatorFill != null) {                    
                    this.healingIndicatorFill.fillAmount = Mathf.Clamp01(this.State.HealingProgress);
                }
            }

            var allowAimWidgets = !this.hideAimFocusWhileHealing || !isHealing;

            var canSeeFocus = State.HasTarget && !State.IsReloading && allowAimWidgets;
            aimFocusParent.gameObject.SetActive(canSeeFocus);
            if (canSeeFocus) {
                var targetScale = this.CalculateFocusScale();
                this.aimFocusParent.localScale = Vector3.one * targetScale;
            }
            
            this.targetAimWorldView.gameObject.SetActive(this.State.TargetAimActive && allowAimWidgets);
            this.forwardAimWorldView.gameObject.SetActive(allowAimWidgets && this.State.NeedToSetAimPosition);
        }

        protected override void Deactivate() {
            this.targetAimWorldView.SetTarget(null);
            this.forwardAimWorldView.SetTarget(null);

            base.Deactivate();
        }

        private float CalculateFocusScale() {
            var aimPercent = Mathf.Clamp01(this.State.AimPercent);
            var spread = Mathf.Max(0f, this.State.ShootingSpread) + aimPercentVisualMult * (1f - aimPercent);
            var radius = spread * Mathf.Max(0f, this.worldSpreadScale);

            if (radius <= Mathf.Epsilon || this.pixelsPerWorldUnit <= Mathf.Epsilon || this.aimFocusBaseWidth <= Mathf.Epsilon) {
                return Mathf.Max(this.aimFocusMinScale, 0f);
            }

            var widthInPixels = radius * this.pixelsPerWorldUnit * 2f;
            var scale = widthInPixels / this.aimFocusBaseWidth;
            return Mathf.Max(this.aimFocusMinScale, scale);
        }

    }

    public interface IDynamicAimState : IViewState {
        Vector3 TargetAimWorldPos  { get; }
        Vector3 ForwardAimWorldPos { get; }

        float Bullets        { get; }
        float MaxBullets     { get; }
        float ShootingSpread { get; }
        float AimPercent     { get; }
        float ItemsQuality   { get; }

        string ItemsQualityKey { get; }

        bool HasTarget            { get; }
        bool TargetAimActive      { get; }
        bool NeedToSetAimPosition { get; }
        bool IsReloading          { get; }
        bool IsTargetBlocked      { get; }
        bool Deactivated          { get; }
        bool IsHealing            { get; }
        float HealingProgress     { get; }
    }
}