namespace AITools.AIEditorWindowTool {
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    [CreateAssetMenu(fileName = "PromptTemplate", menuName = "GPT/Prompt Template")]
    public class PromptTemplate : ScriptableObject {
        [Serializable] public struct Line {
            public            string role;
            [TextArea(3, 30)] public string content;
        }

        public List<Line> systemInstructions = new() {
                        new() {
                                        role    = "system",
                                        content = "You are a helpful assistant.",
                        },
        };
    }
}