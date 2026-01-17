namespace Multicast.DirtyDataEditor.Patcher {
    using System;
    using JetBrains.Annotations;
    using Newtonsoft.Json;

    public readonly struct DdePatcherValue<T> {
        private readonly DdePatchBuilder builder;

        private readonly string tableName;
        private readonly string key;
        private readonly string property;

        public DdePatcherValue(DdePatchBuilder builder, string tableName, string key, string property) {
            this.builder   = builder;
            this.tableName = tableName;
            this.key       = key;
            this.property  = property;
        }

        [PublicAPI]
        public void Set(T value) {
            this.builder.Append(this.tableName, this.key, this.property, value.ToString());
        }

        [PublicAPI]
        public void SetRaw(string value) {
            this.builder.Append(this.tableName, this.key, this.property, value);
        }

        [PublicAPI]
        public void SetJsonObject(dynamic obj) {
            this.builder.Append(this.tableName, this.key, this.property, JsonConvert.SerializeObject(obj));
        }

        [PublicAPI]
        public void SetJsonFromBuilder(Action<dynamic> b) {
            this.SetJsonObject(DdeJsonBuilder.New(b));
        }
    }
}