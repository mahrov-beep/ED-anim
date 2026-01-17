namespace Game.UI.Widgets.ExpProgressionRewards {
    using System.Linq;
    using Domain.ExpProgressionRewards;
    using Multicast;
    using Shared;
    using Shared.UserProfile.Data;
    using UniMob.UI;
    using UnityEngine;
    using Views.ExpProgressionRewards;

    [RequireFieldsInit]
    public class ExpProgressionRewardsBackgroundWidget : StatefulWidget {
    }

    public class ExpProgressionRewardsBackgroundState : ViewState<ExpProgressionRewardsBackgroundWidget>, IExpProgressionRewardsBackgroundState {
        [Inject] private ExpProgressionRewardsModel expProgressionRewardsModel;
        [Inject] private GameDef                    gameDef;
        [Inject] private SdUserProfile              userProfile;

        public override WidgetViewReference View => UiConstants.Views.ExpProgressionRewards.Background;

        public float CurrentProgress {
            get {
                float progress = this.expProgressionRewardsModel.AllUnlocked.Sum(static it => it.PlacesTakenInRewardsRow);

                if (this.expProgressionRewardsModel.FirstLocked is { } firstLocked) {
                    var lastUnlockedLevelToComplete = this.expProgressionRewardsModel.LastUnlocked is { } lastUnlocked
                        ? lastUnlocked.LevelToComplete
                        : 0;

                    var exp = 0f;
                    for (var level = lastUnlockedLevelToComplete + 1; level <= firstLocked.LevelToComplete; level++) {
                        exp += this.gameDef.GetLevel(level).expToNextLevel;
                    }

                    progress += Mathf.InverseLerp(0, exp, this.userProfile.Exp.Value) * firstLocked.PlacesTakenInRewardsRow;
                }

                return progress;
            }
        }

        public float MaxProgress => this.expProgressionRewardsModel.All.Sum(static it => it.PlacesTakenInRewardsRow);
    }
}