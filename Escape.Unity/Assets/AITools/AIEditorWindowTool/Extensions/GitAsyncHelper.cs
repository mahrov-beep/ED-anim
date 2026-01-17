using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace AITools.AIEditorWindowTool {
    public static class GitAsyncHelper {
        static readonly string WorkingDirectory = Application.dataPath + "/..";

        public static Task AddAsync(IEnumerable<string> paths, CancellationToken ct) {
            return RunGitAsync($"add {Quote(paths)}", ct);
        }

        public static Task AddAsync(CancellationToken ct, params string[] paths) {
            return AddAsync(paths, ct);
        }

        public static Task CommitAsync(string message, CancellationToken ct) {
            return RunGitAsync($"commit -m \"{message}\"", ct);
        }

        public static Task RestoreAsync(IEnumerable<string> paths, CancellationToken ct) {
            return RunGitAsync($"restore {Quote(paths)}", ct);
        }

        public static Task RestoreAsync(CancellationToken ct, params string[] paths) {
            return RestoreAsync((IEnumerable<string>)paths, ct);
        }

        public static async Task<bool> HasChangesAsync(IEnumerable<string> paths, CancellationToken ct) {
            var (output, _) = await RunGitWithOutputAsync($"status --porcelain {Quote(paths)}", ct)
                            .ConfigureAwait(false);
            return !string.IsNullOrEmpty(output);
        }

        public static Task<bool> HasChangesAsync(CancellationToken ct, params string[] paths) {
            return HasChangesAsync((IEnumerable<string>)paths, ct);
        }

        private static async Task RunGitAsync(string args, CancellationToken ct) {
            await RunGitWithOutputAsync(args, ct).ConfigureAwait(false);
        }

        public static async Task<(string output, string error)> RunGitWithOutputAsync(string args, CancellationToken ct) {
            var psi = new ProcessStartInfo("git", args) {
                            WorkingDirectory       = WorkingDirectory,
                            RedirectStandardOutput = true,
                            RedirectStandardError  = true,
                            CreateNoWindow         = true,
                            UseShellExecute        = false,
            };

            using var process = new Process();

            process.StartInfo           = psi;
            process.EnableRaisingEvents = true;
            var tcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

            process.Exited += (sender, _) => {
                var p = (Process)sender!;
                tcs.TrySetResult(p.ExitCode);
            };

            ct.Register(static state => {
                var p = (Process)state!;
                try {
                    if (!p.HasExited) {
                        p.Kill();
                    }
                }
                catch (InvalidOperationException ex) {
                    Debug.LogException(ex);
                }
                catch (Win32Exception ex) {
                    Debug.LogException(ex);
                }
            }, process);

            process.Start();

            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask  = process.StandardError.ReadToEndAsync();

            await tcs.Task.ConfigureAwait(false);

            var output = await outputTask.ConfigureAwait(false);
            var error  = await errorTask.ConfigureAwait(false);

            if (!string.IsNullOrEmpty(error)) {
                Debug.LogError(error);
            }
            else if (!string.IsNullOrEmpty(output)) {
                Debug.Log(output);
            }

            return (output, error);
        }

        private static string Quote(IEnumerable<string> paths) {
            return string.Join(" ", paths.Select(p => $"\"{p}\""));
        }

        public struct GitChange {
            public char   Status;  // A,M,D,R,C
            public string Path;    // текущий путь
            public string OldPath; // прежний путь, если R

            public GitChange(char status, string path, string oldPath = null) {
                Status  = status;
                Path    = path;
                OldPath = oldPath;
            }
        }

        public static async Task<List<GitChange>> GetCommitChangesAsync(
                        string commitId,
                        bool skipRenames = false,
                        bool skipMoves = false,
                        CancellationToken ct = default) {

            var diffFilter = "ACDMR"; // A‑dded C‑opy D‑elete M‑odify R‑ename/Move
            if (skipRenames || skipMoves) {
                diffFilter = diffFilter.Replace("R", string.Empty);
            }

            var (output, _) = await RunGitWithOutputAsync(
                                            $"show --pretty=\"\" --name-status --diff-filter={diffFilter} {commitId}", ct)
                            .ConfigureAwait(false);

            var changes = new List<GitChange>();
            foreach (var line in output.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)) {
                var parts = line.Split('\t');
                if (parts.Length < 2) {
                    continue;
                }

                var status = parts[0][0];
                if (status == 'R' && parts.Length >= 3) {
                    changes.Add(new GitChange(status, parts[2], parts[1])); // R  oldPath -> newPath
                }
                else {
                    changes.Add(new GitChange(status, parts[1]));
                }
            }
            return changes;
        }

        public static string GetCurrentBranchSync() {
            try {
                var gitPath = System.IO.Path.Combine(WorkingDirectory, ".git");

                string gitDir;
                if (System.IO.File.Exists(gitPath)) {
                    var gitFileContent = System.IO.File.ReadAllText(gitPath).Trim();
                    if (gitFileContent.StartsWith("gitdir: ")) {
                        gitDir = gitFileContent.Substring("gitdir: ".Length).Trim();
                        if (!System.IO.Path.IsPathRooted(gitDir)) {
                            gitDir = System.IO.Path.Combine(WorkingDirectory, gitDir);
                        }
                    } else {
                        return null;
                    }
                } else if (System.IO.Directory.Exists(gitPath)) {
                    gitDir = gitPath;
                } else {
                    return null;
                }

                var gitHeadPath = System.IO.Path.Combine(gitDir, "HEAD");
                if (!System.IO.File.Exists(gitHeadPath)) {
                    return null;
                }

                var headContent = System.IO.File.ReadAllText(gitHeadPath).Trim();

                if (headContent.StartsWith("ref: refs/heads/")) {
                    return headContent.Substring("ref: refs/heads/".Length);
                }

                if (headContent.Length >= 7) {
                    return headContent.Substring(0, 7);
                }

                return headContent;
            }
            catch {
                return null;
            }
        }
    }
}