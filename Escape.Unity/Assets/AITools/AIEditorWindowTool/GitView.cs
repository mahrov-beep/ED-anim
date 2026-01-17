using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace AITools.AIEditorWindowTool
{
    public class GitView : AIEditorWindow
    {
        private List<string> branches = new List<string>();
        private int selectedBranchIndex = 0;
        private List<CommitData> commitList = new List<CommitData>();
        private CancellationTokenSource cts;
        private int commitCount = 10;
        private Vector2 scrollPos;

        private class CommitData
        {
            public string CommitId;
            public string Message;
            public string Author;
            public string Date;
            public bool Expanded;
            public List<GitAsyncHelper.GitChange> Changes = new List<GitAsyncHelper.GitChange>();
            public bool LoadedChanges = false;
        }

        [MenuItem("GPTGenerated/" + nameof(GitView))]
        public static void ShowWindow()
        {
            GetWindow<GitView>(nameof(GitView));
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            cts = new CancellationTokenSource();
            UpdateBranches();
        }

        protected override void OnDisable()
        {
            cts.Cancel();
            cts.Dispose();
            base.OnDisable();
        }

        public override void OnGUI()
        {
            base.OnGUI();

            GUILayout.Label("Git Commit Viewer", EditorStyles.boldLabel);

            if (branches.Count == 0)
            {
                GUILayout.Label("Loading branches...");
                return;
            }

            GUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Branch:", GUILayout.Width(50));
            int newBranchIndex = EditorGUILayout.Popup(selectedBranchIndex, branches.ToArray(), GUILayout.Width(200));
            if (newBranchIndex != selectedBranchIndex)
            {
                selectedBranchIndex = newBranchIndex;
                UpdateCommitList();
            }
            EditorGUILayout.LabelField("Commits to show:", GUILayout.Width(100));
            int newCommitCount = EditorGUILayout.IntField(commitCount, GUILayout.Width(50));
            if (newCommitCount != commitCount && newCommitCount > 0)
            {
                commitCount = newCommitCount;
                UpdateCommitList();
            }
            if (GUILayout.Button("Refresh", GUILayout.Width(80)))
            {
                UpdateBranches();
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);
            DrawCommitTable();
        }

        private async void UpdateBranches()
        {
            cts.Cancel();
            cts = new CancellationTokenSource();

            branches.Clear();
            var (output, _) = await GitAsyncHelper.RunGitWithOutputAsync("branch --format=\"%(refname:short)\"", cts.Token);
            string[] lines = output.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < lines.Length; ++i)
            {
                string branch = lines[i].Trim().Trim('\"');
                if (!string.IsNullOrEmpty(branch))
                    branches.Add(branch);
            }
            selectedBranchIndex = Mathf.Clamp(selectedBranchIndex, 0, branches.Count - 1);
            UpdateCommitList();
            Repaint();
        }

        private async void UpdateCommitList()
        {
            cts.Cancel();
            cts = new CancellationTokenSource();
            commitList.Clear();
            if (branches.Count == 0) return;
            string branchName = branches[selectedBranchIndex];
            var (output, _) = await GitAsyncHelper.RunGitWithOutputAsync($"log {branchName} -n {commitCount} --pretty=format:%H|%an|%ad|%s --date=short", cts.Token);

            string[] lines = output.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var parts = line.Split('|');
                if (parts.Length >= 4)
                {
                    commitList.Add(new CommitData
                    {
                        CommitId = parts[0],
                        Author = parts[1],
                        Date = parts[2],
                        Message = parts[3],
                        Expanded = false,
                        Changes = new List<GitAsyncHelper.GitChange>(),
                        LoadedChanges = false,
                    });
                }
            }
            Repaint();
        }

        private void DrawCommitTable()
        {
            GUILayout.Label("Commits:", EditorStyles.boldLabel);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            foreach (var commit in commitList)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.BeginHorizontal();

                commit.Expanded = EditorGUILayout.Foldout(commit.Expanded, $"{commit.Date} | {commit.Author} | {commit.Message}");
                EditorGUILayout.EndHorizontal();

                if (commit.Expanded)
                {
                    if (!commit.LoadedChanges)
                    {
                        if (GUILayout.Button("Load Changes"))
                        {
                            LoadCommitChanges(commit);
                        }
                    }
                    else
                    {
                        var csFiles = commit.Changes.Where(ch => IsCsFile(ch.Path)).ToList();
                        if (csFiles.Count == 0)
                        {
                            EditorGUILayout.LabelField("No .cs files changed in this commit.");
                        }
                        else
                        {
                            foreach (var change in csFiles)
                            {
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField($"{change.Status}", GUILayout.Width(25));
                                EditorGUILayout.LabelField($"{change.Path}");

                                if (GUILayout.Button("AIValidate", GUILayout.Width(80)))
                                {
                                    // Placeholder: AIValidate does nothing for now
                                }
                                EditorGUILayout.EndHorizontal();
                            }
                        }
                    }
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();
        }

        private async void LoadCommitChanges(CommitData commit)
        {
            var changes = await GitAsyncHelper.GetCommitChangesAsync(commit.CommitId, ct: cts.Token);
            commit.Changes = changes.Where(ch => IsCsFile(ch.Path)).ToList();
            commit.LoadedChanges = true;
            Repaint();
        }

        private bool IsCsFile(string path)
        {
            return path != null && path.EndsWith(".cs", System.StringComparison.OrdinalIgnoreCase);
        }
    }
}