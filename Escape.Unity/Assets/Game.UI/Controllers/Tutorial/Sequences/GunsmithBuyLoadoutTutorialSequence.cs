namespace Game.UI.Controllers.Tutorial.Sequences {
    using System;
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using Domain.Currencies;
    using Domain.Features;
    using Domain.Gunsmiths;
    using Multicast;
    using Multicast.Misc.Tutorial;
    using Shared;
    using TutorialIDs = UiConstants.TutorialIDs;
    using Routes = UiConstants.Routes;

    public class GunsmithBuyLoadoutTutorialSequence : BaseMultiStepTutorialSequence<GunsmithBuyLoadoutTutorialSequence.Step> {
        [Inject] private CurrenciesModel currenciesModel;
        [Inject] private GunsmithsModel  gunsmithsModel;
        [Inject] private FeaturesModel   featuresModel;

        protected override string TutorialKey => SharedConstants.Game.Tutorials.GUNSMITH_BUY_LOADOUT;

        protected override Dictionary<Step, TutorialStepHighlight> Steps => new() {
            [Step.WaitForGunsmithClick] = new TutorialStepHighlight(Routes.MainMenu, new[] {
                TutorialIDs.MainMenu.GunsmithButton,
                TutorialIDs.MainMenu.GunsmithButtonHelp,
            }),
            [Step.DialogEnoughTickets] = new TutorialStepHighlight(Routes.GunsmithMenu, Array.Empty<TutorialObjectID>()),
            [Step.WaitForLoadoutBuy] = new TutorialStepHighlight(Routes.GunsmithMenu, new[] {
                TutorialIDs.GunsmithMenu.AllLoadouts,
            }),
            [Step.WaitForClose] = new TutorialStepHighlight(Routes.GunsmithMenu, new[] {
                TutorialIDs.GunsmithMenu.Close,
                TutorialIDs.GunsmithMenu.CloseHelp,
            }),
        };

        public override async UniTask On_MainMenu_Flow(ControllerBase.Context context) {
            if (this.CurrentStep == null &&
                this.IsTutorialNotCompleted &&
                context.RootNavigator.TopMostRouteIs(Routes.MainMenu) &&
                this.featuresModel.IsFeatureUnlocked(SharedConstants.Game.Features.GUNSMITH) &&
                this.currenciesModel.Get(SharedConstants.Game.Currencies.LOADOUT_TICKETS).Amount > 0) {
                this.ChangeStep(Step.WaitForGunsmithClick);
            }
        }

        public override async UniTask On_GunsmithMenu_Activated(ControllerBase.Context context) {
            if (this.CurrentStep == Step.WaitForGunsmithClick) {
                await this.ChangeStepAndShowPopup(context, Step.DialogEnoughTickets);

                this.ChangeStep(Step.WaitForLoadoutBuy);
            }
        }

        public override async UniTask On_GunsmithMenu_LoadoutBuy(ControllerBase.Context context) {
            if (this.CurrentStep == Step.WaitForLoadoutBuy) {
                this.ChangeStep(Step.WaitForClose);

                await this.SetTutorialCompleted(context);
            }
        }

        public override async UniTask On_GunsmithMenu_Close(ControllerBase.Context context) {
            
            if (this.CurrentStep == Step.WaitForClose) {
                this.ChangeStep(null);
            }
        }

        public enum Step {
            WaitForGunsmithClick,
            DialogEnoughTickets,
            WaitForLoadoutBuy,
            WaitForClose,
        }
    }
}