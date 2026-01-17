namespace Multicast.DirtyDataEditor.Patcher {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    [Serializable]
    public class DdePatchBuilder {
        [SerializeField]
        private List<DdePatch> patches = new List<DdePatch>();

        public void Append(string tableName, string key, string property, string value) {
            this.patches.Add(new DdePatch {
                tableName = tableName,
                key       = key,
                property  = property,
                value     = value,
            });
        }

        public int PatchesCount => this.patches.Count;

        public IEnumerable<string> EnumeratePatchedTables() {
            return this.patches.Select(it => it.tableName).Distinct();
        }

        public IEnumerable<DdePatch> EnumerateAllByTableName(string tableName) {
            return this.patches.Where(it => it.tableName == tableName);
        }
    }
}