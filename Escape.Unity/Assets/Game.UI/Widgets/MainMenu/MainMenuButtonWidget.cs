namespace Game.UI.Widgets.MainMenu {
    using System;
    using Domain.Features;
    using JetBrains.Annotations;
    using Multicast;
    using Notifier;
    using UniMob;
    using UniMob.UI;
    using Views.MainMenu;
    using Views.Notifier;

    [RequireFieldsInit(Optional = new[] { nameof(LockedByFeatureKey) })]
    public class MainMenuButtonWidget : StatefulWidget {
        public int    NotifierCounter;
        public Action OnClick;

        [CanBeNull] public string LockedByFeatureKey;
    }

    public class MainMenuButtonState : ViewState<MainMenuButtonWidget>, IMainMenuButtonState {
        [Inject] private FeaturesModel featuresModel;

        public override WidgetViewReference View { get; }

        [Atom] public INotifierState Notifier => this.RenderChildT(_ => new NotifierWidget {
            Counter = this.Widget.NotifierCounter,
            IsNew   = this.IsNew(),
        }).As<NotifierState>();

        public bool IsLocked {
            get {
                if (!string.IsNullOrEmpty(this.Widget.LockedByFeatureKey)) {
                    if (!this.featuresModel.IsFeatureUnlocked(this.Widget.LockedByFeatureKey)) {
                        return true;
                    }
                }

                return false;
            }
        }

        public int LockedByLevel {
            get {
                if (!string.IsNullOrEmpty(this.Widget.LockedByFeatureKey)) {
                    if (this.featuresModel.TryGetFeatureUnlockExpProgressionReward(this.Widget.LockedByFeatureKey, out var expProgressionRewardModel)) {
                        return expProgressionRewardModel.LevelToComplete +1;
                    }
                }

                return 0;
            }
        }

        public void Click() {
            this.Widget.OnClick?.Invoke();
        }

        private bool IsNew() {
            if (this.IsLocked) {
                return false;
            }

            if (!string.IsNullOrEmpty(this.Widget.LockedByFeatureKey)) {
                if (!this.featuresModel.IsFeatureViewed(this.Widget.LockedByFeatureKey)) {
                    return true;
                }
            }

            return false;
        }
    }
}