namespace Game.UI.Widgets.Game {
    using Domain.Game;
    using Multicast;
    using Quantum;
    using Quantum.Commands;
    using Services.Photon;
    using Shared;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using Views.Game;

    [RequireFieldsInit]
    public class KnifeAttackOptionalWidget : StatefulWidget {
    }

    public class KnifeAttackOptionalState : HocState<KnifeAttackOptionalWidget> {
        [Inject] private GameLocalCharacterModel localCharacterModel;

        public override Widget Build(BuildContext context) {      
            if (!this.localCharacterModel.CanKnifeAttack || this.localCharacterModel.IsKnifeAttacking) {
                return new Empty();
            }

            return new KnifeAttackWidget();
        }
    }

    [RequireFieldsInit]
    public class KnifeAttackWidget : StatefulWidget {
    }

    public class KnifeAttackState : ViewState<KnifeAttackWidget>, IUnitAbilityState {
        [Inject] private GameLocalCharacterModel localCharacterModel;
        [Inject] private PhotonService           photonService;

        const string KnifeIcon = SharedConstants.Game.Items.WEAPON_KNIFE;

        public override WidgetViewReference View => UiConstants.Views.HUD.UnitAbilityKnifeAttack;

        public float ReloadingProgress => Clamp01(this.localCharacterModel.KnifeCooldownProgress);

        public string ItemKey  => KnifeIcon;
        public string ItemIcon => KnifeIcon;

        static float Clamp01(float value) {
            if (value < 0f) {
                return 0f;
            }

            return value > 1f ? 1f : value;
        }
    }
}
