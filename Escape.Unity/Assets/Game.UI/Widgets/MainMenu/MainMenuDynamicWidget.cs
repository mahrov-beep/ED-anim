namespace Game.UI.Widgets.MainMenu {
    using System;
    using ECS.Scripts;
    using Multicast;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using Widgets.World;

    public class MainMenuDynamicWidget : StatefulWidget {
    }

    public class MainMenuDynamicState : HocState<MainMenuDynamicWidget> {
        private readonly Widget dynamicUiWidget;

        public MainMenuDynamicState(UiDynamicContext uiDynamicContext) {
            this.dynamicUiWidget = new ZStack {
                Children = {
                    Create<UnitHealthBarUiDynamicData>(data => new UnitHealthBarWidget(data)),
                    Create<HitMarkUiDynamicData>(data => new HitMarkWidget(data)),
                    Create<DynamicAimUiDynamicData>(data => new DynamicAimWidget(data)),
                    Create<ItemBoxTimerUiDynamicData>(data => new ItemBoxTimerWidget(data)),
                    Create<UnitPartyUiDynamicData>(data => new UnitPartyWidget(data)),
                },
            };

            Widget Create<TData>(Func<TData, Widget> convert) where TData : class, IUiDynamicData {
                var items = uiDynamicContext.Get(convert);
                return new UnPositionedStack {ChildrenBuilder = () => items.Value};
            }
        }

        public override Widget Build(BuildContext context) => this.dynamicUiWidget;
    }
}