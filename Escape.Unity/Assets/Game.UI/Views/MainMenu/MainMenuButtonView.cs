namespace Game.UI.Views.MainMenu {
    using UniMob.UI;
    using Multicast;
    using Notifier;
    using Sirenix.OdinInspector;
    using UnityEngine;

    public class MainMenuButtonView : AutoView<IMainMenuButtonState> {
        [SerializeField, Required] private NotifierView notifier;

        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("is_locked", () => this.State.IsLocked, true),
            this.Variable("locked_by_level", () => this.State.LockedByLevel, 99),
        };

        protected override AutoViewEventBinding[] Events => new[] {
            this.Event("click", () => this.State.Click()),
        };

        protected override void Render() {
            base.Render();

            this.notifier.Render(this.State.Notifier);
        }
    }

    public interface IMainMenuButtonState : IViewState {
        INotifierState Notifier { get; }

        bool IsLocked      { get; }
        int  LockedByLevel { get; }

        void Click();
    }
}