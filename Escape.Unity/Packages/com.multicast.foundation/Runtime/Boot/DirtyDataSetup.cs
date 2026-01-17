namespace Multicast.Boot {
    using System.Collections.Generic;
    using Boosts;
    using DirtyDataEditor;
    using ExpressionParser;
    using Numerics;
    using UnityEngine.Scripting;

    internal static class DirtyDataSetup {
        [Preserve]
#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
#endif
        private static void RegisterDefaultValue() {
            DirtyDataTypeActivator.Register(() => new List<int>());
            DirtyDataTypeActivator.Register(() => new List<string>());
            DirtyDataTypeActivator.Register(() => new List<float>());
            DirtyDataTypeActivator.Register(() => new List<double>());
            DirtyDataTypeActivator.Register(() => new List<BigDouble>());
            DirtyDataTypeActivator.Register(() => new List<ProtectedBigDouble>());
            DirtyDataTypeActivator.Register(() => new List<ProtectedInt>());
            DirtyDataTypeActivator.Register(() => new List<FormulaBigDouble>());
            DirtyDataTypeActivator.Register(() => new List<FormulaFloat>());
            DirtyDataTypeActivator.Register(() => new List<FormulaInt>());
            DirtyDataTypeActivator.Register(() => new List<FormulaPredicate>());
            DirtyDataTypeActivator.Register(() => new List<FormulaIntList>());
            DirtyDataTypeActivator.Register(() => new List<FormulaFloatList>());
            DirtyDataTypeActivator.Register(() => new List<FormulaBigDoubleList>());
            DirtyDataTypeActivator.Register(() => new List<BoostTag>());

            DirtyDataTypeActivator.Register(() => new Dictionary<string, int>());
            DirtyDataTypeActivator.Register(() => new Dictionary<string, string>());
            DirtyDataTypeActivator.Register(() => new Dictionary<string, float>());
            DirtyDataTypeActivator.Register(() => new Dictionary<string, double>());
            DirtyDataTypeActivator.Register(() => new Dictionary<string, BigDouble>());
            DirtyDataTypeActivator.Register(() => new Dictionary<string, ProtectedBigDouble>());
            DirtyDataTypeActivator.Register(() => new Dictionary<string, ProtectedInt>());
            DirtyDataTypeActivator.Register(() => new Dictionary<string, FormulaBigDouble>());
            DirtyDataTypeActivator.Register(() => new Dictionary<string, FormulaFloat>());
            DirtyDataTypeActivator.Register(() => new Dictionary<string, FormulaInt>());
            DirtyDataTypeActivator.Register(() => new Dictionary<string, FormulaPredicate>());
            DirtyDataTypeActivator.Register(() => new Dictionary<string, FormulaIntList>());
            DirtyDataTypeActivator.Register(() => new Dictionary<string, FormulaFloatList>());
            DirtyDataTypeActivator.Register(() => new Dictionary<string, FormulaBigDoubleList>());
            DirtyDataTypeActivator.Register(() => new Dictionary<string, BoostTag>());
        }
    }
}