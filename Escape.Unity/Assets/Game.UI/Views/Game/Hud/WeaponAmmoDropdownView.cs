namespace Game.UI.Views.Game.Hud {
    using Multicast;
    using UniMob;
    using Sirenix.OdinInspector;
    using TMPro;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using UnityEngine;

    /// <summary>
    /// View для выпадающего списка типов боеприпасов. Пока отвечает только за отображение и переключение.
    /// </summary>
    public class WeaponAmmoDropdownView : AutoView<IWeaponAmmoDropdownState> {
        [SerializeField, Required] private GameObject dropdownRoot;
        [SerializeField, Required] private TextMeshProUGUI selectedCountText;
        [SerializeField, Required] private ViewPanel optionsPanel;
        [SerializeField] private RectTransform arrowTransform;
        [SerializeField] private RectTransform optionsBackground;
        [SerializeField] private float backgroundExtraHeight = 0f;
        [SerializeField] private float optionsSpacing = 6f;
        [SerializeField] private float optionsPaddingBottom = 0f;
        [SerializeField] private float arrowExpandedRotationZ = 180f;
        [SerializeField] private float arrowCollapsedRotationZ = 0f;

        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("is_expanded", () => this.State.IsExpanded, false),
            this.Variable("selected_mag_count", () => this.State.SelectedMagazineCount),
            this.Variable("selected_mag_max", () => this.State.SelectedTotalAmmoCount),
            this.Variable("selected_icon", () => this.State.SelectedIcon),
        };

        protected override AutoViewEventBinding[] Events => new[] {
            this.Event("toggle", () => this.State.Toggle()),
        };

        public void InitState() {
            if (this.HasState) {
                this.State.ResetState();
            }
        }

        protected override void Activate() {
            base.Activate();
            this.ApplyLayoutSettings();
        }

        protected override void Render() {
            base.Render();

            var optionsVisible = this.State.IsExpanded && this.State.IsSelectedWeapon;

            if (dropdownRoot != null) {
                if (dropdownRoot.activeSelf != optionsVisible) {
                    dropdownRoot.SetActive(optionsVisible);
                }
            }

            if (selectedCountText != null) {
                selectedCountText.text = $"{this.State.SelectedMagazineCount}/{this.State.SelectedTotalAmmoCount}";
            }

            if (optionsPanel != null) {
                optionsPanel.Render(this.State.Options);
                this.AlignOptionsPanelContent();
            }

            if (optionsBackground != null) {
                var bg = optionsBackground.gameObject;
                if (bg.activeSelf != optionsVisible) {
                    bg.SetActive(optionsVisible);
                }
            }

            this.SyncArrowRotation(optionsVisible);
            if (optionsVisible) {
                this.SyncBackgroundHeight();
            }
        }

        public void ToggleDropdown() {
            if (!this.HasState) {
                return;
            }

            this.State.Toggle();
        }

        private void SyncBackgroundHeight() {
            if (optionsBackground == null) {
                return;
            }

            var optionsState = this.State?.Options;
            if (optionsState == null) {
                return;
            }

            var size = optionsState.Size;
            var height = size.MaxHeight;

            if (float.IsInfinity(height) || height <= 0f) {
                height = size.MinHeight;
            }

            height = Mathf.Max(0f, height + Mathf.Max(0f, this.backgroundExtraHeight));

            if (!float.IsNaN(height) && !float.IsInfinity(height)) {
                optionsBackground.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            }
        }

        private void SyncArrowRotation(bool optionsVisible) {
            if (arrowTransform == null) {
                return;
            }

            var targetZ = optionsVisible ? this.arrowExpandedRotationZ : this.arrowCollapsedRotationZ;
            var euler = arrowTransform.localEulerAngles;
            if (!Mathf.Approximately(euler.z, targetZ)) {
                arrowTransform.localRotation = Quaternion.Euler(euler.x, euler.y, targetZ);
            }
        }

        private void AlignOptionsPanelContent() {
            if (optionsPanel == null) {
                return;
            }

            var panelTransform = optionsPanel.transform;
            if (panelTransform.childCount == 0) {
                return;
            }

            for (var i = 0; i < panelTransform.childCount; i++) {
                if (panelTransform.GetChild(i) is RectTransform childRect) {
                    childRect.anchorMin = new Vector2(0f, 0f);
                    childRect.anchorMax = new Vector2(1f, 0f);
                    childRect.pivot     = new Vector2(0.5f, 0f);
                    childRect.anchoredPosition = Vector2.zero;
                }
            }
        }

        private void ApplyLayoutSettings() {
            if (!this.HasState) {
                return;
            }

            this.State.ConfigureLayout(this.optionsSpacing, this.optionsPaddingBottom);
        }

#if UNITY_EDITOR
        private void OnValidate() {
            if (!UnityEditor.EditorApplication.isPlaying) {
                return;
            }

            this.ApplyLayoutSettings();
        }
#endif
    }

    public interface IWeaponAmmoDropdownState : IViewState {
        IState Options { get; }

        string SelectedIcon            { get; }
        int    SelectedMagazineCount   { get; }
        int    SelectedTotalAmmoCount  { get; }
        bool   IsExpanded              { get; }
        bool   IsSelectedWeapon        { get; }

        void ConfigureLayout(float optionsSpacing, float optionsPaddingBottom);
        void ResetState();
        void Toggle();
    }
}
