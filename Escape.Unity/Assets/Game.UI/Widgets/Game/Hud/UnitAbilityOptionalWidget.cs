namespace Game.UI.Widgets.Game {
    using Domain.Game;
    using Domain.GameInventory;
    using Multicast;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using UnityEngine;

    [RequireFieldsInit]
    public class UnitAbilityOptionalWidget : StatefulWidget {
    }

    public class UnitAbilityOptionalState : HocState<UnitAbilityOptionalWidget> {
        [Inject] private GameInventoryModel gameInventoryModel;
        [Inject] private GameLocalCharacterModel localCharacterModel;

        public override Widget Build(BuildContext context) {
            if (this.localCharacterModel.CanReviveTeammate || this.localCharacterModel.IsRevivingTeammate) {
                return new ReviveButtonWidget();
            }

            if (this.gameInventoryModel.AbilityModel.HasAbility) {
                return new UnitAbilityWidget();
            }

            return new Empty();
        }
    }
}
