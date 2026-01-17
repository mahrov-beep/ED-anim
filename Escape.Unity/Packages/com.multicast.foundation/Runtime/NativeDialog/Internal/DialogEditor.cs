#if UNITY_EDITOR

namespace Multicast.NativeDialogInternal {
    using UnityEditor;

    internal class DialogEditor : IDialog {
        private readonly IDialogReceiver receiver;

        private string decideLabel;
        private string cancelLabel;
        private string closeLabel;

        private int id;

        public DialogEditor(IDialogReceiver receiver) {
            this.receiver = receiver;
        }

        public void Dispose() {
        }

        public void SetLabel(string decide, string cancel, string close) {
            this.decideLabel = decide;
            this.cancelLabel = cancel;
            this.closeLabel  = close;
        }

        public int ShowSelect(string message) {
            var newID = ++this.id;

            EditorApplication.delayCall += () => {
                if (EditorUtility.DisplayDialog(string.Empty, message, this.decideLabel, this.cancelLabel)) {
                    this.receiver.OnSubmit(newID.ToString());
                }
                else {
                    this.receiver.OnCancel(newID.ToString());
                }
            };

            return newID;
        }

        public int ShowSelect(string title, string message) {
            var newID = ++this.id;

            EditorApplication.delayCall += () => {
                if (EditorUtility.DisplayDialog(title, message, this.decideLabel, this.cancelLabel)) {
                    this.receiver.OnSubmit(newID.ToString());
                }
                else {
                    this.receiver.OnCancel(newID.ToString());
                }
            };

            return newID;
        }

        public int ShowSubmit(string message) {
            var newID = ++this.id;

            EditorApplication.delayCall += () => {
                EditorUtility.DisplayDialog(string.Empty, message, this.closeLabel);
                this.receiver.OnSubmit(newID.ToString());
            };

            return newID;
        }

        public int ShowSubmit(string title, string message) {
            var newID = ++this.id;

            EditorApplication.delayCall += () => {
                EditorUtility.DisplayDialog(title, message, this.closeLabel);
                this.receiver.OnSubmit(newID.ToString());
            };

            return newID;
        }

        public void Dissmiss(int id) {
        }
    }
}

#endif