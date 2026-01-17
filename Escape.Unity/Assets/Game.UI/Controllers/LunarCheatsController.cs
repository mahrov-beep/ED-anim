namespace Game.UI.Controllers {
    using System;
    using System.Collections.Generic;
    using Game.Domain.GameProperties;
    using Cysharp.Threading.Tasks;
    using global::Photon.Deterministic;
    using Multicast;
    using Multicast.Cheats;
    using Multicast.Numerics;
    using Quantum.Commands;
    using Quantum;
    using Game.Services.Cheats;
    using Services.Photon;
    using Shared;
    using Shared.DTO;
    using Shared.UserProfile.Commands;
    using Shared.UserProfile.Commands.Currencies;
    using Shared.UserProfile.Commands.Rewards;
#if DEBUG
    using Shared.UserProfile.Commands.Quests;
#endif
    using Tayx.Graphy;
    using Tutorial;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using Object = UnityEngine.Object;

    [Serializable, RequireFieldsInit]
    public struct LunarCheatsControllerArgs : IFlowControllerArgs {
    }

    public class LunarCheatsController : FlowController<LunarCheatsControllerArgs> {
        private bool botsEnabled = true;
#if DEBUG
        private string questTaskKeyToToggle = "";
#endif
        [Inject] private ICheatButtonsRegistry        buttons;
        [Inject] private ICheatGamePropertiesRegistry properties;
        [Inject] private IDebugFireSettingsService    debugFireSettings;

        protected override async UniTask Activate(Context context) {
            this.properties.Register("Photon Dev App",
                () => PhotonService.DevAppVersionPref,
                v => PhotonService.DevAppVersionPref = v
            );

            this.properties.Register(GameProperties.Booleans.DisableReconnect);
            this.properties.Register(GameProperties.Booleans.ShowDevScenes);
            this.properties.Register(GameProperties.Booleans.ForceUseMobileControls);
            this.properties.Register(GameProperties.Booleans.IgnoreTraderShopBlockers);
            this.properties.Register(TutorialProperties.Booleans.DebugSkipTutorials);
            this.properties.Register(ReconGameProperties.Booleans.AlwaysShowEnemyOutline);
            this.properties.Register(DebugGameProperties.Booleans.DebugThirdPersonSpectatorMode);
            this.properties.Register(DebugGameProperties.Booleans.EnableAimAssist);
            this.properties.Register(DebugGameProperties.Booleans.DebugAimAssist);


            this.properties.Register("Cheats.DebugFire.Enabled",
                () => this.debugFireSettings.DebugFireEnabled,
                v => this.debugFireSettings.DebugFireEnabled = v
            );

            this.properties.Register("Cheats.DebugFire.Key",
                () => this.debugFireSettings.DebugFireKey.ToString(),
                v => {
                    if (Enum.TryParse<Key>(v, true, out var key)) {
                        this.debugFireSettings.DebugFireKey = key;
                    }
                }
            );
#if DEBUG
            this.properties.Register("Quests.TaskKey",
                () => this.questTaskKeyToToggle,
                v => this.questTaskKeyToToggle = v
            );
#endif


            this.properties.Register("Bots Enabled",
                () => botsEnabled,
                v => {
                    botsEnabled = v;
                    var game = QuantumRunner.DefaultGame;
                    if (game != null) {
                        var result = game.SendCommand(new SetSystemEnabledCommand<BotBehaviourTreeUpdateSystem> { Enabled = v });
                        if (result != DeterministicCommandSendResult.Success) {
                            Debug.LogError(result.ToString());
                        }
                    }
                }
            );

            this.buttons.RegisterAction("Toggle FPS Counter", () => {
                var manager = Object.FindFirstObjectByType<GraphyManager>(FindObjectsInactive.Include);
                manager.ToggleActive();
            });

            this.buttons.RegisterAction("Toggle Texture Memory Stats", () => {
                var manager = Object.FindFirstObjectByType<TextureMemoryStats>(FindObjectsInactive.Include).gameObject;
                manager.SetActive(!manager.activeSelf);
            });

            this.buttons.RegisterAction("Toggle Native Memory Snapshot", () => {
                var manager = Object.FindFirstObjectByType<NativeMemorySnapshot>(FindObjectsInactive.Include).gameObject;
                manager.SetActive(!manager.activeSelf);
            });

            this.buttons.RegisterAction("Toggle Quantum Stats", () => {
                var manager = Object.FindFirstObjectByType<QuantumStats>(FindObjectsInactive.Include);
                manager.gameObject.SetActive(!manager.gameObject.activeSelf);
            });

            this.buttons.RegisterAction("Quests - Report Enemy Kill", () => this.RequestFlow(async flowContext => {
                await flowContext.Server.GameReportQuestCounterTask(new ReportGameQuestCounterTaskRequest {
                    Property      = QuestCounterPropertyTypes.EnemyKilled,
                    CounterValue  = 1,
                    Filters       = Array.Empty<QuestTaskFilters>(),
                    GameId        = App.Get<PhotonService>().CurrentGameId,
                    TargetUserIds = new[] { App.ServerAccessTokenInfo.UserId },
                }, ServerCallRetryStrategy.Throw);
            }));

            this.buttons.RegisterAction("Quests - Report Died", () => this.RequestFlow(async flowContext => {
                await flowContext.Server.GameReportQuestCounterTask(new ReportGameQuestCounterTaskRequest {
                    Property      = QuestCounterPropertyTypes.Died,
                    CounterValue  = 1,
                    Filters       = Array.Empty<QuestTaskFilters>(),
                    GameId        = App.Get<PhotonService>().CurrentGameId,
                    TargetUserIds = new[] { App.ServerAccessTokenInfo.UserId },
                }, ServerCallRetryStrategy.Throw);
            }));
#if DEBUG
            this.buttons.RegisterAction("Quests - Toggle Task Completion", () => this.RequestFlow(async flowContext => {
                if (string.IsNullOrWhiteSpace(this.questTaskKeyToToggle)) {
                    Debug.LogError("Quest task key is empty");
                    return;
                }

                await flowContext.Server.ExecuteUserProfile(new UserProfileDebugCheatToggleQuestTaskCommand {
                    QuestTaskKey = this.questTaskKeyToToggle,
                }, ServerCallRetryStrategy.Throw);
            }));
#endif

            this.buttons.RegisterAction("Add 1000 Badges", () => this.RequestFlow(async flowContext => {
                await flowContext.Server.ExecuteUserProfile(new UserProfileDebugCheatAddCurrencyCommand {
                    CurrencyToAdd = new Dictionary<string, int> { [SharedConstants.Game.Currencies.BADGES] = 1000 },
                }, ServerCallRetryStrategy.Throw);
            }));

            this.buttons.RegisterAction("Add 100 Bucks", () => this.RequestFlow(async flowContext => {
                await flowContext.Server.ExecuteUserProfile(new UserProfileDebugCheatAddCurrencyCommand {
                    CurrencyToAdd = new Dictionary<string, int> { [SharedConstants.Game.Currencies.BUCKS] = 100 },
                }, ServerCallRetryStrategy.Throw);
            }));

            this.buttons.RegisterAction("Add 100 Crypt", () => this.RequestFlow(async flowContext => {
                await flowContext.Server.ExecuteUserProfile(new UserProfileDebugCheatAddCurrencyCommand {
                    CurrencyToAdd = new Dictionary<string, int> { [SharedConstants.Game.Currencies.CRYPT] = 100 },
                }, ServerCallRetryStrategy.Throw);
            }));

            this.buttons.RegisterAction("Add 10 Loadout Tickets", () => this.RequestFlow(async flowContext => {
                await flowContext.Server.ExecuteUserProfile(new UserProfileDebugCheatAddCurrencyCommand {
                    CurrencyToAdd = new Dictionary<string, int> { [SharedConstants.Game.Currencies.LOADOUT_TICKETS] = 10 },
                }, ServerCallRetryStrategy.Throw);
            }));

            this.buttons.RegisterAction("Add 100 EXP", () => this.RequestFlow(async flowContext => {
                await flowContext.Server.ExecuteUserProfile(new UserProfileQueueDropRewardCommand {
                    RewardGuid = Guid.NewGuid().ToString(),
                    Reward     = Reward.Int(SharedConstants.RewardTypes.EXP, SharedConstants.Game.Exp.MATCH_PLAYED, 100),
                }, ServerCallRetryStrategy.Throw);
            }));

            this.buttons.RegisterAction("Set target FPS - 60", () => Application.targetFrameRate = 60);
            this.buttons.RegisterAction("Set target FPS - 30", () => Application.targetFrameRate = 30);
            this.buttons.RegisterAction("Render 1.2", () => ScalableBufferManager.ResizeBuffers(1.2f, 1.2f));
            this.buttons.RegisterAction("Render 1", () => ScalableBufferManager.ResizeBuffers(1f, 1f));
            this.buttons.RegisterAction("Render 0.8", () => ScalableBufferManager.ResizeBuffers(0.8f, 0.8f));
            this.buttons.RegisterAction("Render 0.6", () => ScalableBufferManager.ResizeBuffers(0.6f, 0.6f));
            this.buttons.RegisterAction("Render 0.4", () => ScalableBufferManager.ResizeBuffers(0.4f, 0.4f));
            
            this.buttons.RegisterAction("Texture Memory Budget 50 MB", () => SetMipmapStreamingMemoryBudget(50));
            this.buttons.RegisterAction("Texture Memory Budget 100 MB", () => SetMipmapStreamingMemoryBudget(100));
            this.buttons.RegisterAction("Texture Memory Budget 150 MB", () => SetMipmapStreamingMemoryBudget(150));
            this.buttons.RegisterAction("Texture Memory Budget 200 MB", () => SetMipmapStreamingMemoryBudget(200));
            this.buttons.RegisterAction("Texture Memory Budget 350 MB", () => SetMipmapStreamingMemoryBudget(350));
            this.buttons.RegisterAction("Texture Memory Budget 500 MB", () => SetMipmapStreamingMemoryBudget(500));
            
            this.buttons.RegisterAction("Texture Mipmap Limit - Full", () => QualitySettings.globalTextureMipmapLimit = 0);
            this.buttons.RegisterAction("Texture Mipmap Limit - Half 1/2", () => QualitySettings.globalTextureMipmapLimit = 1);
            this.buttons.RegisterAction("Texture Mipmap Limit - Quarter 1/4", () => QualitySettings.globalTextureMipmapLimit = 2);
            this.buttons.RegisterAction("Texture Mipmap Limit - Eighth 1/8", () => QualitySettings.globalTextureMipmapLimit = 3);

            this.buttons.RegisterAction("Add 1000 Badges Bucks Rating", () => this.RequestFlow(async flowContext => {
                await flowContext.Server.ExecuteUserProfile(new UserProfileDebugCheatAddCurrencyCommand {
                    CurrencyToAdd = new Dictionary<string, int> {
                        [SharedConstants.Game.Currencies.BADGES] = 1000,
                        [SharedConstants.Game.Currencies.BUCKS]  = 1000,
                        [SharedConstants.Game.Currencies.RATING] = 1000,
                    },
                }, ServerCallRetryStrategy.Throw);
            }));

            this.buttons.RegisterAction("Delete profile from server", () => this.RequestFlow(async flowContext => {
                await flowContext.Server.UserDelete(new UserDeleteRequest(), ServerCallRetryStrategy.Throw);
                await flowContext.Server.ExecuteUserProfile(new UserProfileFetchCommand(), ServerCallRetryStrategy.Throw);
            }));
        }

        private static void SetMipmapStreamingMemoryBudget(int megaBytes) {
            var isValueInMegaBytes = QualitySettings.streamingMipmapsMemoryBudget < 1024 * 10; // If greater than 10 GB - probably it is bytes
            if (!isValueInMegaBytes) {
                Debug.LogError("QualitySettings.streamingMipmapsMemoryBudget value is not in megabytes!");
                return;
            }

            // The total amount of memory (in megabytes, or in bytes, depending on your platform)...
            QualitySettings.streamingMipmapsMemoryBudget = megaBytes;
        }
    }
}