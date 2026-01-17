namespace Game.UI.Widgets.MailBox {
    using System;
    using System.Linq;
    using Header;
    using Multicast;
    using Shared;
    using Shared.UserProfile.Data;
    using UniMob;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using Views.MailBox;

    [RequireFieldsInit]
    public class MailBoxMenuWidget : StatefulWidget {
        public Action OnClose;
    }

    public class MailBoxMenuState : ViewState<MailBoxMenuWidget>, IMailBoxMenuState {
        [Inject] private SdUserProfile userProfile;

        public override WidgetViewReference View => UiConstants.Views.MailBox.Screen;

        [Atom] public IState Messages => this.RenderChild(_ => new ScrollGridFlow {
            MaxCrossAxisCount = 1,
            Children = {
                new Container {
                    Size = WidgetSize.Fixed(1000, 150),
                },

                this.userProfile.MailBox.Messages.Select(it => new MailBoxMessageWidget {
                    MailBoxMessageGuid = it.MessageGuid,
                }),

                new Container {
                    Size = WidgetSize.Fixed(1000, 500),
                },
            },
        });

        [Atom] public IState Header => this.RenderChild(_ => new Row {
            CrossAxisSize      = AxisSize.Max,
            MainAxisSize       = AxisSize.Max,
            CrossAxisAlignment = CrossAxisAlignment.Center,
            MainAxisAlignment  = MainAxisAlignment.End,
            Size               = WidgetSize.Stretched,
            Children = {
                new HeaderCurrencyWidget(SharedConstants.Game.Currencies.LOADOUT_TICKETS),
                new HeaderCurrencyWidget(SharedConstants.Game.Currencies.BADGES),
                new HeaderCurrencyWidget(SharedConstants.Game.Currencies.BUCKS),
                new HeaderCurrencyWidget(SharedConstants.Game.Currencies.CRYPT),
            },
        });

        public void Close() {
            this.Widget.OnClose?.Invoke();
        }
    }
}