namespace Multicast {
    using System;
    using System.Collections.Generic;
    using Collections;
    using Sirenix.OdinInspector;
    using UnityEngine;
    using UnityEngine.Pool;

    public abstract class DefAsset<TDef> : ScriptableObject where TDef : Def {
        [SerializeField]
        [TableList(AlwaysExpanded = true, ShowPaging = false, ShowIndexLabels = true)]
        [ValidateInput(nameof(ValidateDefs))]
        protected List<TDef> defs = new List<TDef>();

        private LookupCollection<TDef> cachedLookup;

        public LookupCollection<TDef> GetLookup() {
            if (this.cachedLookup != null) {
                return this.cachedLookup;
            }

            try {
                return this.cachedLookup = new LookupCollection<TDef>(this.defs);
            }
            catch {
                Debug.LogError($"Failed to create lookup for '{this.name}'", this);
                throw;
            }
        }

        private void OnValidate() {
            this.cachedLookup = null;
        }

        protected virtual bool ValidateDefs(List<TDef> defs, ref string errorMessage, ref InfoMessageType? messageType) {
            using (ListPool<string>.Get(out var names)) {
                for (var index = 0; index < defs.Count; index++) {
                    var def = defs[index];

                    if (names.Contains(def.key)) {
                        errorMessage = $"Duplicate key found: '{def.key}' at index {index}";
                        messageType  = InfoMessageType.Error;
                        return false;
                    }

                    names.Add(def.key);
                }
            }

            return true;
        }
    }
}