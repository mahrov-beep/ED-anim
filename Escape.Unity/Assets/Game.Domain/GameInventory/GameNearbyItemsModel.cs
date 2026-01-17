namespace Game.Domain.GameInventory {
    using System;
    using System.Collections.Generic;
    using Multicast;
    using Quantum;
    using UniMob;
    using UnityEngine;

    public class GameNearbyItemsModel : Model {
        private readonly AtomEntityRefList<GameNearbyItemModel> nearbyItems;

        public GameNearbyItemsModel(Lifetime lifetime) : base(lifetime) {
            this.nearbyItems = new AtomEntityRefList<GameNearbyItemModel>(lifetime, () => new GameNearbyItemModel(), it => ref it.Frame, it => ref it.ItemEntity);
            
            this.NearbyItemBox  = new GameNearbyItemBoxModel(lifetime, isBackpack: false);
            this.NearbyBackpack = new GameNearbyItemBoxModel(lifetime, isBackpack: true);
        }

        [Atom] public GameNearbyItemBoxModel NearbyItemBox  { get; set; }
        [Atom] public GameNearbyItemBoxModel NearbyBackpack { get; set; }

        // use NearbyItemBox/NearbyBackpack instead
        [Atom, Obsolete] public EntityRef NearbyItemEntity         { get; set; }
        [Atom, Obsolete] public EntityRef NearbyBackpackItemEntity { get; set; }
        [Atom, Obsolete] public EntityRef OpenedNearbyItemEntity   { get; set; }
        [Atom, Obsolete] public bool      IsOpenedByOtherPlayer    { get; set; }
        [Atom, Obsolete] public float     ItemBoxTimer             { get; set; }
        [Atom, Obsolete] public float     ItemBoxTime              { get; set; }

        public List<GameNearbyItemModel> EnumerateNearbyItems() {
            return this.nearbyItems.AsList;
        }

        public GameNearbyItemModel UpdateNearbyItem(int frameNum, EntityRef itemEntity) {
            return this.nearbyItems.GetAndRefresh(frameNum, itemEntity);
        }

        public void DeleteOutdated(int frameNum) {
            this.nearbyItems.DeleteOutdatedItems(frameNum);
        }
    }

    public class GameNearbyItemModel {
        public EntityRef ItemEntity;
        public int       Frame;

        public MutableAtom<bool> HasEnoughSpaceInInventory { get; } = Atom.Value(false);
    }
    
    public class GameNearbyItemBoxModel : Model {
        public bool IsBackpack { get; }

        [Atom] public EntityRef Entity { get; set; }

        [Atom] public bool  IsOpenedByMe          { get; set; }
        [Atom] public bool  IsOpenedByOtherPlayer { get; set; }
        [Atom] public float ItemBoxTimer          { get; set; }
        [Atom] public float ItemBoxTime           { get; set; }

        public bool CanEquipBest => !this.IsOpenedByOtherPlayer && this.ItemBoxTimer < 0.05f;
        public bool CanOpen      => !this.IsOpenedByOtherPlayer;

        public GameNearbyItemBoxModel(Lifetime lifetime, bool isBackpack) : base(lifetime) {
            this.IsBackpack = isBackpack;
        }
    }
}