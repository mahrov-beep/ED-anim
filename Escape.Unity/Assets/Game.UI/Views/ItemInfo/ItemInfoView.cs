namespace Game.UI.Views.ItemInfo {
    using System;
    using DG.Tweening;
    using UniMob.UI;
    using Multicast;
    using Quantum;
    using Shared;
    using Sirenix.OdinInspector;
    using UniMob.UI.Widgets;
    using UnityEngine;

    public class ItemInfoView : AutoView<IItemInfoState> {
        [SerializeField, Required] private ViewPanel statsPanel;

        [SerializeField, Required] private RectTransform root;
        
        [SerializeField, Required] private RectTransform buttonRect;

        private bool prevRotated = false;
        
        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("item_key", () => this.State.ItemKey, SharedConstants.Game.Items.WEAPON_AR),
            this.Variable("item_icon", () => this.State.ItemIcon, SharedConstants.Game.Items.WEAPON_AR),
            this.Variable("item_rarity", () => this.State.ItemRarity, ERarityType.Common),
            this.Variable("item_weight", () => this.State.Weight, 1),
            this.Variable("item_quality", () => this.State.Quality, 999),
            this.Variable("can_take", () => this.State.CanTake, true),
            this.Variable("is_take_visible", () => this.State.IsTakeVisible, true),
        };

        protected override AutoViewEventBinding[] Events => new[] {
            this.Event("close", () => this.State.Close()),
            this.Event("take", () => this.State.Take()),
            this.Event("rotate", () => this.State.Rotate()),
        };

        protected override void Render() {
            base.Render();

            this.statsPanel.Render(this.State.Stats);
            
            WidgetPosition.SetPosition(this.root, this.State.WidgetPosition);
        }

        private void Update() {
            if (this.prevRotated != this.State.Rotated) {
                this.prevRotated = this.State.Rotated;
                
                this.buttonRect.DOLocalRotate(this.State.Rotated ? Vector3.forward * -90 : Vector3.zero, 0.5f);
            }
        }
    }

    public interface IItemInfoState : IViewState {
        string ItemKey    { get; }
        string ItemIcon   { get; }
        string ItemRarity { get; }

        float Weight  { get; }
        int   Quality { get; }

        bool CanTake       { get; }
        bool IsTakeVisible { get; }
        bool Rotated       { get; }

        IState Stats { get; }

        WidgetPosition.Position WidgetPosition { get; }

        void Close();
        void Take();
        void Rotate();
    }
}