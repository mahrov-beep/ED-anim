namespace Multicast.ScriptingDefinesManagement {
    using System;
    using System.Diagnostics;
    using JetBrains.Annotations;

    [PublicAPI]
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class ScriptingDefineSuggestionAttribute : Attribute {
        public ScriptingDefineSuggestionAttribute(string define) {
            this.Define = define;
        }

        public string Define { get; }
    }
}