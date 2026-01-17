namespace Game.UI.Widgets.GameModes {
    using System;
    using System.Linq;
    using Controllers.Tutorial;
    using Domain.GameModes;
    using Multicast;
    using Multicast.GameProperties;
    using Shared;
    using Shared.Balance;
    using Shared.UserProfile.Data;
    using UniMob;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using Views.GameModes;

    [RequireFieldsInit]
    public class GameModesMenuWidget : StatefulWidget {
        public Action<string>       OnGameModeSelect;
        public Action<bool, string> OnConfirm;
    }

    public class GameModesMenuState : ViewState<GameModesMenuWidget>, IGameModesMenuState {
        [Inject] private GameModesModel gameModesModel;
        [Inject] private GameDef        gameDef;
        [Inject] private SdUserProfile  userProfile;

        [Inject] private GamePropertiesModel gameProperties;
        [Inject] private TutorialService     tutorialService;

        private readonly StateHolder detailsState;
        private readonly StateHolder modesState;

        public GameModesMenuState() {
            this.detailsState = this.CreateChild(this.BuildDetails);
            this.modesState   = this.CreateChild(this.BuildModes);
        }

        public override WidgetViewReference View => UiConstants.Views.GameModes.Screen;

        [Atom] public string SelectedGameMode { get; set; }

        public IState Details => this.detailsState.Value;
        public IState Modes   => this.modesState.Value;

        public bool CanConfirm => this.NoEnoughQuality == false &&
                                  this.NoEnoughProfileLevel == false &&
                                  this.gameModesModel.TryGet(this.SelectedGameMode, out _);

        [Atom] public bool NoEnoughQuality => !string.IsNullOrEmpty(this.SelectedGameMode) &&
                                              this.gameModesModel.TryGet(this.SelectedGameMode, out var selectedModel) &&
                                              this.userProfile.Level.Value >= selectedModel.MinProfileLevel &&
                                              ItemBalance.GetLoadoutQuality(this.gameDef, this.userProfile.Loadouts.GetSelectedLoadoutClone()) < selectedModel.MinLoadoutQuality;

        [Atom] public bool NoEnoughProfileLevel => this.gameModesModel.TryGet(this.SelectedGameMode, out var selectedModel) &&
                                                   this.userProfile.Level.Value < selectedModel.MinProfileLevel;

        public override void InitState() {
            base.InitState();

            this.SelectedGameMode = this.tutorialService.HasAnyActiveTutorial ? string.Empty : this.gameModesModel.SelectedGameMode.Key;
        }

        public void Close() {
            this.Widget.OnConfirm?.Invoke(false, null);
        }

        public void Confirm() {
            this.Widget.OnConfirm?.Invoke(true, this.SelectedGameMode);
        }

        private Widget BuildDetails(BuildContext context) {
            return new AnimatedSwitcher {
                Child           = BuildDetailsContent(),
                Duration        = 0.2f,
                ReverseDuration = 0.15f,
                TransitionMode  = AnimatedSwitcherTransitionMode.Sequential,
            };

            Widget BuildDetailsContent() {
                if (this.gameModesModel.TryGet(this.SelectedGameMode, out var selectedGameModeModel)) {
                    return new GameModeDetailsWidget {
                        GameModeKey = selectedGameModeModel.Key,

                        Key = Key.Of(selectedGameModeModel.Key),
                    };
                }

                return new Empty();
            }
        }

        private Widget BuildModes(BuildContext context) {
            var showDevScenes = gameProperties.Get(GameProperties.Booleans.ShowDevScenes);
            
            return new UnPositionedStack {
                Children = {
                    this.gameModesModel.VisibleGameModes
                                    .Where(model => !model.ModeQuantumAsset.isDevelopOnly || showDevScenes)
                                    .Select(this.BuildGameModeItem),
                },
            };
        }

        private Widget BuildGameModeItem(GameModeModel gameModeModel) {
            return new GameModeItemWidget {
                GameModeKey = gameModeModel.Key,
                IsSelected  = this.SelectedGameMode == gameModeModel.Key,
                OnSelect    = () => {
                    this.SelectedGameMode = gameModeModel.Key;
                    this.Widget.OnGameModeSelect?.Invoke(gameModeModel.Key);
                },

                Key = Key.Of(gameModeModel.Key),
            };
        }
    }
}