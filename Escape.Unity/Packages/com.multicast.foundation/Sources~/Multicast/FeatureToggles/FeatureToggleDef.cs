namespace Multicast.FeatureToggles {
    using System;
    using System.Collections.Generic;
    using DirtyDataEditor;
    using ExpressionParser;
    using JetBrains.Annotations;

    [Serializable, DDEObject]
    public class FeatureToggleDef : Def {
        [DDE("default", null), CanBeNull] public FeatureToggleValueDef defaults;

        [DDE("platform", DDE.Empty)] public Dictionary<string, FeatureToggleValueDef> platforms;
        [DDE("variant", DDE.Empty)] public Dictionary<string, FeatureToggleValueDef> variants;
    }

    [Serializable, DDEObject]
    public class FeatureToggleValueDef {
        [DDE("formula", null)] public FormulaInt formula;
        [DDE("text", null)] public string text;
    }
}