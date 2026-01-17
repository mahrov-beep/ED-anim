namespace Multicast {
    using Cysharp.Threading.Tasks;
    using JetBrains.Annotations;
    using NativeDialogInternal;

    public static class NativeDialog {
        [PublicAPI]
        public static UniTask<bool> OkCancel(string title, string message, string ok, string cancel) {
            var tcs = new UniTaskCompletionSource<bool>();
            DialogManager.SetLabel(ok, cancel, string.Empty);
            DialogManager.ShowSelect(title, message, result => tcs.TrySetResult(result));
            return tcs.Task;
        }

        [PublicAPI]
        public static UniTask<bool> Confirm(string title, string message, string close) {
            var tcs = new UniTaskCompletionSource<bool>();
            DialogManager.SetLabel(string.Empty, string.Empty, close);
            DialogManager.ShowSubmit(title, message, result => tcs.TrySetResult(result));
            return tcs.Task;
        }
    }
}