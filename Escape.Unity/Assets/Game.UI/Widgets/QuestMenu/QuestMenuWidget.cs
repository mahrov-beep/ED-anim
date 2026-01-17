namespace Game.UI.Widgets.QuestMenu {
    using System;
    using System.Linq;
    using Domain.Quests;
    using Header;
    using Multicast;
    using Shared;
    using UniMob;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using Views.QuestMenu;

    [RequireFieldsInit]
    public class QuestMenuWidget : StatefulWidget {
        public Action OnClose;
    }

    public class QuestMenuState : ViewState<QuestMenuWidget>, IQuestMenuState {
        [Inject] private QuestsModel questsModel;

        private readonly StateHolder headerState;
        private readonly StateHolder questListState;
        private readonly StateHolder selectedQuestState;

        private readonly MutableAtom<string> selectedQuestKey;

        public QuestMenuState() {
            this.selectedQuestKey = Atom.Value(this.StateLifetime, value: "");

            this.headerState        = this.CreateChild(this.BuildHeader);
            this.questListState     = this.CreateChild(this.BuildQuestList);
            this.selectedQuestState = this.CreateChild(this.BuildSelectedQuest);
        }

        public override void InitState() {
            base.InitState();

            if (this.questsModel.QuestMenuInitialSelectedQuest is { } initialQuest) {
                this.selectedQuestKey.Value = initialQuest.Key;
            }
        }

        public override WidgetViewReference View => UiConstants.Views.QuestMenu.Screen;

        public IState Header        => this.headerState.Value;
        public IState QuestList     => this.questListState.Value;
        public IState SelectedQuest => this.selectedQuestState.Value;

        public void Close() {
            this.Widget.OnClose?.Invoke();
        }

        private Widget BuildHeader(BuildContext context) {
            return new Row {
                CrossAxisSize      = AxisSize.Max,
                MainAxisSize       = AxisSize.Max,
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

        private Widget BuildQuestList(BuildContext context) {
            return new ScrollGridFlow {
                MaxCrossAxisExtent = 1,
                Padding            = new RectPadding(0, 0, 130, 500),

                Children = {
                    this.questsModel.QuestsVisibleInQuestMenu.Select(it => this.BuildQuestListItem(it.Key)),
                },
            };
        }

        private Widget BuildSelectedQuest(BuildContext context) {
            return new AnimatedSwitcher {
                Child           = BuildSelectedQuestContent(),
                Duration        = 0.25f,
                ReverseDuration = 0.05f,
                TransitionMode  = AnimatedSwitcherTransitionMode.Sequential,
            };

            Widget BuildSelectedQuestContent() {
                if (this.questsModel.TryGet(this.selectedQuestKey.Value, out var selectedQuest)) {
                    return new QuestMenuQuestDetailsWidget {
                        QuestKey = selectedQuest.Key,

                        Key = Key.Of(selectedQuest.Key),
                    };
                }

                return new Empty();
            }
        }

        private Widget BuildQuestListItem(string questKey) {
            return new QuestMenuListItemWidget {
                QuestKey         = questKey,
                SelectedQuestKey = this.selectedQuestKey,

                Key = Key.Of(questKey),
            };
        }
    }
}