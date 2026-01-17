namespace Game.UI.Widgets.Header {
    using Shared;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using Views.Header;
    using Widgets.Party;

    public class HeaderWidget : StatefulWidget {
    }

    public class HeaderState : ViewState<HeaderWidget>, IHeaderState {
        private readonly StateHolder contentStateHolder;

        public override WidgetViewReference View => UiConstants.Views.Header.Screen;

        public HeaderState() {
            this.contentStateHolder = this.CreateChild(this.BuildContent);
        }

        private Widget BuildContent(BuildContext context) {
            return new Row {
                CrossAxisAlignment = CrossAxisAlignment.Center,
                MainAxisAlignment  = MainAxisAlignment.End,
                Size               = WidgetSize.Stretched,
                Children = {
                    new HeaderCurrencyWidget(SharedConstants.Game.Currencies.BADGES),
                    new HeaderCurrencyWidget(SharedConstants.Game.Currencies.BUCKS),
                    new HeaderCurrencyWidget(SharedConstants.Game.Currencies.CRYPT),
                },
            };
        }

        public IState Content => this.contentStateHolder.Value;
    }
}