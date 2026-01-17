namespace Game.UI.Views.GameInventory {
    using UniMob.UI;
    using Multicast;
    using Quantum;
    using Sirenix.OdinInspector;
    using UniMob.UI.Widgets;
    using UnityEngine;

    public class GameInventoryCellView : AutoView<IGameInventoryCellState> {
        [SerializeField, Required] private UniMobDropZoneBehaviour trashDropZone;
        [SerializeField, Required] private ViewPanel               trash;

        protected override void Awake() {
            base.Awake();

            this.trashDropZone.IsPayloadAcceptableDelegate = p => {
                if (!this.HasState) {
                    return false;
                }

                if (p is not DragAndDropPayloadItem itemEntity) {
                    return false;
                }

                var dropRange = this.State.GetDropPlace(itemEntity, out var isDropPlaceValid);

                // Не выставляем в false так как это приводит к тому,
                // что при перемещении предмета за пределы инвентаря и если последнее место бы не валидным,
                // на последнем месте в инвентаре не срабатывает OnHoverEndEvent
                // и красное выделение остается некрасиво висеть в инвентаре.
                //
                // Взамен просто притворяемся что бросить предмет можно в любом слоте,
                // а в OnAccept перепроверяем и отметаем невалидные случаи.
                //
                // this.trashDropZone.CanDrop = isDropPlaceValid;
                this.trashDropZone.CanDrop = true;

                this.State.OnCellDrag(itemEntity, dropRange, isDropPlaceValid);

                return this.trashDropZone.CanDrop;
            };

            this.trashDropZone.OnAccept.AddListener(p => {
                if (this.HasState && p is DragAndDropPayloadItem payloadItem) {
                    var dropRange = this.State.GetDropPlace(payloadItem, out var isDropPlaceValid);

                    if (!isDropPlaceValid) {
                        return;
                    }

                    this.State.OnCellDrag(payloadItem, CellsRange.Empty, false);

                    this.State.OnMoveItemToTrash(payloadItem, dropRange);
                }
            });

            this.trashDropZone.OnHoverEndEvent.AddListener((p) => {
                this.State.OnCellDrag(null, CellsRange.Empty, false);
            });

            this.trashDropZone.OnDragAndDropEndEvent.AddListener((p) => {
                this.State.OnCellDrag(null, CellsRange.Empty, false);
            });
        }

        protected override void Render() {
            base.Render();

            this.trash.Render(this.State.TrashItem);
        }

        private void Update() {
            this.trashDropZone.CanAccept    = this.State.CanAccept;
            this.trashDropZone.CanNotAccept = this.State.CanNotAccept;
            this.trashDropZone.RefreshHoverHighlight();
        }
    }

    public interface IGameInventoryCellState : IViewState {
        CellsRange GetDropPlace(DragAndDropPayloadItem payload, out bool valid);

        void OnMoveItemToTrash(DragAndDropPayloadItem payload, CellsRange dropRange);
        void OnCellDrag(DragAndDropPayloadItem payload, CellsRange dropRange, bool succeed);

        bool IsBusy       { get; }
        bool CanAccept    { get; }
        bool CanNotAccept { get; }

        int IndexI { get; }
        int IndexJ { get; }

        IState TrashItem { get; }
    }
}