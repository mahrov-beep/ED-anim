namespace Game.UI.Widgets.GameModes {
    using System.Linq;
    using Domain.GameModes;
    using Items;
    using Multicast;
    using Quantum;
    using Shared;
    using Shared.Balance;
    using Shared.UserProfile.Data;
    using UniMob;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using Views.GameModes;

    [RequireFieldsInit]
    public class GameModeDetailsWidget : StatefulWidget {
        public string GameModeKey;
    }

    public class GameModeDetailsState : ViewState<GameModeDetailsWidget>, IGameModeDetailsState {
        [Inject] private GameModesModel gameModesModel;
        [Inject] private GameDef        gameDef;
        [Inject] private SdUserProfile  userProfile;

        private readonly StateHolder lootDetailsState;

        public GameModeDetailsState() {
            this.lootDetailsState = this.CreateChild(this.BuildLootDetails);
        }

        [Atom] private GameModeModel GameModeModel => this.gameModesModel.Get(this.Widget.GameModeKey);

        public override WidgetViewReference View => UiConstants.Views.GameModes.Details;

        public string GameModeKey => this.GameModeModel.Key;

        [Atom] public int CurrentQuality => ItemBalance.GetLoadoutQuality(this.gameDef, this.userProfile.Loadouts.GetSelectedLoadoutClone());

        public int RequiredQuality => this.GameModeModel.MinLoadoutQuality;

        public int CurrentProfileLevel  => this.userProfile.Level.Value;
        public int RequiredProfileLevel => this.GameModeModel.MinProfileLevel;

        public IState LootDetails => this.lootDetailsState.Value;

        private Widget BuildLootDetails(BuildContext context) {
            return new GridFlow {
                MainAxisSize       = AxisSize.Max,
                CrossAxisSize      = AxisSize.Max,
                CrossAxisAlignment = CrossAxisAlignment.Start,
                MainAxisAlignment  = MainAxisAlignment.Center,
                Children = {
                    this.GameModeModel.LootRarities.Select(rarity => new RarityItemAttachmentMarkerWidget {
                        Rarity = rarity,
                    }),
                },
            };
        }
    }
}