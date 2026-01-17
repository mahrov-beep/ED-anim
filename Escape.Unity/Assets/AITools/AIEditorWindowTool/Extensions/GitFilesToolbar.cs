namespace AITools.AIEditorWindowTool {
    using System;
    using System.Threading;
    using UnityEditor;
    using UnityEngine;
    using static GitAsyncHelper;

    public class GitFilesToolbar {
        private readonly string[]     files;
        private readonly Func<string> getCommitMessage;

        public GitFilesToolbar(string[] files, Func<string> getCommitMessage) {
            this.files            = files;
            this.getCommitMessage = getCommitMessage;
        }

        public void Draw() {
            EditorGUILayout.BeginHorizontal();
            // EditorGUI.BeginDisabledGroup(!_isFileModified());

            if (GUILayout.Button("Git Commit"))  Commit();
            if (GUILayout.Button("Reset Files")) Reset();

            // EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(20);
        }

        private async void Commit() {
            try {
                await AddAsync(files, CancellationToken.None);
                await CommitAsync(getCommitMessage(), CancellationToken.None);
            }
            catch (Exception e) {
                Debug.LogException(e);
            }
            finally { }
        }

        private async void Reset() {
            try {
                await RestoreAsync(files, CancellationToken.None);
            }
            catch (Exception e) {
                Debug.LogException(e);
            }
            finally { }
        }
    }
}