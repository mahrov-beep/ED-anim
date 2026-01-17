namespace Multicast.SafeArea {
    using Sirenix.OdinInspector;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.EventSystems;

    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    public class RectFilter : UIBehaviour {
        [SerializeField]
        [ListDrawerSettings(
            ShowFoldout      = false,
            HideAddButton    = true,
            HideRemoveButton = true,
            IsReadOnly       = true,
            DraggableItems   = false)]
        private RectFilterExtender[] extenders;

        private readonly SafeAreaOffset             insets = new SafeAreaOffset();
        private          CanvasScaler               canvasScaler;
        private          DrivenRectTransformTracker tracker = new DrivenRectTransformTracker();
        private          RectTransform              rect;

        public RectTransform RectTransform => this.rect ? this.rect : (this.rect = this.GetComponent<RectTransform>());

        public CanvasScaler CanvasScaler {
            get {
                if (!this.canvasScaler) {
                    this.canvasScaler = this.gameObject.GetComponentInParent<CanvasScaler>();
                }

                return this.canvasScaler;
            }
        }

        public float Scale {
            get {
                var screen    = new Vector2(Screen.width, Screen.height);
                var reference = this.CanvasScaler.referenceResolution;
                switch (this.CanvasScaler.screenMatchMode) {
                    case CanvasScaler.ScreenMatchMode.MatchWidthOrHeight:
                        return Mathf.Pow(2f, Mathf.Lerp(
                            Mathf.Log(screen.x / reference.x, 2f),
                            Mathf.Log(screen.y / reference.y, 2f), this.CanvasScaler.matchWidthOrHeight));

                    case CanvasScaler.ScreenMatchMode.Expand:
                        return Mathf.Min(screen.x / reference.x, screen.y / reference.y);

                    case CanvasScaler.ScreenMatchMode.Shrink:
                        return Mathf.Max(screen.x / reference.x, screen.y / reference.y);

                    default:
                        return 1f;
                }
            }
        }

        public float ScaleByHeight => Screen.height / this.CanvasScaler.referenceResolution.y;
        public float ScaleByWidth  => Screen.width / this.CanvasScaler.referenceResolution.x;

        protected override void OnEnable() {
            base.OnEnable();
            this.canvasScaler = null;
            this.UpdateRect();
        }

        protected override void OnDisable() {
            base.OnDisable();
            this.tracker.Clear();
            this.canvasScaler = null;
        }

        protected override void OnTransformParentChanged() {
            base.OnTransformParentChanged();
            this.canvasScaler = null;
            this.UpdateRect();
        }

        protected override void OnRectTransformDimensionsChange() {
            base.OnRectTransformDimensionsChange();
            this.UpdateRect();
        }

        public void RefreshExtenders() {
            this.extenders = this.GetComponents<RectFilterExtender>();
        }

        [Button]
        public void UpdateRect() {
            if (!this.IsActive())
                return;

            if (!this.CanvasScaler) {
                Debug.LogWarning("No CanvasScaler is parents");
                return;
            }

            this.tracker.Clear();

            DrivenTransformProperties properties = 0;

            bool lockX = false, lockY = false;
            foreach (var extender in this.extenders) {
                lockX |= extender.LockX;
                lockY |= extender.LockY;
            }

            if (lockX) {
                properties |= DrivenTransformProperties.SizeDeltaX;
                properties |= DrivenTransformProperties.AnchorMaxX;
                properties |= DrivenTransformProperties.AnchorMinX;
                properties |= DrivenTransformProperties.PivotX;
                properties |= DrivenTransformProperties.AnchoredPositionX;
            }

            if (lockY) {
                properties |= DrivenTransformProperties.SizeDeltaY;
                properties |= DrivenTransformProperties.AnchorMaxY;
                properties |= DrivenTransformProperties.AnchorMinY;
                properties |= DrivenTransformProperties.PivotY;
                properties |= DrivenTransformProperties.AnchoredPositionY;
            }

            this.tracker.Add(this, (RectTransform) this.transform, properties);

            this.insets.top    = 0;
            this.insets.bottom = 0;
            this.insets.left   = 0;
            this.insets.right  = 0;

            foreach (var extender in this.extenders) {
                extender.ApplyOffset(this.insets);
            }

            var center = new Vector2(this.insets.left - this.insets.right, this.insets.bottom - this.insets.top) * 0.5f;
            var size   = new Vector2(-this.insets.Horizontal, -this.insets.Vertical);

            var anchorMin = this.RectTransform.anchorMin;
            var anchorMax = this.RectTransform.anchorMax;
            var position  = this.RectTransform.anchoredPosition;
            var sizeDelta = this.RectTransform.sizeDelta;
            var pivot     = this.RectTransform.pivot;

            if (lockX) {
                anchorMin.x = 0;
                anchorMax.x = 1;
                position.x  = center.x;
                sizeDelta.x = size.x;
                pivot.x     = 0.5f;
            }

            if (lockY) {
                anchorMin.y = 0;
                anchorMax.y = 1;
                position.y  = center.y;
                sizeDelta.y = size.y;
                pivot.y     = 0.5f;
            }

            this.RectTransform.anchorMin        = anchorMin;
            this.RectTransform.anchorMax        = anchorMax;
            this.RectTransform.anchoredPosition = position;
            this.RectTransform.sizeDelta        = sizeDelta;
            this.RectTransform.pivot            = pivot;
        }
    }
}