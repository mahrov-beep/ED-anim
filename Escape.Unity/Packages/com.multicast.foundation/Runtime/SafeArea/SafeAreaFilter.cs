namespace Multicast.SafeArea {
    using UnityEngine;
    using Sirenix.OdinInspector;

    public class SafeAreaFilter : RectFilterExtender {
        [SerializeField] private bool top    = true;
        [SerializeField] private bool bottom = true;
        [SerializeField] private bool left   = true;
        [SerializeField] private bool right  = true;

        [PropertySpace(SpaceBefore = 10, SpaceAfter = 10)]
        [SerializeField] private bool inverse = false;

        public override bool LockX => this.right || this.left;
        public override bool LockY => this.top || this.bottom;

        public override void ApplyOffset(SafeAreaOffset offset) {
            if (!this.isActiveAndEnabled)
                return;

            var invScale = 1f / this.Filter.Scale;

            var fullArea = ScreenSafeArea.FullArea;
            var safeArea = ScreenSafeArea.SafeArea;

            if (this.inverse) {
                invScale *= -1;
            }

            if (this.left) offset.left     += (safeArea.xMin - fullArea.xMin) * invScale;
            if (this.right) offset.right   += (fullArea.xMax - safeArea.xMax) * invScale;
            if (this.bottom) offset.bottom += (safeArea.yMin - fullArea.yMin) * invScale;
            if (this.top) offset.top       += (fullArea.yMax - safeArea.yMax) * invScale;

            if (ScreenSafeArea.BannerPlacement != BannerPlacementType.None) {
                var height = TryCalcRealBannerSize(out var realHeight)
                    ? realHeight / this.Filter.Scale
                    : 120 * this.Filter.ScaleByHeight / this.Filter.Scale;

                if (ScreenSafeArea.BannerPlacement == BannerPlacementType.Top && this.top) {
                    offset.top += height * (this.inverse ? -1 : 1);
                }
                else if (ScreenSafeArea.BannerPlacement == BannerPlacementType.Bottom && this.bottom) {
                    offset.bottom += height * (this.inverse ? -1 : 1);
                }
            }
        }

        private static bool TryCalcRealBannerSize(out float size) {
            size = default;

            if (Application.isEditor) {
                return false;
            }

            var dpi = Screen.dpi;
            if (dpi < 20 || dpi > 1000) {
                return false;
            }

            var scale          = Screen.dpi / 160;
            var screenHeightDp = Screen.height / scale;
            var iPhone         = SystemInfo.deviceModel.Contains("iPhone");
            var useSmallBanner = iPhone || screenHeightDp < 720;
            var bannerSizeDp   = useSmallBanner ? 50 : 90;

            size = (bannerSizeDp + 5) * scale;
            return true;
        }

        private Rect                 lastSafeArea;
        private BannerPlacementType? lastBannerPlacement;

        void Update() {
            if (ScreenSafeArea.SafeArea != this.lastSafeArea ||
                ScreenSafeArea.BannerPlacement != this.lastBannerPlacement) {
                this.lastSafeArea        = ScreenSafeArea.SafeArea;
                this.lastBannerPlacement = ScreenSafeArea.BannerPlacement;
                this.Filter.UpdateRect();
            }
        }

#if UNITY_EDITOR
        [ButtonGroup("Safe"), Button("None", ButtonSizes.Medium)]
        private static void SimNone() => ScreenSafeArea.SetSimulated(ScreenSafeArea.SimDevice.None);

        [ButtonGroup("Safe"), Button("iPhoneX", ButtonSizes.Medium)]
        private static void SimIPhone() => ScreenSafeArea.SetSimulated(ScreenSafeArea.SimDevice.iPhoneX);

        [ButtonGroup("Banner"), Button("Banner (None)", ButtonSizes.Medium)]
        private static void SimBannerNone() => ScreenSafeArea.BannerPlacement = BannerPlacementType.None;

        [ButtonGroup("Banner"), Button("Banner (Top)", ButtonSizes.Medium)]
        private static void SimBannerTop() => ScreenSafeArea.BannerPlacement = BannerPlacementType.Top;

        [ButtonGroup("Banner"), Button("Banner (Bottom)", ButtonSizes.Medium)]
        private static void SimBannerBottom() => ScreenSafeArea.BannerPlacement = BannerPlacementType.Bottom;
#endif
    }
}