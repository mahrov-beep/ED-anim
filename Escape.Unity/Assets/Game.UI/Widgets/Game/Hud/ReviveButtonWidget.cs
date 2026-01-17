namespace Game.UI.Widgets.Game {
    using Domain.Game;
    using Multicast;
    using Quantum;
    using Quantum.Commands;
    using Services.Photon;
    using Shared;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using UnityEngine;
    using Views.Game;
    using Views.Game.Hud;

    [RequireFieldsInit]
    public class ReviveButtonOptionalWidget : StatefulWidget {
    }

    public class ReviveButtonOptionalState : HocState<ReviveButtonOptionalWidget> {
        [Inject] private GameLocalCharacterModel localCharacterModel;

        public override Widget Build(BuildContext context) {
            if (!this.localCharacterModel.CanReviveTeammate && !this.localCharacterModel.IsRevivingTeammate) {
                return new Empty();
            }

            return new ReviveButtonWidget();
        }
    }

    [RequireFieldsInit]
    public class ReviveButtonWidget : StatefulWidget {
    }

    public class ReviveButtonState : ViewState<ReviveButtonWidget>, IUnitAbilityState {
        [Inject] private GameLocalCharacterModel localCharacterModel;
        [Inject] private PhotonService           photonService;

        const string ReviveIcon = SharedConstants.Game.Items.HEAL_BOX_SMALL;

        public override WidgetViewReference View => UiConstants.Views.HUD.UnitAbilityRevive;

        public float ReloadingProgress {
            get {
                if (!this.localCharacterModel.IsRevivingTeammate) {
                    return 0f;
                }

                return Mathf.Clamp01(1f - this.localCharacterModel.ReviveProgress);
            }
        }

        public string ItemKey  => ReviveIcon;
        public string ItemIcon => ReviveIcon;
    }
}
