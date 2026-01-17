namespace Multicast.DirtyDataEditor {
    using System;
    using System.Linq;
    using System.Reflection;
    using Collections;
    using Sirenix.OdinInspector;
    using UnityEngine;
    using TextAsset = Multicast.TextAsset;

    public abstract class DirtyDataValidationAsset : ScriptableObject {
        public abstract object Parse(IEnumerableCache<TextAsset> cache);

        public virtual void Validate(object def, SelfValidationResult result) {
        }

        protected static void ValidateFields<TDef>(SelfValidationResult result, Type type, DefAsset<TDef> defAsset)
            where TDef : Def {
            ValidateFields<TDef, string>(result, type, defAsset.GetLookup(), it => it);
        }
        
        protected static void ValidateFields<TDef>(SelfValidationResult result, Type type, LookupCollection<TDef> defs)
            where TDef : Def {
            ValidateFields<TDef, string>(result, type, defs, it => it);
        }

        protected static void ValidateFields<TDef, TFieldValue>(SelfValidationResult result, Type type, LookupCollection<TDef> defs,
            Func<TFieldValue, string> fieldMapping)
            where TDef : Def {
            var missingValues = type.GetFields(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(it => it.FieldType == typeof(TFieldValue))
                .Select(it => fieldMapping.Invoke((TFieldValue) it.GetValue(null)))
                .Where(it => !defs.TryGet(it, out _));

            foreach (var missingValue in missingValues) {
                result.AddError($"'{missingValue}' declared in '{type.FullName}' class but missing in DDE");
            }
        }
    }
}