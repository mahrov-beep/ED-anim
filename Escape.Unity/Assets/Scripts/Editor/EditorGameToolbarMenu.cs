namespace Scripts.Editor {
    using Game.Services.Photon;
    using Multicast.Build.PreBuildSteps;
    using Multicast.DirtyDataEditor.GoogleSheets;
    using Multicast.FeatureToggles;
    using Multicast.Localization;
    using Multicast.Modules.UserData;
    using Multicast.UnityToolbarExtender;
    using Quantum.Editor;
    using Sirenix.OdinInspector;
    using Sirenix.Utilities.Editor;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;

    public class EditorGameToolbarMenu {
        private static readonly GUIContent DdeContent                 = new("DDE", "Load DDE from Google Sheets");
        private static readonly GUIContent LocalizationContent        = new("Localization", "Load localization from Google Sheet");
        private static readonly GUIContent BootContent                = new("Boot", "Start game with UserData and Features selectors");
        private static readonly GUIContent PhotonDevAppVersion        = new("Photon", "Override photon AppVersion for current machine");
        private static readonly GUIContent UseLocalServerContent      = new("Localhost", "Use local meta server instance");
        private static readonly GUIContent DockerStartContent         = new("▶ Start", "Start all containers (DB + Server)");
        private static readonly GUIContent DockerStopContent          = new("⏸", "Stop containers (except logs viewer)");
        private static readonly GUIContent DockerRebuildServerContent = new("🔨 Server", "Rebuild and restart server only");
        private static readonly GUIContent DockerRebuildAllContent    = new("🔨 All", "Full reset (DB cleanup + rebuild all)");
        private static readonly GUIContent DockerLogsContent          = new("📋 Logs", "Open containers log viewer");
        private static readonly GUIContent SkipMenuContent            = new("Skip Menu", "Skip main menu and go directly to game");

        private static DockerHelper.ServerInfo serverInfo;
        private static double                  lastStatusCheck;
        private static GUIStyle                statusLabelStyle;
        private static GUIStyle                versionLabelStyle;

        private static string cachedDevAppVersion;
        private static string skipMenuEditorPrefsKey;

        private const double DOCKER_STATUS_CHECK_INTERVAL = 10.0;

        [InitializeOnLoadMethod]
        private static void Init() {
            ToolbarExtender.LeftToolbarGUI.Add(DrawLeftGUI);
            ToolbarExtender.RightToolbarGUI.Add(DrawRightGUI);
            cachedDevAppVersion = PhotonService.DevAppVersionPref;
            skipMenuEditorPrefsKey = $"GameProperty.{Game.UI.GameProperties.Booleans.SkipMainMenu.Name}";
        }

        private static void DrawLeftGUI() {
            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            {
                GUILayout.Space(60);

                if (GUILayout.Button(DdeContent, EditorStyles.toolbarButton, GUILayout.Width(40))) {
                    DirtyDataFastImportMenu.ShowLoadSheetDialog(GUILayoutUtility.GetLastRect());
                }

                GUILayout.Space(5);

                if (GUILayout.Button(LocalizationContent, EditorStyles.toolbarButton, GUILayout.Width(90))) {
                    LocalizationFastImportMenu.ShowLoadSheetDialog(GUILayoutUtility.GetLastRect());
                }

                GUILayout.Space(20);

                var magicFixRect = GUILayoutUtility.GetRect(25, 18);
                if (SirenixEditorGUI.SDFIconButton(magicFixRect, "Fix TMP Icons\nReimport Addressables", SdfIconType.Magic, style: EditorStyles.toolbarButton)) {
                    AddressablesImporterRun.Execute();
                    ReimportTmpSpriteAssets.ReimportTextMeshProSpriteAtlasSupportPluginAssets();
                    QuantumEditorAutoBaker.BakeAllScenes_MapData();
                    AssetDatabase.Refresh();
                    AssetDatabase.SaveAssets();
                }

                GUILayout.FlexibleSpace();

                if (GUILayout.Button(BootContent, EditorStyles.toolbarButton, GUILayout.Width(40))) {
                    FeatureTogglesEditorMenu.SetShowFeatureSelectorOnce();
                    UserDataMenu.SetShowFeatureSelectorOnce();

                    EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                    EditorSceneManager.OpenScene("Assets/Bootloader.unity");
                    EditorApplication.isPlaying = true;
                }

                GUILayout.Space(5);

                var skipMenuEnabled = EditorPrefs.GetBool(skipMenuEditorPrefsKey, false);
                GUIHelper.PushColor(skipMenuEnabled ? new Color(1f, 0.8f, 0f) : new Color(1, 1, 1, 0.5f));
                GUIHelper.PushIsBoldLabel(skipMenuEnabled);
                var newSkipMenu = GUILayout.Toggle(skipMenuEnabled, SkipMenuContent, GUILayout.Width(75));
                if (newSkipMenu != skipMenuEnabled) {
                    EditorPrefs.SetBool(skipMenuEditorPrefsKey, newSkipMenu);
                }
                GUIHelper.PopIsBoldLabel();
                GUIHelper.PopColor();

                GUILayout.Space(5);
            }

            EditorGUI.EndDisabledGroup();
        }

        private static void DrawRightGUI() {
            DrawServer();
            GUILayout.Space(10);
            DrawPhoton();
            GUILayout.Space(20);
        }

        private static void DrawPhoton() {
            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            GUIHelper.PushColor(!string.IsNullOrWhiteSpace(cachedDevAppVersion) ? new Color(1, 1, 1, 0.8f) : new Color(1, 1, 1, 0.5f));
            GUIHelper.PushLabelWidth(45);
            var newValue = EditorGUILayout.TextField(
                PhotonDevAppVersion,
                cachedDevAppVersion,
                EditorStyles.miniTextField,
                GUILayout.Width(120)
            );
            if (newValue != cachedDevAppVersion) {
                cachedDevAppVersion = newValue;
                PhotonService.DevAppVersionPref = newValue;
            }
            GUIHelper.PopLabelWidth();
            GUIHelper.PopColor();

            GUILayout.EndHorizontal();
        }

        private static void DrawServer() {
            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            GUIHelper.PushColor(Multicast.Server.ServerMenu.UseLocalServer ? new Color(1f, 0.8f, 0f) : new Color(1, 1, 1, 0.5f));

            GUIHelper.PushIsBoldLabel(Multicast.Server.ServerMenu.UseLocalServer);
            Multicast.Server.ServerMenu.UseLocalServer = GUILayout.Toggle(Multicast.Server.ServerMenu.UseLocalServer, UseLocalServerContent, GUILayout.Width(80));
            GUIHelper.PopIsBoldLabel();

            if (ServerMenuSettings.EnableDockerPanel) {
                if (EditorApplication.timeSinceStartup - lastStatusCheck > DOCKER_STATUS_CHECK_INTERVAL) {
                    serverInfo      = DockerHelper.GetServerInfo();
                    lastStatusCheck = EditorApplication.timeSinceStartup;
                }

                serverInfo ??= DockerHelper.GetServerInfo();

                DrawStatusIndicator(serverInfo);

                var isOperationInProgress = !string.IsNullOrEmpty(serverInfo.CurrentOperation);

                var isRunning =
                    serverInfo.Status == DockerHelper.ServerStatus.Healthy ||
                    serverInfo.Status == DockerHelper.ServerStatus.Running ||
                    serverInfo.Status == DockerHelper.ServerStatus.Unhealthy ||
                    serverInfo.Status == DockerHelper.ServerStatus.Starting;

                var isStopped =
                    serverInfo.Status == DockerHelper.ServerStatus.Stopped ||
                    serverInfo.Status == DockerHelper.ServerStatus.Unknown;

                GUILayout.BeginHorizontal();

                EditorGUI.BeginDisabledGroup(!isStopped || isOperationInProgress);
                if (GUILayout.Button(DockerStartContent, EditorStyles.miniButtonLeft, GUILayout.Width(55))) {

                    DockerHelper.ComposeUp();

                    EditorApplication.delayCall += () => {
                        lastStatusCheck = 0;
                        DockerHelper.RefreshServerStatus();
                    };
                }
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(!isRunning || isOperationInProgress);
                if (GUILayout.Button(DockerStopContent, EditorStyles.miniButtonMid, GUILayout.Width(22))) {

                    DockerHelper.ComposeStop();

                    EditorApplication.delayCall += () => {
                        lastStatusCheck = 0;
                        DockerHelper.RefreshServerStatus();
                    };
                }
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(!isRunning || isOperationInProgress);
                if (GUILayout.Button(DockerRebuildServerContent, EditorStyles.miniButtonMid, GUILayout.Width(66))) {

                    DockerHelper.ComposeRebuildServer();

                    EditorApplication.delayCall += () => {
                        lastStatusCheck = 0;
                        DockerHelper.RefreshServerStatus();
                    };
                }
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(isOperationInProgress);
                if (GUILayout.Button(DockerRebuildAllContent, EditorStyles.miniButtonMid, GUILayout.Width(42))) {

                    DockerHelper.ComposeReload();

                    EditorApplication.delayCall += () => {
                        lastStatusCheck = 0;
                        DockerHelper.RefreshServerStatus();
                    };
                }
                EditorGUI.EndDisabledGroup();

                if (GUILayout.Button(DockerLogsContent, EditorStyles.miniButtonRight, GUILayout.Width(55))) {
                    var isDozzleRunning = DockerHelper.IsDozzleRunning();
                    var localIp = DockerHelper.GetLocalIPAddress();
                    if (isDozzleRunning) {
                        Application.OpenURL($"http://{localIp}:14088");
                    } else {
                        DockerHelper.StartDozzle();
                        EditorApplication.delayCall += () => {
                            System.Threading.Tasks.Task.Delay(1500).ContinueWith(_ => {
                                EditorApplication.delayCall += () => Application.OpenURL($"http://{localIp}:14088");
                            });
                        };
                    }
                }

                GUILayout.EndHorizontal();
            }

            GUIHelper.PopColor();
            GUILayout.EndHorizontal();
        }

        private static void DrawStatusIndicator(DockerHelper.ServerInfo info) {
            string icon;
            Color  color;
            string tooltip;

            if (!string.IsNullOrEmpty(info.CurrentOperation)) {
                icon    = "⟳";
                color   = new Color(1f, 0.7f, 0f);
                tooltip = $"Docker: {info.CurrentOperation}";
            }
            else {
                (icon, color, tooltip) = info.Status switch {
                    DockerHelper.ServerStatus.Healthy => ("●", new Color(0.3f, 1f, 0.3f), "Server: Healthy ✓"),
                    DockerHelper.ServerStatus.Running => ("●", new Color(1f, 0.9f, 0.3f), "Server: Running"),
                    DockerHelper.ServerStatus.Starting => ("◐", new Color(1f, 0.7f, 0f), "Server: Starting..."),
                    DockerHelper.ServerStatus.Unhealthy => ("●", new Color(1f, 0.3f, 0.3f), "Server: Unhealthy ✗"),
                    DockerHelper.ServerStatus.Stopped => ("○", new Color(0.5f, 0.5f, 0.5f), "Server: Stopped"),
                    _ => ("?", new Color(0.7f, 0.7f, 0.7f), "Server: Unknown"),
                };
            }

            var prevColor = GUI.color;
            GUI.color = color;

            statusLabelStyle ??= new GUIStyle(EditorStyles.boldLabel) {
                fontSize  = 14,
                alignment = TextAnchor.MiddleCenter,
            };

            var content = new GUIContent(icon, tooltip);
            GUILayout.Label(content, statusLabelStyle, GUILayout.Width(16), GUILayout.Height(18));
            GUI.color = prevColor;

            if (!string.IsNullOrEmpty(info.Version) && info.Status == DockerHelper.ServerStatus.Healthy) {
                versionLabelStyle ??= new GUIStyle(EditorStyles.miniLabel) {
                    fontSize  = 9,
                    alignment = TextAnchor.MiddleLeft,
                    normal    = { textColor = new Color(0.7f, 0.7f, 0.7f, 0.9f) },
                };

                var versionRect = GUILayoutUtility.GetRect(new GUIContent($"v{info.Version}"), versionLabelStyle, GUILayout.Width(40));
                if (GUI.Button(versionRect, $"v{info.Version}", versionLabelStyle)) {
                    EditorGUIUtility.systemCopyBuffer = info.Version;
                    Debug.Log($"Server version copied to clipboard: {info.Version}");
                }
                EditorGUIUtility.AddCursorRect(versionRect, MouseCursor.Link);
            }
        }
    }
}