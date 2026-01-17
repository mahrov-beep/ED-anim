namespace Multicast.NativeDialogInternal {
    internal class DialogNull : IDialog {
        public void Dispose() {
        }

        public void SetLabel(string decide, string cancel, string close) {
        }

        public int ShowSelect(string message) {
            return 0;
        }

        public int ShowSelect(string title, string message) {
            return 0;
        }

        public int ShowSubmit(string message) {
            return 0;
        }

        public int ShowSubmit(string title, string message) {
            return 0;
        }

        public void Dissmiss(int id) {
        }
    }
}