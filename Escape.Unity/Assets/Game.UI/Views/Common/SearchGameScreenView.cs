namespace Game.UI.Views.Common {
    using Multicast;
    using Sirenix.OdinInspector;
    using UniMob.UI;
    using UnityEngine;
    using Views.MainMenu;

    public class SearchGameScreenView : AutoView<ISearchGameScreenState> {
        [SerializeField, Required] private MainMenuPlayButtonCancelView playButtonView;

        protected override void Render() {
            base.Render();

            this.playButtonView.Render(this.State.PlayButton);
        }
    }

    public interface ISearchGameScreenState : IViewState {
        IMainMenuPlayButtonState PlayButton { get; }
    }
}
