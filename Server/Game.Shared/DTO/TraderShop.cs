namespace Game.Shared.DTO {
    using System.Collections.Generic;
    using MessagePack;
    using Quantum;

    [MessagePackObject, RequireFieldsInit]
    public class TraderShopState {
        [Key(0)] public List<GameSnapshotLoadoutItem> TradedItems;
    }
}