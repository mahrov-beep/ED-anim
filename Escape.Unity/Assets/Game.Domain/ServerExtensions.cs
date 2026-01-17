using System;
using Cysharp.Threading.Tasks;
using Game.Shared;
using Game.Shared.DTO;
using Game.Shared.ServerEvents;
using Game.Shared.UserProfile;
using Game.Shared.UserProfile.Data;
using JetBrains.Annotations;
using Multicast;
using UniMob;
using UnityEngine;

public static class ServerExtensions {
    [PublicAPI]
    public static UniTask ExecuteUserProfile<TCommand>(this ServerRequests requests, TCommand command, ServerCallRetryStrategy retryStrategy)
        where TCommand : class, IUserProfileServerCommand, IServerCommandExecutableFromClient {
        return requests.Execute<UserProfileServerCommandContext, SdUserProfile, IUserProfileServerCommand>(SharedConstants.UrlRoutes.USER_PROFILE, command, retryStrategy);
    }

    [PublicAPI]
    public static UniTask<GuestAuthResponse> AuthGuest(this ServerRequests requests, GuestAuthRequest request, ServerCallRetryStrategy retryStrategy) {
        return requests.Request<GuestAuthRequest, GuestAuthResponse>(SharedConstants.UrlRoutes.Auth.GUEST, request, retryStrategy);
    }

    [PublicAPI]
    public static UniTask<UserDeleteResponse> UserDelete(this ServerRequests requests, UserDeleteRequest request, ServerCallRetryStrategy retryStrategy) {
        return requests.Request<UserDeleteRequest, UserDeleteResponse>(SharedConstants.UrlRoutes.User.DELETE, request, retryStrategy);
    }

    [PublicAPI]
    public static UniTask<UserGetInfoResponse> UserGetInfo(this ServerRequests requests, UserGetInfoRequest request, ServerCallRetryStrategy retryStrategy) {
        return requests.Request<UserGetInfoRequest, UserGetInfoResponse>(SharedConstants.UrlRoutes.User.GET_INFO, request, retryStrategy);
    }

    [PublicAPI]
    public static UniTask<ChangeNickNameResponse> UserChangeNickName(this ServerRequests requests, ChangeNickNameRequest request, ServerCallRetryStrategy retryStrategy) {
        return requests.Request<ChangeNickNameRequest, ChangeNickNameResponse>(SharedConstants.UrlRoutes.CHANGE_NICKNAME, request, retryStrategy);
    }

    [PublicAPI]
    public static UniTask<ReportGameSnapshotResponse> GameReportSnapshot(this ServerRequests requests, ReportGameSnapshotRequest request, ServerCallRetryStrategy retryStrategy) {
        return requests.Request<ReportGameSnapshotRequest, ReportGameSnapshotResponse>(SharedConstants.UrlRoutes.Game.REPORT_GAME_SNAPSHOT, request, retryStrategy);
    }

    [PublicAPI]
    public static UniTask<ReportGameQuestCounterTaskResponse> GameReportQuestCounterTask(this ServerRequests requests,
        ReportGameQuestCounterTaskRequest request, ServerCallRetryStrategy retryStrategy) {
        return requests.Request<ReportGameQuestCounterTaskRequest, ReportGameQuestCounterTaskResponse>(
            SharedConstants.UrlRoutes.Game.REPORT_QUEST_COUNTER_TASK, request, retryStrategy);
    }

    [PublicAPI]
    public static void ConnectToAppEvents(this ServerRequests requests, Lifetime lifetime, Action<bool> onConnectionLost) {
        requests.ConnectToEvents<IAppServerEvent>(lifetime, SharedConstants.UrlRoutes.ServerEvents.APP, onConnectionLost);
    }

    [PublicAPI]
    public static void ConnectToGameEvents(this ServerRequests requests, Lifetime lifetime, string gameId, Action<bool> onConnectionLost) {
        requests.ConnectToEvents<IGameServerEvent>(lifetime, SharedConstants.UrlRoutes.ServerEvents.GAME + "?gameId=" + gameId, onConnectionLost);
    }

    [PublicAPI]
    public static UniTask<FriendsListResponse> Friends(this ServerRequests requests, FriendsListRequest request, ServerCallRetryStrategy retryStrategy) {
        return requests.Request<FriendsListRequest, FriendsListResponse>(SharedConstants.UrlRoutes.Friends.FRIEND_LIST, request, retryStrategy);
    }

    [PublicAPI]
    public static UniTask<IncomingRequestsResponse> FriendsIncoming(this ServerRequests requests, IncomingRequestsRequest request, ServerCallRetryStrategy retryStrategy) {
        return requests.Request<IncomingRequestsRequest, IncomingRequestsResponse>(SharedConstants.UrlRoutes.Friends.INCOMING_REQUESTS, request, retryStrategy);
    }

    [PublicAPI]
    public static UniTask<FriendsListResponse> FriendsOnline(this ServerRequests requests, FriendsListRequest request, ServerCallRetryStrategy retryStrategy) {
        return requests.Request<FriendsListRequest, FriendsListResponse>(SharedConstants.UrlRoutes.Friends.ONLINE, request, retryStrategy);
    }

    [PublicAPI]
    public static UniTask<FriendAddByNicknameResponse> FriendsAddByNickname(this ServerRequests requests, FriendAddByNicknameRequest request, ServerCallRetryStrategy retryStrategy) {
        return requests.Request<FriendAddByNicknameRequest, FriendAddByNicknameResponse>(SharedConstants.UrlRoutes.Friends.ADD_BY_NICKNAME, request, retryStrategy);
    }

    [PublicAPI]
    public static UniTask<FriendAcceptResponse> FriendsAccept(this ServerRequests requests, FriendAcceptRequest request, ServerCallRetryStrategy retryStrategy) {
        return requests.Request<FriendAcceptRequest, FriendAcceptResponse>(SharedConstants.UrlRoutes.Friends.ACCEPT, request, retryStrategy);
    }

    [PublicAPI]
    public static UniTask<FriendDeclineResponse> FriendsDecline(this ServerRequests requests, FriendDeclineRequest request, ServerCallRetryStrategy retryStrategy) {
        return requests.Request<FriendDeclineRequest, FriendDeclineResponse>(SharedConstants.UrlRoutes.Friends.DECLINE, request, retryStrategy);
    }

    [PublicAPI]
    public static UniTask<FriendRemoveResponse> FriendsRemove(this ServerRequests requests, FriendRemoveRequest request, ServerCallRetryStrategy retryStrategy) {
        return requests.Request<FriendRemoveRequest, FriendRemoveResponse>(SharedConstants.UrlRoutes.Friends.REMOVE, request, retryStrategy);
    }

    [PublicAPI]
    public static UniTask<FriendsIncomingBulkResponse> FriendsIncomingBulk(this ServerRequests requests, FriendsIncomingBulkRequest request, ServerCallRetryStrategy retryStrategy) {
        return requests.Request<FriendsIncomingBulkRequest, FriendsIncomingBulkResponse>(SharedConstants.UrlRoutes.Friends.INCOMING_BULK, request, retryStrategy);
    }

    [PublicAPI]
    public static UniTask<PartyInviteResponse> PartyInvite(this ServerRequests requests, PartyInviteRequest request, ServerCallRetryStrategy retryStrategy) {
        return requests.Request<PartyInviteRequest, PartyInviteResponse>(SharedConstants.UrlRoutes.Party.INVITE, request, retryStrategy);
    }

    [PublicAPI]
    public static UniTask<PartyAcceptInviteResponse> PartyAccept(this ServerRequests requests, PartyAcceptInviteRequest request, ServerCallRetryStrategy retryStrategy) {
        return requests.Request<PartyAcceptInviteRequest, PartyAcceptInviteResponse>(SharedConstants.UrlRoutes.Party.ACCEPT, request, retryStrategy);
    }

    [PublicAPI]
    public static UniTask<PartyDeclineInviteResponse> PartyDecline(this ServerRequests requests, PartyDeclineInviteRequest request, ServerCallRetryStrategy retryStrategy) {
        return requests.Request<PartyDeclineInviteRequest, PartyDeclineInviteResponse>(SharedConstants.UrlRoutes.Party.DECLINE, request, retryStrategy);
    }

    [PublicAPI]
    public static UniTask<PartyLeaveResponse> PartyLeave(this ServerRequests requests, PartyLeaveRequest request, ServerCallRetryStrategy retryStrategy) {
        return requests.Request<PartyLeaveRequest, PartyLeaveResponse>(SharedConstants.UrlRoutes.Party.LEAVE, request, retryStrategy);
    }

    [PublicAPI]
    public static UniTask<PartyStartGameResponse> PartyStart(this ServerRequests requests, PartyStartGameRequest request, ServerCallRetryStrategy retryStrategy) {
        return requests.Request<PartyStartGameRequest, PartyStartGameResponse>(SharedConstants.UrlRoutes.Party.START, request, retryStrategy);
    }

    [PublicAPI]
    public static UniTask<PartyStatusResponse> PartyStatus(this ServerRequests requests, PartyStatusRequest request, ServerCallRetryStrategy retryStrategy) {
        return requests.Request<PartyStatusRequest, PartyStatusResponse>(SharedConstants.UrlRoutes.Party.STATUS, request, retryStrategy);
    }

    [PublicAPI]
    public static UniTask<PartyKickResponse> PartyKick(this ServerRequests requests, PartyKickRequest request, ServerCallRetryStrategy retryStrategy) {
        return requests.Request<PartyKickRequest, PartyKickResponse>(SharedConstants.UrlRoutes.Party.KICK, request, retryStrategy);
    }

    [PublicAPI]
    public static UniTask<PartyMakeLeaderResponse> PartyMakeLeader(this ServerRequests requests, PartyMakeLeaderRequest request, ServerCallRetryStrategy retryStrategy) {
        return requests.Request<PartyMakeLeaderRequest, PartyMakeLeaderResponse>(SharedConstants.UrlRoutes.Party.MAKE_LEADER, request, retryStrategy);
    }

    [PublicAPI]
    public static UniTask<PartySetReadyResponse> PartySetReady(this ServerRequests requests, PartySetReadyRequest request, ServerCallRetryStrategy retryStrategy) {
        return requests.Request<PartySetReadyRequest, PartySetReadyResponse>(SharedConstants.UrlRoutes.Party.READY_SET, request, retryStrategy);
    }

    [PublicAPI]
    public static UniTask<LoadoutGetByUserResponse> LoadoutGetByUser(this ServerRequests requests, LoadoutGetByUserRequest request, ServerCallRetryStrategy retryStrategy) {
        return requests.Request<LoadoutGetByUserRequest, LoadoutGetByUserResponse>(SharedConstants.UrlRoutes.Loadout.GET_BY_USER, request, retryStrategy);
    }

    [PublicAPI]
    public static UniTask<MatchmakingJoinResponse> MatchmakingJoin(this ServerRequests requests, MatchmakingJoinRequest request, ServerCallRetryStrategy retryStrategy) {
        return requests.Request<MatchmakingJoinRequest, MatchmakingJoinResponse>(SharedConstants.UrlRoutes.Matchmaking.JOIN, request, retryStrategy);
    }

    [PublicAPI]
    public static UniTask<MatchmakingCancelResponse> MatchmakingCancel(this ServerRequests requests, MatchmakingCancelRequest request, ServerCallRetryStrategy retryStrategy) {
        return requests.Request<MatchmakingCancelRequest, MatchmakingCancelResponse>(SharedConstants.UrlRoutes.Matchmaking.CANCEL, request, retryStrategy);
    }

    [PublicAPI]
    public static UniTask<MatchmakingStatusResponse> MatchmakingStatus(this ServerRequests requests, MatchmakingStatusRequest request, ServerCallRetryStrategy retryStrategy) {
        return requests.Request<MatchmakingStatusRequest, MatchmakingStatusResponse>(SharedConstants.UrlRoutes.Matchmaking.STATUS, request, retryStrategy);
    }
}