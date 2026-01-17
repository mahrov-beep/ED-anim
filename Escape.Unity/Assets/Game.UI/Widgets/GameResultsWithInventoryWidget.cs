namespace Game.UI.Widgets {
    using System;
    using Domain.GameInventory;
    using Multicast;
    using GameInventory;
    using GameResults.Simple;
    using UniMob.UI;
    using UniMob.UI.Widgets;

    [RequireFieldsInit]
    public class GameResultsWithInventoryWidget : StatefulWidget {
        public Action OnClose;

        public string PlayedGameId;
    }

    public class GameResultsWithInventoryState : HocState<GameResultsWithInventoryWidget> {
        [Inject] private GameInventoryModel gameInventoryModel;

        public override void InitState() {
            base.InitState();
            this.gameInventoryModel.IsUsageLocked = true;
        }

        public override void Dispose() {
            this.gameInventoryModel.IsUsageLocked = false;
            base.Dispose();
        }

        public override Widget Build(BuildContext context) {
            return new ZStack {
                Children = {
                    new SimpleGameResultsWidget {
                        PlayedGameId = this.Widget.PlayedGameId,
                        OnContinue   = this.Widget.OnClose,
                    },
                    new GameInventoryWidget {
                        OnClose               = this.Widget.OnClose,
                        ShowItemsThrowZone    = false,
                        NoDraggingInInventory = true,
                        IgnoreNearby          = true,
                        OnIncrementLoadout    = default,
                        OnDecrementLoadout    = default,
                        HasSomeLoadouts       = false,
                        LoadoutIndex          = 0,
                        LoadoutCount          = 0,
                    },
                },
            };
        }
    }
}