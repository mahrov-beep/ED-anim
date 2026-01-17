namespace Game.UI.Controllers.Features.SelectedItemInfo {
    using Multicast;
    using Quantum;

    public static class SelectedItemInfoFeatureEvents {
        public static readonly EventSource Close = new();

        public static readonly EventSource<SelectArgs> Select = new();

        [RequireFieldsInit]
        public struct SelectArgs {
            public string ItemKey;

            public EntityRef               ItemEntity;
            public GameSnapshotLoadoutItem ItemSnapshot;

            public WidgetPosition.Position Position;

            public bool IsTakeButtonVisible;
        }
    }
}