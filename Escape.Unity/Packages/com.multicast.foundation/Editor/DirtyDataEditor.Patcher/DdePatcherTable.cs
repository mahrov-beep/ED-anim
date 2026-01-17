namespace Multicast.DirtyDataEditor.Patcher {
    using JetBrains.Annotations;

    public readonly struct DdePatcherTable<TItem> {
        private readonly DdePatchBuilder builder;

        private readonly string tableName;

        public DdePatcherTable(DdePatchBuilder builder, string tableName) {
            this.builder   = builder;
            this.tableName = tableName;
        }

        [PublicAPI]
        public DdePatcherRow<TItem> this[string key] => new DdePatcherRow<TItem>(this.builder, this.tableName, key);
    }
}