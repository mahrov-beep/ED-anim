namespace Multicast.Localization {
    using System;
    using System.Linq;
    using Sirenix.OdinInspector;
    using UnityEngine;

    public class LocalizationTable : ScriptableObject {
#if UNITY_EDITOR
        private static readonly string[] ValidClosingTags = {
            "</align>",
            "</allcaps>",
            "</alpha>",
            "</b>",
            "</color>",
            "</cspace>",
            "</font>",
            "</font-weight>",
            "</gradient>",
            "</i>",
            "</indent>",
            "</line-height>",
            "</line-indent>",
            "</link>",
            "</lowercase>",
            "</margin>",
            "</mark>",
            "</mspace>",
            "</nobr>",
            "</noparse>",
            "</page>",
            "</pos>",
            "</rotate>",
            "</s>",
            "</size>",
            "</smallcaps>",
            "</space>",
            "</sprite>",
            "</style>",
            "</sub>",
            "</sup>",
            "</u>",
            "</uppercase>",
            "</voffset>",
            "</width>",
        };
#endif

        [SerializeField, Required]
        private string page;

        [SerializeField, Required]
        private string lang;

        [SerializeField, Required]
        [ValidateInput(nameof(ValidateValues))]
        private string[] values;

        public string   Page   => this.page;
        public string   Lang   => this.lang;
        public string[] Values => this.values;

        internal void SetPage(string newPage)       => this.page = newPage;
        internal void SetLang(string newLang)       => this.lang = newLang;
        internal void SetValues(string[] newValues) => this.values = newValues;

        private bool ValidateValues(string[] arr, ref string message) {
#if UNITY_EDITOR
            for (var textIndex = 0; textIndex < arr.Length; textIndex++) {
                var text = arr[textIndex];
                if (string.IsNullOrEmpty(text)) {
                    continue;
                }

                if (text.Contains("&#")) {
                    message = $"Values[{textIndex}]: {text} contains '&#', looks like incorrect html";
                    return false;
                }

                var tagIndex = 0;
                do {
                    tagIndex = text.IndexOf("</", tagIndex + 1, StringComparison.Ordinal);

                    if (tagIndex >= 0 &&
                        text.Substring(tagIndex) is var substr &&
                        ValidClosingTags.All(t => !substr.StartsWith(t))) {
                        var messageDetail = substr.IndexOf(">", StringComparison.Ordinal) is var end && end >= 0
                            ? substr.Substring(0, end + 1)
                            : substr;

                        message = $"Values[{textIndex}]: {text} contains '{messageDetail}', looks like incorrect html";
                        return false;
                    }
                } while (tagIndex != -1);

                if (this.lang == "EN") {
                    foreach (var c in text) {
                        if (c > 128 && char.IsLetter(c)) {
                            message = $"Values[{textIndex}]: {text} contains non-english '{c}'";
                            return false;
                        }
                    }
                }
            }
#endif

            return true;
        }
    }
}