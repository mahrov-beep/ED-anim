namespace Multicast.DirtyDataEditor.Patcher {
    using System;

    [Serializable]
    public struct DdePatch {
        public string tableName;
        public string key;
        public string property;
        public string value;
    }
}