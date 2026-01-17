namespace Game.UI.Views.Header {
    using UniMob.UI;
    using Multicast;
    using Sirenix.OdinInspector;
    using UniMob.UI.Widgets;
    using UnityEngine;

    public class HeaderView : AutoView<IHeaderState> {
        [SerializeField, Required] private ViewPanel contentView;

        protected override void Render() {
            base.Render();

            this.contentView.Render(this.State.Content);
        }
    }

    public interface IHeaderState : IViewState {
        IState Content { get; }
    }
}