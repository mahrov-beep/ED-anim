namespace Game.UI.Controllers.Tutorial {
    using Sequences;

    public class TutorialService : BaseTutorialSequence {
        public override bool IsActive => false;

        public override void ForceComplete() {
        }

        public bool HasAnyActiveTutorial => this.HasAnyActiveSubSequence();

        public TutorialService(
            FirstPlayTutorialSequence firstPlayTutorialSequence,
            GunsmithBuyLoadoutTutorialSequence gunsmithBuyLoadoutTutorialSequence
        ) {
            // порядок добавления туторов задает приоритет их активации

            this.AddSubSequence(firstPlayTutorialSequence);
            this.AddSubSequence(gunsmithBuyLoadoutTutorialSequence);
        }
    }
}