namespace Quantum {
    using Photon.Deterministic;

    public interface IItemDef {
        ERarityType Rarity { get; }

        FP  Weight { get; }

        int CellsWidth  { get; }
        int CellsHeight { get; }

        FP  EquipPriority { get; }
        int Quality       { get; }

        AmmoTypes AmmoType { get; }

        bool AllowMerge { get; }
        int  MaxUsages  { get; }
    }
}