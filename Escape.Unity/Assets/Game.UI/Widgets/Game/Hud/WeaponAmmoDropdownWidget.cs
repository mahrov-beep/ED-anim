namespace Game.UI.Widgets.Game.Hud {
    using System;
    using System.Collections.Generic;
    using Multicast;
    using UniMob;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using Views.Game.Hud;
    
    public sealed class WeaponAmmoOptionModel {
        public static readonly WeaponAmmoOptionModel Empty = new WeaponAmmoOptionModel {
            Icon = string.Empty,
            MagazineCount = 0,
            TotalAmmoCount = 0,
            IsSelected = false,
            OnSelect = null,
        };

        public string Icon;
        public int    MagazineCount;
        public int    TotalAmmoCount;
        public bool   IsSelected;
        public Action OnSelect;
    }

    public class WeaponAmmoDropdownWidget : StatefulWidget {
        public bool            IsSelectedWeapon;
        public WeaponAmmoOptionModel[] Options;
        public Func<WeaponAmmoOptionModel[]> OptionsBuilder;
        public WeaponAmmoOptionModel SelectedOption;
        public Action<bool>    OnToggle;
        public float           OptionsSpacing       = 6f;
        public float           OptionsPaddingBottom = 0f;
    }

    public class WeaponAmmoDropdownState : ViewState<WeaponAmmoDropdownWidget>, IWeaponAmmoDropdownState {
        private readonly MutableAtom<bool> isExpanded = Atom.Value(false);
        private readonly MutableAtom<float> optionsSpacing = Atom.Value(0f);
        private readonly MutableAtom<float> optionsPaddingBottom = Atom.Value(0f);

        private WeaponAmmoOptionModel[] optionsSnapshot = Array.Empty<WeaponAmmoOptionModel>();
        private WeaponAmmoOptionModel   selectedOption;
        private readonly List<WeaponAmmoOptionWidget> optionWidgetsBuffer = new List<WeaponAmmoOptionWidget>();

        public override WidgetViewReference View => default;

        public IState Options => this.RenderChild(_ => this.BuildOptions());

        public string SelectedIcon => this.SelectedOption.Icon;

        public int SelectedMagazineCount => this.SelectedOption.MagazineCount;

        public int SelectedTotalAmmoCount => this.SelectedOption.TotalAmmoCount;

        public bool IsExpanded => this.isExpanded?.Value ?? false;
        public bool IsSelectedWeapon => this.Widget.IsSelectedWeapon;

        public override void InitState() {
            base.InitState();
            this.ConfigureLayout(this.Widget.OptionsSpacing, this.Widget.OptionsPaddingBottom);
        }

        public override void DidUpdateWidget(WeaponAmmoDropdownWidget oldWidget) {
            base.DidUpdateWidget(oldWidget);

            if (oldWidget.IsSelectedWeapon && !this.Widget.IsSelectedWeapon) {
                this.ResetState();
            }

            if (Math.Abs(oldWidget.OptionsSpacing - this.Widget.OptionsSpacing) > float.Epsilon ||
                Math.Abs(oldWidget.OptionsPaddingBottom - this.Widget.OptionsPaddingBottom) > float.Epsilon) {
                this.ConfigureLayout(this.Widget.OptionsSpacing, this.Widget.OptionsPaddingBottom);
            }

            this.InvalidateOptionsCache();
        }

        public void Toggle() {
            if (!this.Widget.IsSelectedWeapon) {
                return;
            }

            var newValue = !this.isExpanded.Value;
            this.isExpanded.Value = newValue;
            this.Widget.OnToggle?.Invoke(newValue);
        }

        private Widget BuildOptions() {
            if (!this.IsExpanded || !this.Widget.IsSelectedWeapon) {
                return new Empty();
            }

            var options = this.UpdateOptionsSnapshot();
            if (options.Length == 0) {
                return new Empty();
            }

            var spacing = Math.Max(0f, this.optionsSpacing.Value);
            var paddingBottom = Math.Max(0f, this.optionsPaddingBottom.Value);

            this.optionWidgetsBuffer.Clear();
            for (var i = 0; i < options.Length; i++) {
                var optionModel = options[i];
                if (optionModel == null) {
                    continue;
                }

                this.optionWidgetsBuffer.Add(new WeaponAmmoOptionWidget {
                    Icon = optionModel.Icon ?? string.Empty,
                    MagazineCount = optionModel.MagazineCount,
                    TotalAmmoCount = optionModel.TotalAmmoCount,
                    IsSelected = optionModel.IsSelected,
                    OnSelect = () => this.HandleSelect(optionModel),
                });
            }

            if (this.optionWidgetsBuffer.Count == 0) {
                return new Empty();
            }

            return new WeaponAmmoOptionsWidget {
                Options = this.optionWidgetsBuffer.ToArray(),
                Spacing = spacing,
                PaddingBottom = paddingBottom,
            };
        }

        private WeaponAmmoOptionModel SelectedOption {
            get {
                this.UpdateOptionsSnapshot();
                return this.selectedOption;
            }
        }

        private void HandleSelect(WeaponAmmoOptionModel optionModel) {
            this.isExpanded.Value = false;
            optionModel?.OnSelect?.Invoke();
        }

        public void ConfigureLayout(float spacing, float paddingBottom) {
            spacing = Math.Max(0f, spacing);
            paddingBottom = Math.Max(0f, paddingBottom);

            this.optionsSpacing.Value = spacing;
            this.optionsPaddingBottom.Value = paddingBottom;
        }

        public void ResetState() {
            using (Atom.NoWatch) {
                this.isExpanded.Value = false;
            }

            this.InvalidateOptionsCache();
        }

        private WeaponAmmoOptionModel[] UpdateOptionsSnapshot() {
            var options = this.Widget.OptionsBuilder?.Invoke() ?? this.Widget.Options ?? Array.Empty<WeaponAmmoOptionModel>();
            this.optionsSnapshot = options ?? Array.Empty<WeaponAmmoOptionModel>();
            options = this.optionsSnapshot;

            var explicitSelected = this.Widget.SelectedOption;
            if (explicitSelected != null) {
                this.selectedOption = explicitSelected;
                return this.optionsSnapshot;
            }

            if (options == null || options.Length == 0) {
                this.selectedOption = WeaponAmmoOptionModel.Empty;
                return this.optionsSnapshot;
            }

            for (var i = 0; i < options.Length; i++) {
                var option = options[i];
                if (option != null && option.IsSelected) {
                    this.selectedOption = option;
                    return this.optionsSnapshot;
                }
            }

            this.selectedOption = options[0] ?? WeaponAmmoOptionModel.Empty;
            return this.optionsSnapshot;
        }

        private void InvalidateOptionsCache() {
            this.optionsSnapshot = Array.Empty<WeaponAmmoOptionModel>();
            this.selectedOption  = this.Widget.SelectedOption ?? WeaponAmmoOptionModel.Empty;
        }
    }
}
