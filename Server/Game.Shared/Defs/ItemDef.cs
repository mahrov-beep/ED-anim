namespace Game.Shared.Defs {
    using System.Collections.Generic;
    using Multicast;
    using Multicast.DirtyDataEditor;
    using Photon.Deterministic;
    using Quantum;

    [DDEObject]
    public class ItemDef : Def, IItemDef {
        [DDE("type")]
        public ItemTypes Type;

        [DDE("rarity")]
        public ERarityType Rarity;

        [DDE("sell_cost", DDE.Empty)]
        public Dictionary<string, int> SellCost;

        [DDE("buy_cost", DDE.Empty)]
        public Dictionary<string, int> BuyCost;

        [DDE("min_trader_level_to_buy", 0)]
        public int MinTraderLevelToBuy;

        [DDE("quantity_in_trader_shop", 1)]
        public int QuantityInTraderShop;

        [DDE("weight")]
        public FP Weight;

        [DDE("cells_width", 1)]  public int CellsWidth;
        [DDE("cells_height", 1)] public int CellsHeight;

        [DDE("ammo_type", AmmoTypes.Invalid)] 
        [DDENonNullWhen(nameof(Type), ItemTypes.Weapon)]
        [DDENonNullWhen(nameof(Type), ItemTypes.AmmoBox)]
        public AmmoTypes AmmoType;

        [DDE("allow_merge", false)]
        public bool AllowMerge;
        
        [DDE("max_usages", 0)]
        [DDENonNullWhen(nameof(AllowMerge), true)]
        public int MaxUsages;

        [DDE("equip_priority", 0)]
        public FP EquipPriority;

        public int Quality => this.EquipPriority.AsInt;

        ERarityType IItemDef.Rarity        => this.Rarity;
        FP IItemDef.         Weight        => this.Weight;
        int IItemDef.        CellsWidth    => this.CellsWidth;
        int IItemDef.        CellsHeight   => this.CellsHeight;
        FP IItemDef.         EquipPriority => this.EquipPriority;
        int IItemDef.        Quality       => this.Quality;
        AmmoTypes IItemDef.  AmmoType      => this.AmmoType;
        bool IItemDef.       AllowMerge    => this.AllowMerge;
        int IItemDef.        MaxUsages     => this.MaxUsages;
    }
}