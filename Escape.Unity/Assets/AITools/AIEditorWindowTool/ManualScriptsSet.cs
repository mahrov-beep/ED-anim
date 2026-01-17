namespace AITools.AIEditorWindowTool {
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    [CreateAssetMenu(
                    fileName = "ManualScriptsSet",
                    menuName = "AI/Manual Scripts Set",
                    order = 1000)]
    public class ManualScriptsSet : ScriptableObject {
        [field: SerializeField]
        public List<MonoScript> Scripts { get; private set; } = new();

        private void OnValidate() {
            Scripts.RemoveAll(s => !s);
            Scripts = Scripts.Distinct().ToList();
        }
    }
}