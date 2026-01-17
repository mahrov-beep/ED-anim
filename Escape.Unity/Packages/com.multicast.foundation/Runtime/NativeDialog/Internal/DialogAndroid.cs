#if UNITY_ANDROID

using UnityEngine;

namespace Multicast.NativeDialogInternal {
    internal sealed class DialogAndroid : IDialog {
        private readonly AndroidJavaClass cls;

        public DialogAndroid() {
            this.cls = new AndroidJavaClass("unity.plugins.dialog.DialogManager");
        }

        public void Dispose() {
            this.cls.Dispose();
        }

        public void SetLabel(string decide, string cancel, string close) {
            this.cls.CallStatic("SetLabel", decide, cancel, close);
        }

        public int ShowSelect(string message) {
            return this.cls.CallStatic<int>("ShowSelectDialog", message);
        }

        public int ShowSelect(string title, string message) {
            return this.cls.CallStatic<int>("ShowSelectTitleDialog", title, message);
        }

        public int ShowSubmit(string message) {
            return this.cls.CallStatic<int>("ShowSubmitDialog", message);
        }

        public int ShowSubmit(string title, string message) {
            return this.cls.CallStatic<int>("ShowSubmitTitleDialog", title, message);
        }

        public void Dissmiss(int id) {
            this.cls.CallStatic("DissmissDialog", id);
        }
    }
}

#endif // UNITY_ANDROID