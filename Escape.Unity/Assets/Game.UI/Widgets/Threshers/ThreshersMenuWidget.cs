namespace Game.UI.Widgets.Threshers {
    using System;
    using System.Linq;
    using Controllers.Features.Thresher;
    using Domain.Threshers;
    using Multicast;
    using UniMob;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using Views.Threshers;

    [RequireFieldsInit]
    public class ThreshersMenuWidget : StatefulWidget {
        public Action OnClose { get; set; }
    }

    public class ThreshersMenuState : ViewState<ThreshersMenuWidget>, IThreshersMenuState {
        [Inject] private ThreshersModel threshersModel;

        private readonly StateHolder threshersListState;
        private readonly StateHolder thresherState;

        public override WidgetViewReference View => UiConstants.Views.Threshers.Screen;

        [Atom] public string SelectedThresherKey { get; private set; } = "";

        public IState Thresher      => this.thresherState.Value;
        public IState ThreshersList => this.threshersListState.Value;

        public bool CanLevelUp => this.threshersModel.TryGet(this.SelectedThresherKey, out var selectedThresherModel)
                                  && selectedThresherModel.CanLevelUp;

        public ThreshersMenuState() {
            this.threshersListState = this.CreateChild(this.BuildThreshersList);
            this.thresherState      = this.CreateChild(this.BuildThresher);
        }

        public override void InitState() {
            base.InitState();

            this.SelectedThresherKey = this.threshersModel.AllThreshers.FirstOrDefault()?.Key;
        }

        public void Close() {
            this.Widget.OnClose?.Invoke();
        }

        public void LevelUp() {
            ThresherFeatureEvents.LevelUp.Raise(new ThresherFeatureEvents.LevelUpArgs {
                thresherKey = this.SelectedThresherKey,
            });
        }

        private Widget BuildThresher(BuildContext context) {
            return new AnimatedSwitcher {
                Child           = BuildThresherContent(),
                Duration        = 0.2f,
                ReverseDuration = 0.15f,
                TransitionMode  = AnimatedSwitcherTransitionMode.Sequential,
            };

            Widget BuildThresherContent() {
                if (this.threshersModel.TryGet(this.SelectedThresherKey, out var selectedThresherModel)) {
                    if (selectedThresherModel.IsAtMaxLevel) {
                        return new Container {
                            Size = WidgetSize.Stretched,
                        };
                    }

                    return new ThresherWidget {
                        ThresherKey = selectedThresherModel.Key,

                        Key = Key.Of(selectedThresherModel.Key),
                    };
                }

                return new Container {
                    Size = WidgetSize.Stretched,
                };
            }
        }

        private Widget BuildThreshersList(BuildContext context) {
            return new ScrollGridFlow {
                MaxCrossAxisCount = 1,
                Padding           = new RectPadding(0, 0, 20, 200),
                Children = {
                    this.threshersModel.AllThreshers
                        .Select(this.BuildThresherItem),
                },
            };
        }

        private Widget BuildThresherItem(ThresherModel thresherModel) {
            return new ThresherItemWidget {
                ThresherKey = thresherModel.Key,
                IsSelected  = this.SelectedThresherKey == thresherModel.Key,
                OnSelect    = () => this.SelectedThresherKey = thresherModel.Key,

                Key = Key.Of(thresherModel.Key),
            };
        }
    }
}