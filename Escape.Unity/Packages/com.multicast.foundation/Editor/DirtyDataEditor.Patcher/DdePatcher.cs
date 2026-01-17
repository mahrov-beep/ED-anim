namespace Multicast.DirtyDataEditor.Patcher {
    using System;
    using Collections;
    using JetBrains.Annotations;
    using UnityEngine;

    public partial class DdePatcher<TGameDef> {
        private readonly Color? backgroundColor;

        private readonly DdePatchBuilder builder = new DdePatchBuilder();

        public DdePatcher(Color? backgroundColor = null) {
            this.backgroundColor = backgroundColor;
        }

        [PublicAPI]
        public DdePatcherTable<TItem> GetTable<TItem>(Func<TGameDef, LookupCollection<TItem>> selector, string tableName)
            where TItem : Def {
            return new DdePatcherTable<TItem>(this.builder, tableName);
        }

        public override string ToString() {
            return JsonUtility.ToJson(this.builder, true);
        }
    }
}