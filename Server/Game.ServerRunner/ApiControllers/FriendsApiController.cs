namespace Game.ServerRunner.ApiControllers;

using Db;
using Db.Model;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Multicast;
using Shared;
using Shared.DTO;
using Grains;
using Microsoft.EntityFrameworkCore;
using Game.ServerRunner.Grains;
[ApiController]
[Produces(ServerConstants.CONTENT_TYPE_MSGPACK)]
public class FriendsApiController(
    ILogger<FriendsApiController> log,
    IGrainFactory grainFactory,
    IDbContextFactory<GameDbContext> dbContextFactory)
    : Controller {

    [Authorize]
    [HttpPost(SharedConstants.UrlRoutes.Friends.FRIEND_LIST)]
    public async Task<ServerResult<FriendsListResponse>> Friends(FriendsListRequest request) {
        if (!this.HttpContext.TryGetUserId(out var userId)) {
            return ServerResult.Error(1, "Invalid user id");
        }

        var userFriendsGrain = grainFactory.GetUserFriendsGrain(userId);

        var friends = await userFriendsGrain.GetFriends();

        await using var db = await dbContextFactory.CreateDbContextAsync();

        var nicknames = await db.UserProfiles
            .Where(p => friends.Contains(p.Id))
            .Select(p => new { p.Id, p.NickName })
            .ToListAsync();

        var map = nicknames.ToDictionary(x => x.Id, x => x.NickName);

        var friendsDto = new List<FriendInfoDto>(friends.Length);
        foreach (var id in friends) {
            var status = await grainFactory.GetGrain<IUserStatusGrain>(id).GetStatus();

            friendsDto.Add(new FriendInfoDto {
                Id       = id,
                NickName = map.TryGetValue(id, out var n) ? n : string.Empty,
                Status   = status,
            });
        }

        return new FriendsListResponse {
            Friends = friendsDto.ToArray(),
        };
    }

    [Authorize]
    [HttpPost(SharedConstants.UrlRoutes.Friends.ONLINE)]
    public async Task<ServerResult<FriendsListResponse>> Online(FriendsListRequest request) {
        if (!this.HttpContext.TryGetUserId(out var userId)) {
            return ServerResult.Error(1, "Invalid user id");
        }

        var userFriendsGrain = grainFactory.GetUserFriendsGrain(userId);
        var friends          = await userFriendsGrain.GetFriends();

        var statusMap = new Dictionary<Guid, EUserStatus>(friends.Length);
        foreach (var fid in friends) {
            var statusGrain = grainFactory.GetGrain<IUserStatusGrain>(fid);

            var status = await statusGrain.GetStatus();

            if (status == EUserStatus.InMenu || status == EUserStatus.InGame) {
                statusMap[fid] = status;
            }
        }
        var online = statusMap.Keys;

        await using var db = await dbContextFactory.CreateDbContextAsync();

        var nicknames = await db.UserProfiles
            .Where(p => online.Contains(p.Id))
            .Select(p => new { p.Id, p.NickName })
            .ToListAsync();

        var map = nicknames.ToDictionary(x => x.Id, x => x.NickName);

        var friendsDto = new List<FriendInfoDto>(statusMap.Count);
        foreach (var id in online) {
            friendsDto.Add(new FriendInfoDto {
                Id       = id,
                NickName = map.TryGetValue(id, out var n) ? n : string.Empty,
                Status   = statusMap[id],
            });
        }

        return new FriendsListResponse { Friends = friendsDto.ToArray() };
    }

    [Authorize]
    [HttpPost(SharedConstants.UrlRoutes.Friends.INCOMING_REQUESTS)]
    public async Task<ServerResult<IncomingRequestsResponse>> IncomingRequests(IncomingRequestsRequest request) {
        if (!this.HttpContext.TryGetUserId(out var userId)) {
            return ServerResult.Error(1, "Invalid user id");
        }

        var userFriendsGrain = grainFactory.GetUserFriendsGrain(userId);

        var incoming = await userFriendsGrain.GetIncomingRequests();

        await using var db = await dbContextFactory.CreateDbContextAsync();

        var nicknames = await db.UserProfiles
            .Where(p => incoming.Contains(p.Id))
            .Select(p => new { p.Id, p.NickName })
            .ToListAsync();

        var map = nicknames.ToDictionary(x => x.Id, x => x.NickName);

        var incomingDto = incoming.Select(id => new FriendInfoDto {
                Id       = id,
                NickName = map.TryGetValue(id, out var n) ? n : string.Empty,
                Status   = EUserStatus.Offline,
            })
            .ToArray();

        return new IncomingRequestsResponse {
            IncomingRequests = incomingDto,
        };
    }

    [Authorize]
    [HttpPost(SharedConstants.UrlRoutes.Friends.ADD)]
    public async Task<ServerResult<FriendAddResponse>> Add(FriendAddRequest request) {
        if (request is null || request.Id == Guid.Empty) {
            return ServerResult.Error(1, "Invalid request");
        }

        if (!this.HttpContext.TryGetUserId(out Guid selfId)) {
            return ServerResult.Error(2, "Invalid user id");
        }

        if (selfId == request.Id) {
            return ServerResult.Error(3, "Cannot add yourself");
        }

        await using var db = await dbContextFactory.CreateDbContextAsync();

        bool exists = await db.Users.AnyAsync(u => u.Id == request.Id);
        if (!exists) {
            return ServerResult.Error(5, "User not found");
        }

        IUserFriendsGrain grain = grainFactory.GetUserFriendsGrain(selfId);

        FriendActionResult result = await grain.RequestFriendship(request.Id);

        FriendAddResponse resp = new FriendAddResponse {
            Result  = result.Status,
            Created = result.Status == EFriendActionStatus.PendingCreated,
        };

        return resp;
    }

    [Authorize]
    [HttpPost(SharedConstants.UrlRoutes.Friends.ADD_BY_NICKNAME)]
    public async Task<ServerResult<FriendAddByNicknameResponse>> AddByNickname(FriendAddByNicknameRequest request) {
        if (request is null || string.IsNullOrWhiteSpace(request.NickName)) {
            return ServerResult.Error(1, "Invalid request");
        }

        if (!this.HttpContext.TryGetUserId(out Guid selfId)) {
            return ServerResult.Error(2, "Invalid user id");
        }

        await using var db = await dbContextFactory.CreateDbContextAsync();

        var nickname = request.NickName.Trim();
        var user = await db.UserProfiles
            .Where(p => p.NickName == nickname)
            .Select(p => new { p.Id })
            .SingleOrDefaultAsync();

        if (user is null) {
            return ServerResult.Error(3, "User not found");
        }

        if (user.Id == selfId) {
            return ServerResult.Error(4, "Cannot add yourself");
        }

        var exists = await db.Users.AnyAsync(u => u.Id == user.Id);
        if (!exists) {
            return ServerResult.Error(3, "User not found");
        }

        var grain  = grainFactory.GetUserFriendsGrain(selfId);
        var result = await grain.RequestFriendship(user.Id);

        return new FriendAddByNicknameResponse {
            Result         = result.Status,
            Created        = result.Status == EFriendActionStatus.PendingCreated,
            ResolvedUserId = user.Id,
        };
    }

    [Authorize]
    [HttpPost(SharedConstants.UrlRoutes.Friends.ACCEPT)]
    public async Task<ServerResult<FriendAcceptResponse>> Accept(FriendAcceptRequest request) {
        if (request is null || request.Id == Guid.Empty) {
            return ServerResult.Error(1, "Invalid request");
        }

        if (!this.HttpContext.TryGetUserId(out Guid selfId)) {
            return ServerResult.Error(2, "Invalid user id");
        }

        if (selfId == request.Id) {
            return ServerResult.Error(3, "Cannot accept yourself");
        }

        var grain = grainFactory.GetUserFriendsGrain(selfId);

        var result = await grain.AcceptFriendship(request.Id);

        return new FriendAcceptResponse { Result = result.Status };
    }

    [Authorize]
    [HttpPost(SharedConstants.UrlRoutes.Friends.DECLINE)]
    public async Task<ServerResult<FriendDeclineResponse>> Decline(FriendDeclineRequest request) {
        if (request is null || request.Id == Guid.Empty) {
            return ServerResult.Error(1, "Invalid request");
        }

        if (!this.HttpContext.TryGetUserId(out Guid selfId)) {
            return ServerResult.Error(2, "Invalid user id");
        }

        if (selfId == request.Id) {
            return ServerResult.Error(3, "Cannot decline yourself");
        }

        var grain = grainFactory.GetUserFriendsGrain(selfId);

        var result = await grain.RejectFriendshipRequest(request.Id);

        return new FriendDeclineResponse { Result = result.Status };
    }

    [Authorize]
    [HttpPost(SharedConstants.UrlRoutes.Friends.REMOVE)]
    public async Task<ServerResult<FriendRemoveResponse>> Remove(FriendRemoveRequest request) {
        if (request is null || request.Id == Guid.Empty) {
            return ServerResult.Error(1, "Invalid request");
        }

        if (!this.HttpContext.TryGetUserId(out Guid selfId)) {
            return ServerResult.Error(2, "Invalid user id");
        }

        if (selfId == request.Id) {
            return ServerResult.Error(3, "Cannot remove yourself");
        }

        var grain = grainFactory.GetUserFriendsGrain(selfId);

        var result = await grain.RemoveFriend(request.Id);

        if (result.Status != EFriendActionStatus.Removed) {
            return ServerResult.Error(4, $"Can't remove user {request.Id}");
        }

        return new FriendRemoveResponse { Result = EFriendActionStatus.Removed };
    }

    [Authorize]
    [HttpPost(SharedConstants.UrlRoutes.Friends.INCOMING_BULK)]
    public async Task<ServerResult<FriendsIncomingBulkResponse>> IncomingBulk(FriendsIncomingBulkRequest request) {
        if (request is null) {
            return ServerResult.Error(1, "Invalid request");
        }

        if (!this.HttpContext.TryGetUserId(out Guid selfId)) {
            return ServerResult.Error(2, "Invalid user id");
        }

        var grain = grainFactory.GetUserFriendsGrain(selfId);

        await grain.ProcessIncomingBulk(request.Action);

        return new FriendsIncomingBulkResponse();
    }
}