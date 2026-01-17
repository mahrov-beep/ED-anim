namespace Game.UI.Controllers {
    using System;
    using Cysharp.Threading.Tasks;
    using Domain.UserData;
    using Multicast;
    using Multicast.Analytics;
    using Multicast.Localization;
    using Quantum;
    using Shared.DTO;
    using Shared.UserProfile.Commands;
    using UnityEngine;

    public class AppBootController : FlowController<AppBootControllerArgs> {
        private const string DEBUG_TIMER_APP_BOOT = "app_boot_flow";

        [Inject] private IAnalyticsRegistration  analyticsRegistration;
        [Inject] private GameData                gameData;
        [Inject] private AppSharedFormulaContext appSharedFormulaContext;

        protected override async UniTask Activate(Context context) {
            this.analyticsRegistration.RegisterGlobalArgument(() => new AnalyticsArg("playtime", this.gameData.UserStats.PlaytimeMinutes.Value));

            this.appSharedFormulaContext.RegisterVariable("paid_user", () => this.gameData.Purchases.Lookup.Count > 0 ? 1 : 0);

            if (Quantum.QConstants.CHARACTER_LOADOUT_SLOTS != CharacterLoadoutSlotsExtension.CHARACTER_LOADOUT_SLOTS) {
                Debug.LogError("CRITICAL: CHARACTER_LOADOUT_SLOTS different in Quantum.Constants and CharacterLoadoutSlotsExtension");
            }

            if (Quantum.QConstants.WEAPON_ATTACHMENT_SLOTS != WeaponAttachmentSlotsExtension.WEAPON_ATTACHMENT_SLOTS) {
                Debug.LogError("CRITICAL: WEAPON_ATTACHMENT_SLOTS different in Quantum.Constants and WeaponAttachmentSlotsExtension");
            }

            Application.runInBackground      = true;
#if UNITY_IOS || UNITY_ANDROID
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
#endif
            LocalizationService.SelectedLang = "EN";

#if UNITY_EDITOR
            App.SetEditorCloneKeyDelegate(() => {
                var dataPath = UnityEngine.Application.dataPath;
                var projectName = System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(dataPath));
                return projectName;
            });
#endif

            App.RetryDelegate = async ex => await context.GetNavigator(AppNavigatorType.System).AlertMetaFailedToConnectToServer($"{ex.GetType().Name} :: {ex.Message}");
            App.BadRequestDelegate = async ex => {
                await context.GetNavigator(AppNavigatorType.System).AlertMetaFailedBadRequest($"{ex.GetType().Name} :: {ex.Message}");
                Application.Quit();
            };

            App.AuthDelegate = async () => {
                var response = await context.Server.AuthGuest(new GuestAuthRequest {
                    DeviceId = GetDeviceId(),
                }, ServerCallRetryStrategy.RetryWithUserDialog);

                return response.AccessToken;

                static string GetDeviceId() {
                    var idPrefsKey = $"sv_id{App.EditorCloneKey}";

                    if (!PlayerPrefs.HasKey(idPrefsKey)) {
                        PlayerPrefs.SetString(idPrefsKey, Guid.NewGuid().ToString());
                    }

                    var id = PlayerPrefs.GetString(idPrefsKey);

#if UNITY_EDITOR
                    if (Application.isEditor) {
                        return $"Editor.{id}.{App.EditorCloneKey}";
                    }
#endif

                    return id;
                    //return SystemInfo.deviceUniqueIdentifier;
                }
            };

            await using (await context.RunProgressScreenDisposable("fetching_user_profile", useSystemNavigator: true)) {
                await context.Server.ExecuteUserProfile(new UserProfileFetchCommand(), ServerCallRetryStrategy.RetryWithUserDialog);
            }
        }
    }
}