namespace Multicast.Boot {
    using System;
    using System.Diagnostics;
    using Analytics;
    using Cysharp.Threading.Tasks;
    using Sirenix.OdinInspector;
    using UniMob.UI.Widgets;
    using Unity;
    using UnityEngine;
    using Debug = UnityEngine.Debug;

    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(DontDestroyOnLoad))]
    public sealed class BootLoader : MonoBehaviour {
        [SerializeField, Required] private ViewPanel rootViewPanel;
        [SerializeField, Required] private ViewPanel systemViewPanel;
        [SerializeField, Required] private Transform preloaderRoot;

        [SerializeField, Required] private string preloaderResource = "PRELOADER";

        [SerializeField, Required]
        [ListDrawerSettings(ShowPaging = false, ShowFoldout = false)]
        private string[] gameAssemblies = Array.Empty<string>();

        internal static string EditorPlatformOverride {
#if UNITY_EDITOR
            get => UnityEditor.EditorPrefs.GetString("BootLoader.EditorPlatformOverride");
            set => UnityEditor.EditorPrefs.SetString("BootLoader.EditorPlatformOverride", value);
#else
            get => null;
#endif
        }

        private void Start() {
            this.Boot().Forget();
        }

        private void Update() {
            if (ControllersShared.RootController != null) {
                ControllersShared.RootController.UpdateHierarchical();
            }
        }

        private void OnDestroy() {
            if (ControllersShared.RootController != null) {
                ControllersShared.StopRootController();
            }
        }

        private async UniTask Boot() {
            try {
                DontDestroyOnLoad(this);

                var bootTimer = Stopwatch.StartNew();

                var preloaderPrefab = Resources.Load<GameObject>(this.preloaderResource);

                if (preloaderPrefab == null) {
                    throw new Exception("Failed to load preloader prefab resource");
                }

                var preloaderObj = Instantiate(preloaderPrefab, this.preloaderRoot, false);
                var preloaderUi  = preloaderObj.GetComponent<PreloaderUI>();

                if (preloaderUi == null) {
                    throw new Exception("Preloader prefab resource does not contains PreloaderUI script");
                }

                await ControllersShared.RunRootController(new BootloaderControllerArgs {
                    EditorPlatformOverride = EditorPlatformOverride,
                    GameAssemblies         = this.gameAssemblies,

                    PreloaderRoot   = this.preloaderRoot,
                    SystemViewPanel = this.systemViewPanel,
                    RootViewPanel   = this.rootViewPanel,

                    PreloaderUI = preloaderUi,
                });

                GameObject.Destroy(preloaderUi.gameObject);

                this.AppUpdateFlowLoop().Forget();

                App.Analytics.Send("boot_time",
                    new AnalyticsArg("seconds", (int)bootTimer.Elapsed.TotalSeconds),
                    new AnalyticsArg("installer", Application.installerName)
                );
            }
            catch (Exception ex) {
                Debug.LogException(ex);
                Debug.LogError("APP LAUNCH FAILED");
            }
        }

        private async UniTaskVoid AppUpdateFlowLoop() {
            var lastTransactionVersion = -1;

            var userDataService = App.UserDataService;

            while (!this.destroyCancellationToken.IsCancellationRequested) {
                if (lastTransactionVersion != userDataService.Root.TransactionVersion) {
                    lastTransactionVersion = userDataService.Root.TransactionVersion;

                    App.RequestAppUpdateFlow();
                }

                if (App.Current.AppUpdateRequested) {
                    App.Current.AppUpdateRequested = false;

                    if (ControllersShared.RootController != null) {
                        await ControllersShared.RootController.ExecuteFlowHierarchical();
                    }
                }

                await UniTask.NextFrame();
            }
        }
    }
}