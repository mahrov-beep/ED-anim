namespace AITools.AIEditorWindowTool {
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Проверяет список файлов (и их .meta) на наличие изменений в гите.
    /// Содержит только состояние, без событий/колбэков.
    /// </summary>
    public class FileChangesMonitor : IDisposable {
        private readonly List<string>            files; // Список путей к файлам (без .meta)
        private          CancellationTokenSource cts;
        private          Task                    monitorTask;
        private readonly object                  @lock = new();

        private volatile bool  isModified;
        private volatile float lastCheckTime;

        /// <summary>
        /// true если обнаружены изменения в любом из файлов (или их .meta)
        /// </summary>
        public bool IsModified => isModified;

        /// <summary>
        /// Время последней проверки (EditorApplication.timeSinceStartup)
        /// </summary>
        public double LastCheckTime => lastCheckTime;

        /// <param name="files">Список путей к файлам для мониторинга (например, ".cs" и ".txt" и т.п.)</param>
        public FileChangesMonitor(IEnumerable<string> files) {
            if (files == null)
                throw new ArgumentNullException(nameof(files));

            this.files = new List<string>(files);

            if (this.files.Count == 0)
                throw new ArgumentException("FileChangesMonitor: files list is empty.");
        }

        public void Dispose() {
            CancelCurrentTask();
            GC.SuppressFinalize(this);
        }

        public void CancelCurrentTask() {
            lock (@lock) {
                if (cts != null) {
                    cts.Cancel();
                    cts.Dispose();
                    cts = null;
                }
                monitorTask = null;
            }
        }

        public async Task UpdateFileModifiedCacheAsync() {
            CancelCurrentTask();
            var cts = new CancellationTokenSource();
            lock (@lock) {
                this.cts = cts;
            }
            var token = cts.Token;

            var filesToCheck = new List<string>();
            try {
                foreach (var file in files) {
                    if (string.IsNullOrWhiteSpace(file))
                        throw new ArgumentException("FileChangesMonitor: file path is null or empty.");
                    filesToCheck.Add(file);
                    filesToCheck.Add(file + ".meta");
                }
            }
            catch (Exception ex) {
                Debug.LogError($"FileChangesMonitor: Error preparing file list: {ex}");
                lock (@lock) {
                    if (this.cts == cts) {
                        this.cts    = null;
                        monitorTask = null;
                    }
                }
                return;
            }

            var checkTime = EditorApplication.timeSinceStartup;
            try {
                var hasChanges = await GitAsyncHelper.HasChangesAsync(token, filesToCheck.ToArray());

                isModified    = hasChanges;
                lastCheckTime = (float)checkTime;
            }
            catch (OperationCanceledException) {
                Debug.Log("FileChangesMonitor: UpdateFileModifiedCacheAsync was canceled");
            }
            catch (Exception e) {
                Debug.LogError(e);
            }
            finally {
                lock (@lock) {
                    if (this.cts == cts) {
                        this.cts    = null;
                        monitorTask = null;
                    }
                }
            }
        }

        public void UpdateFileModifiedCache() {
            lock (@lock) {
                if (monitorTask != null && !monitorTask.IsCompleted && !monitorTask.IsCanceled && !monitorTask.IsFaulted)
                    return;

                monitorTask = UpdateFileModifiedCacheAsync();
                _           = monitorTask;
            }
        }
    }

}