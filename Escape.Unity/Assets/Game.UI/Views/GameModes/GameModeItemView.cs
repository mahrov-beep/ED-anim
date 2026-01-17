namespace Game.UI.Views.GameModes {
    using System;
    using UniMob.UI;
    using Multicast;
    using Shared;
    using Sirenix.OdinInspector;
    using UnityEngine;

    public class GameModeItemView : AutoView<IGameModeItemState> {
        [SerializeField, TableList]
        private ItemPosition[] positions = Array.Empty<ItemPosition>();

        [Serializable]
        private struct ItemPosition {
            public string  gameModeKey;
            public Vector2 position;
        }

        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("game_mode_key", () => this.State.GameModeKey, SharedConstants.Game.GameModes.INIT_GAME_MODE),
            this.Variable("is_selected", () => this.State.IsSelected, false),
            this.Variable("current_profile_level", () => this.State.CurrentProfileLevel, 1),
            this.Variable("required_profile_level", () => this.State.RequiredProfileLevel, 5),
        };

        protected override AutoViewEventBinding[] Events => new[] {
            this.Event("select", () => this.State.Select()),
        };

        protected override void Render() {
            base.Render();

            var gameModeKey = this.State.GameModeKey;

            var positionIndex = Array.FindIndex(this.positions, it => it.gameModeKey == gameModeKey);
            var position      = positionIndex != -1 ? this.positions[positionIndex].position : Vector2.zero;

            this.rectTransform.anchoredPosition = position;
        }
    }

    public interface IGameModeItemState : IViewState {
        string GameModeKey { get; }

        bool IsSelected { get; }

        int RequiredProfileLevel { get; }
        int CurrentProfileLevel  { get; }

        void Select();
    }
}