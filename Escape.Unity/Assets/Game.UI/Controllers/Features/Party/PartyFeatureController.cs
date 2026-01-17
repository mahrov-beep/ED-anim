namespace Game.UI.Controllers.Features.Party {
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using JetBrains.Annotations;
    using Multicast;
    using Game.Shared.ServerEvents;
    using Game.Shared.DTO;
    using UniMob;
    using Game.Domain.Party;
    using Photon;
    using Services.Photon;
    using Shared.UserProfile.Commands.Game;
    using UnityEngine;

    [Serializable, RequireFieldsInit]
    public struct PartyFeatureControllerArgs : IFlowControllerArgs {
        public IScenesController ScenesController;
    }

    public class PartyFeatureController : FlowController<PartyFeatureControllerArgs> {
        private readonly Queue<(string leaderId, string leaderName)> queue = new();
        private bool isShowing;
        [Inject] private PartyModel partyModel;
        [Inject] private PhotonService photonService;

        protected override async UniTask Activate(Context context) {
            try {
                var status = await context.Server.PartyStatus(new Game.Shared.DTO.PartyStatusRequest { }, ServerCallRetryStrategy.RetryWithUserDialog);
                if (status.LeaderUserId != Guid.Empty && status.Members != null && status.Members.Length > 0) {
                    this.partyModel.Set(status.LeaderUserId, status.Members, status.ReadyMembers);
                    Debug.LogWarning($"PartyStatusRefresh leader={status.LeaderUserId} count={status.Members.Length}");
                }
                else {
                    this.partyModel.Clear();
                    Debug.LogWarning("PartyStatusRefresh empty");
                }
            }
            catch (Exception ex) {
                Debug.LogWarning($"PartyStatusRefresh error: {ex.Message}");
            }

            App.Events.Listen<PartyInviteReceivedAppServerEvent>(this.Lifetime, evt => {
                var leaderId = evt.LeaderUserId.ToString();
                var leaderName = ResolveName(leaderId);
                queue.Enqueue((leaderId, leaderName));
                if (!isShowing) {
                    RequestFlow(ShowNext);
                }
                Debug.LogWarning($"PartyInviteReceived leader={leaderId}");
            });

            App.Events.Listen<PartyUpdatedAppServerEvent>(this.Lifetime, evt => {
                var members = evt.Members ?? Array.Empty<Guid>();
                var ready   = evt.ReadyMembers ?? Array.Empty<Guid>();
                var isLocalMember = members.Contains(App.ServerAccessTokenInfo.UserId);
                if (isLocalMember) {
                    this.partyModel.Set(evt.LeaderUserId, members, ready);
                }
                else {
                    this.partyModel.Clear();
                }
                var count = members.Length;
                var isMember = this.partyModel.IsMember;
                Debug.LogWarning($"PartyUpdated leader={evt.LeaderUserId} count={count} localMember={isMember}");
            });

            App.Events.Listen<PartyDisbandedAppServerEvent>(this.Lifetime, evt => {
                this.partyModel.Clear();
                Debug.LogWarning($"PartyDisbanded leader={evt.LeaderUserId}");
            });

            App.Events.Listen<PartyGameStartedAppServerEvent>(this.Lifetime, evt => {
                Debug.LogWarning($"PartyGameStarted leader={evt.LeaderUserId} gameId={evt.GameId}");
            });

            App.Events.Listen<PartyMatchmakingStartedAppServerEvent>(this.Lifetime, evt => {
                var currentMembers = this.partyModel.Members.Value ?? Array.Empty<Guid>();
                var isLocalMember = currentMembers.Contains(App.ServerAccessTokenInfo.UserId);
                var isLeader = evt.LeaderUserId == App.ServerAccessTokenInfo.UserId;
                
                if (isLocalMember && !isLeader) {
                    Debug.LogWarning($"MM: Party matchmaking started leader={evt.LeaderUserId} mode={evt.GameModeKey} (party member)");
                    RequestFlow(StartWaitForMatch, evt.GameModeKey);
                } else if (isLeader) {
                    Debug.LogWarning($"MM: Party matchmaking started leader={evt.LeaderUserId} mode={evt.GameModeKey} (I'm leader, using own flow)");
                }
            });

            App.Events.Listen<PartyMatchmakingCanceledAppServerEvent>(this.Lifetime, evt => {
                var currentMembers = this.partyModel.Members.Value ?? Array.Empty<Guid>();
                var isLocalMember = currentMembers.Contains(App.ServerAccessTokenInfo.UserId);
                var isLeader = evt.LeaderUserId == App.ServerAccessTokenInfo.UserId;
                
                if (isLocalMember && !isLeader) {
                    Debug.LogWarning($"MM: Party matchmaking canceled by leader={evt.LeaderUserId} canceledBy={evt.CanceledByUserId} (party member, stopping)");
                    this.partyModel.StopMatchmaking();
                } else if (isLeader) {
                    Debug.LogWarning($"MM: Party matchmaking canceled leader={evt.LeaderUserId} canceledBy={evt.CanceledByUserId} (I'm leader)");
                }
            });
        }

        private async UniTask StartWaitForMatch(Context context, string gameModeKey) {
            try {
                Debug.LogWarning($"MM: Party member starting wait for match {gameModeKey}");
                
                if (this.photonService.IsConnected) {
                    Debug.LogWarning("MM: Disconnecting from Photon before matchmaking (party member)");
                    await this.photonService.DisconnectAsync(ConnectFailReason.UserRequest);
                }

                Quantum.QuantumReconnectInformation.Reset();

                // Ensure server knows we're no longer in any game
                await context.Server.ExecuteUserProfile(new Shared.UserProfile.Commands.Game.UserProfileLeaveAllGamesCommand(), 
                    ServerCallRetryStrategy.RetryWithUserDialog);

                await context.Server.ExecuteUserProfile(new Shared.UserProfile.Commands.GameModes.UserProfileSelectGameModeCommand {
                    GameModeKey = gameModeKey,
                }, ServerCallRetryStrategy.RetryWithUserDialog);

                this.partyModel.StartMatchmaking();
                
                Debug.LogWarning($"MM: Waiting for match result from server (party member)");

                MatchmakingStatusResponse status;
                await using (await context.RunSearchGameScreenDisposable(useSystemNavigator: true)) {
                    status = await this.WaitForMatchmaking(context);
                }

                this.partyModel.StopMatchmaking();

                if (status == null || status.Join == null) {
                    Debug.LogWarning("MM: Wait timed out or canceled for party member");
                    return;
                }

                Debug.LogWarning($"MM: Match found! Room={status.Join.RoomName}, Region={status.Join.Region} (party member)");

                await using (await context.RunLoadingScreenDisposable(useSystemNavigator: true)) {
                    await this.Args.ScenesController.GoToEmpty(context);

                    var connectionArgs = new PhotonGameConnectArgs {
                        Session  = status.Join.RoomName,
                        Creating = true,
                        Region   = status.Join.Region,
                    };
                    
                    Debug.LogWarning($"MM: Connecting to Photon (party member)");
                    var result = await context.RunForResult(new PhotonJoinGameControllerArgs {
                        connectionArgs      = connectionArgs,
                        gameModeKeyOverride = status.GameModeKey,
                        maxPlayersOverride  = null,
                    }, default(ConnectResult));
                    
                    if (result.Success) {
                        Debug.LogWarning($"MM: Connected to Photon, joining game (party member)");
                        await context.Server.ExecuteUserProfile(new UserProfileJoinGameCommand {
                            GameId = this.photonService.CurrentGameId,
                        }, ServerCallRetryStrategy.RetryWithUserDialog);
                        
                        await this.Args.ScenesController.GoToGameplay(context);
                        Debug.LogWarning($"MM: Transitioned to gameplay (party member)");
                    } else {
                        Debug.LogWarning("MM: Photon connection failed, returning to main menu (party member)");
                        await this.Args.ScenesController.GoToMainMenu(context);
                    }
                }
            } catch (Exception ex) {
                Debug.LogWarning($"MM: Error during wait: {ex.Message} (party member)");
                this.partyModel.StopMatchmaking();
                await this.Args.ScenesController.GoToMainMenu(context);
            }
        }

        private async UniTask<Game.Shared.DTO.MatchmakingStatusResponse> WaitForMatchmaking(Context context) {
            var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(30);
            Game.Shared.DTO.MatchmakingStatusResponse status = null;

            while (DateTime.UtcNow < deadline) {
                if (!this.partyModel.IsSearchingMatch.Value) {
                    Debug.LogWarning("MM: Search canceled (party member)");
                    return null;
                }

                var remaining = (int)(deadline - DateTime.UtcNow).TotalSeconds;
                this.partyModel.UpdateMatchmakingTime(remaining);

                var s = await context.Server.MatchmakingStatus(new Game.Shared.DTO.MatchmakingStatusRequest { }, ServerCallRetryStrategy.RetryWithUserDialog);
                if (s != null && s.Status == Game.Shared.DTO.EMatchmakingQueueStatus.Matched && s.Join != null) {
                    Debug.LogWarning($"MM: Matched Room={s.Join.RoomName} Region={s.Join.Region} Mode={s.GameModeKey}");
                    status = s;
                    break;
                }
                await UniTask.Delay(TimeSpan.FromMilliseconds(250));
            }

            return status;
        }

        private async UniTask ShowNext(Context context) {
            if (isShowing) return;
            if (queue.Count == 0) return;
            
            isShowing = true;

            var (leaderId, leaderName) = queue.Dequeue();

            if (string.IsNullOrEmpty(leaderName)) {
                if (Guid.TryParse(leaderId, out var id)) {
                    try {
                        var friends = await context.Server.Friends(new FriendsListRequest { }, ServerCallRetryStrategy.RetryWithUserDialog);
                        var arr = friends.Friends ?? Array.Empty<FriendInfoDto>();
                        foreach (var f in arr) {
                            if (f.Id == id) {
                                leaderName = f.NickName;
                                break;
                            }
                        }
                    } catch { }
                }
            }
            
            var accepted = await context.RunForResult(new PartyInviteControllerArgs {
                leaderUserId = leaderId,
                leaderName   = leaderName,
            }, false);

            isShowing = false;
            if (queue.Count > 0) {
                await ShowNext(context);
            }
        }

        [CanBeNull]
        private string ResolveName(string leaderId) {
            return null;
        }
    }
}

