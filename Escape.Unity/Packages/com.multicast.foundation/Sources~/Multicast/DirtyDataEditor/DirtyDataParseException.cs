namespace Multicast.DirtyDataEditor {
    using System;

    public class DirtyDataParseException : Exception {
        public DirtyDataParseException(string message, string sourceText = null)
            : base(message + FormatSourceTextEditorOnly(sourceText)) {
        }

        public DirtyDataParseException(string message, Exception ex, string sourceText = null)
            : base(message + ": " + ex.Message + FormatSourceTextEditorOnly(sourceText)) {
        }

        private static string FormatSourceTextEditorOnly(string text) {
            if (string.IsNullOrEmpty(text)) {
                return "";
            }

            if (!MulticastLog.IsDebugLogEnabled) {
                return "";
            }

            return "\n\n" + text.Trim() + "\n";
        }
    }
}