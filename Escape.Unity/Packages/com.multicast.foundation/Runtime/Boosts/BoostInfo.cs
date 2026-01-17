namespace Multicast.Boosts {
    using System;

    [Serializable]
    public struct BoostInfo {
        public string category;
        public string scopeKey;
        public string scope;
        public string localizationKey;

        public object Tag;

        public BoostInfo(string category, string localizationKey) {
            this.category        = category;
            this.localizationKey = localizationKey;
            this.Tag             = null;
            this.scope           = null;
            this.scopeKey     = null;
        }
        
        public BoostInfo(string category, string scope, string scopeKey, string localizationKey = null) {
            this.category        = category;
            this.scope           = scope;
            this.scopeKey        = scopeKey;
            this.localizationKey = localizationKey;
            this.Tag             = null;
        }
    }
}