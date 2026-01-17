namespace Game.UI.Views.Threshers {
    using UniMob.UI;
    using Multicast;
    using Shared;
    using Sirenix.OdinInspector;
    using UniMob.UI.Widgets;
    using UnityEngine;
    using UnityEngine.Serialization;

    public class ThresherView : AutoView<IThresherState> {
        [SerializeField, Required] private ViewPanel itemsPanel;

        [SerializeField, Required] private UniMobDropZoneBehaviour toThreshDropZone;

        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("thresher_key", () => this.State.ThresherKey, SharedConstants.Game.Threshers.TRADER),
            this.Variable("level", () => this.State.Level, 5),
            this.Variable("can_level_up", () => this.State.CanLevelUp, true),
        };

        protected override AutoViewEventBinding[] Events => new[] {
            this.Event("level_up", () => this.State.LevelUp()),
        };

        protected override void Awake() {
            base.Awake();

            this.toThreshDropZone.IsPayloadAcceptableDelegate = p => {
                if (!this.HasState) {
                    return false;
                }

                return p is DragAndDropPayloadItem itemEntity && this.State.CanMoveItemToThresh(itemEntity);
            };
            this.toThreshDropZone.OnAccept.AddListener(p => {
                if (this.HasState && p is DragAndDropPayloadItem payloadItem) {
                    this.State.OnMoveItemToThresh(payloadItem);
                }
            });
        }

        protected override void Render() {
            base.Render();

            this.itemsPanel.Render(this.State.Items);
        }
    }

    public interface IThresherState : IViewState {
        string ThresherKey { get; }

        IState Items { get; }

        int Level { get; }

        bool CanLevelUp { get; }

        bool CanMoveItemToThresh(DragAndDropPayloadItem payload);
        void OnMoveItemToThresh(DragAndDropPayloadItem payload);

        void LevelUp();
    }
}