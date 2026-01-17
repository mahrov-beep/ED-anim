namespace Game.UI.Widgets.Gunsmiths {
    using System;
    using System.Linq;
    using Domain.Gunsmiths;
    using Domain.Threshers;
    using GameInventory.Snapshot;
    using Header;
    using Multicast;
    using Quantum;
    using Shared;
    using Shared.UserProfile.Data;
    using Shared.UserProfile.Data.Gunsmiths;
    using UniMob;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using Views.Gunsmiths;

    [RequireFieldsInit]
    public class GunsmithMenuWidget : StatefulWidget {
        public string GunsmithKey;

        public Action OnClose;
    }

    public class GunsmithMenuState : ViewState<GunsmithMenuWidget>, IGunsmithMenuState {
        [Inject] private GunsmithsModel gunsmithsModel;
        [Inject] private ThreshersModel threshersModel;
        [Inject] private GameDef        gameDef;

        private readonly StateHolder loadoutsState;
        private readonly StateHolder headerState;

        public GunsmithMenuState() {
            this.loadoutsState = this.CreateChild(this.BuildLoadouts);
            this.headerState   = this.CreateChild(this.BuildHeader);
        }

        [Atom] private GunsmithModel GunsmithModel => this.gunsmithsModel.Get(this.Widget.GunsmithKey);
        [Atom] private ThresherModel ThresherModel => this.threshersModel.Get(this.GunsmithModel.Def.thresher);

        public override WidgetViewReference View => UiConstants.Views.Gunsmiths.Screen;

        public string GunsmithKey => this.GunsmithModel.Key;
        public IState Loadouts    => this.loadoutsState.Value;
        public IState Header      => this.headerState.Value;

        public void Close() {
            this.Widget.OnClose?.Invoke();
        }

        private Widget BuildLoadouts(BuildContext context) {
            return new GridFlow {
                CrossAxisAlignment = CrossAxisAlignment.Center,
                MainAxisAlignment  = MainAxisAlignment.Center,

                Children = {
                    this.GunsmithModel.Data.Loadouts.Select(it => this.BuildLoadout(it)),
                },
            };
        }

        private Widget BuildLoadout(SdGunsmithLoadout sdGunsmithLoadout) {
            var loadoutDef = this.gameDef.GunsmithLoadouts.Get(sdGunsmithLoadout.GunsmithLoadoutKey.Value);

            if (this.ThresherModel.Level < loadoutDef.enableOnLevel) {
                return new GunsmithLoadoutLevelBlockerWidget {
                    RequiredThresherLevel = loadoutDef.enableOnLevel,
                    ThresherKey           = this.ThresherModel.Key,
                };
            }

            return new GunsmithLoadoutWidget {
                GunsmithKey         = this.GunsmithModel.Key,
                GunsmithLoadoutGuid = sdGunsmithLoadout.Guid,

                Key = Key.Of(sdGunsmithLoadout.Guid),
            };
        }

        private Widget BuildHeader(BuildContext context) {
            return new Row {
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
            };
        }
    }
}