namespace Game.UI.Widgets.Game {
    using System.Linq;
    using Domain.Quests;
    using Multicast;
    using Quests;
    using UniMob.UI;
    using UniMob.UI.Widgets;

    [RequireFieldsInit]
    public class GameQuestsWidget : StatefulWidget {
    }

    public class GameQuestsState : HocState<GameQuestsWidget> {
        private const float TOP_PADDING = 25f;

        [Inject] private QuestsModel activeQuestsModel;

        public override Widget Build(BuildContext context) {
            return new PaddingBox(new RectPadding(0, 0, TOP_PADDING, 0)) {
                Child = new Column {
                    Size               = WidgetSize.Stretched,
                    MainAxisSize       = AxisSize.Max,
                    CrossAxisSize      = AxisSize.Max,
                    MainAxisAlignment  = MainAxisAlignment.Start,
                    CrossAxisAlignment = CrossAxisAlignment.Start,
                    Children = {
                        this.activeQuestsModel.QuestsVisibleInGame.Select(this.BuildQuest),
                    },
                },
            };
        }

        private Widget BuildQuest(QuestModel questModel) {
            return new QuestWidget {
                QuestKey = questModel.Key,
                Key      = Key.Of(questModel.Key),
            };
        }
    }
}