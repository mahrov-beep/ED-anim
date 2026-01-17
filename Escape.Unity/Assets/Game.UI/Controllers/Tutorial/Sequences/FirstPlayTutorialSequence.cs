namespace Game.UI.Controllers.Tutorial.Sequences {
    using System;
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using Multicast;
    using Multicast.Misc.Tutorial;
    using Shared;
    using TutorialIDs = UiConstants.TutorialIDs;
    using Routes = UiConstants.Routes;

    public class FirstPlayTutorialSequence : BaseMultiStepTutorialSequence<FirstPlayTutorialSequence.Step> {
        protected override string TutorialKey => SharedConstants.Game.Tutorials.FIRST_PLAY;

        protected override Dictionary<Step, TutorialStepHighlight> Steps => new() {
            [Step.WaitForPlayClick] = new TutorialStepHighlight(Routes.MainMenu, new[] {
                TutorialIDs.MainMenu.PlayButton,
                TutorialIDs.MainMenu.PlayButtonHelp,
            }),
            [Step.DialogGameModesInfo] = new TutorialStepHighlight(Routes.MainMenu, Array.Empty<TutorialObjectID>()),
            [Step.WaitForFactorySelect] = new TutorialStepHighlight(Routes.GameModeSelection, new[] {
                TutorialIDs.GameModeSelector.Details,
                TutorialIDs.GameModeSelector.SelectModeFactory,
                TutorialIDs.GameModeSelector.SelectModeFactoryHelp,
            }),
            [Step.WaitForConfirmClick] = new TutorialStepHighlight(Routes.GameModeSelection, new[] {
                TutorialIDs.GameModeSelector.ConfirmButton,
                TutorialIDs.GameModeSelector.ConfirmButtonHelp,
            }),
        };

        public override async UniTask On_MainMenu_Flow(ControllerBase.Context context) {
            if (this.CurrentStep == null && this.IsTutorialNotCompleted && context.RootNavigator.TopMostRouteIs(Routes.MainMenu)) {
                this.ChangeStep(Step.WaitForPlayClick);
            }
        }

        public override async UniTask On_GameModeSelector_Activated(ControllerBase.Context context) {
            if (this.CurrentStep == Step.WaitForPlayClick) {
                await this.ChangeStepAndShowPopup(context, Step.DialogGameModesInfo);

                this.ChangeStep(Step.WaitForFactorySelect);
            }
        }

        public override async UniTask On_GameModeSelector_ModeSelected(ControllerBase.Context context, string gameModeKey) {
            if (this.CurrentStep == Step.WaitForFactorySelect && gameModeKey == SharedConstants.Game.GameModes.FACTORY) {
                this.ChangeStep(Step.WaitForConfirmClick);
            }
        }

        public override async UniTask On_GameModeSelector_GameConfirmed(ControllerBase.Context context, string gameModeKey) {
            if (this.CurrentStep == Step.WaitForConfirmClick) {
                this.ChangeStep(null);

                await this.SetTutorialCompleted(context);
            }
        }

        public enum Step {
            WaitForPlayClick,
            DialogGameModesInfo,
            WaitForFactorySelect,
            WaitForConfirmClick,
        }
    }
}