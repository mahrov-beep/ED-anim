namespace Game.ServerRunner.Grains;

using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Multicast;
using Orleans;
using Shared.DTO;
using Shared.ServerEvents;
public interface IMatchmakingGrain : IGrainWithStringKey {
    ValueTask<ServerResult<MatchmakingJoinResponse>> Join(Guid userId, string queueKey);

    ValueTask<ServerResult<MatchmakingCancelResponse>> Cancel(Guid userId);

    ValueTask<MatchmakingStatusResponse> Status(Guid userId);

    ValueTask ClearMatchedState(Guid userId);

    ValueTask ClearForTesting();
}
public sealed class MatchmakingGrain(IGrainFactory grainFactory, ILogger<MatchmakingGrain> logger) : Grain, IMatchmakingGrain {
    private readonly Dictionary<Guid, string>            userToQueue      = [];
    private readonly LinkedList<MatchmakingGroup>        queue            = [];
    private readonly Dictionary<Guid, MatchJoinContract> matched          = [];
    private readonly Dictionary<Guid, DateTime>          enqueuedAt       = [];
    private readonly Dictionary<string, DateTime>        lastTryFormMatch = [];

    public async ValueTask<ServerResult<MatchmakingJoinResponse>> Join(Guid userId, string queueKey) {
        if (userId == Guid.Empty || string.IsNullOrWhiteSpace(queueKey)) {
            return ServerResult.Error(1, "invalid");
        }

        if (matched.ContainsKey(userId)) {
            return new MatchmakingJoinResponse { Result = EMatchmakingJoinStatus.AlreadyInGame };
        }

        if (userToQueue.ContainsKey(userId)) {
            return new MatchmakingJoinResponse { Result = EMatchmakingJoinStatus.AlreadyQueued };
        }

        Guid leaderId = await grainFactory.GetUserPartyGrain(userId).GetLeader();

        if (leaderId != Guid.Empty && leaderId != userId) {
            return ServerResult.Error(2, "not_leader");
        }

        Guid[] allMembersId;

        if (leaderId == Guid.Empty) {
            allMembersId = [userId];
        }
        else {
            var status = await grainFactory.GetGrain<IPartyGrain>(leaderId).GetStatus();

            var members = status.Members ?? new[] { leaderId };
            var ready   = status.ReadyMembers ?? Array.Empty<Guid>();
            if (members.Any(m => m != leaderId && !ready.Contains(m))) {
                return ServerResult.Error(3, "not_all_ready");
            }

            allMembersId = members;
        }

        foreach (var memberId in allMembersId) {
            var memberStatus = await grainFactory.GetUserStatusGrain(memberId).GetStatus();
            if (memberStatus == EUserStatus.InGame) {
                logger.LogWarning("MM: cannot join - member {UserId} is in game", memberId);
                return ServerResult.Error(5, "member_in_game");
            }
        }

        var group = new MatchmakingGroup(queueKey, allMembersId);

        foreach (var m in allMembersId) {
            userToQueue[m] = queueKey;
            enqueuedAt[m]  = DateTime.UtcNow;
        }

        queue.AddLast(group);

        var leaderUserId = leaderId == Guid.Empty ? userId : leaderId;
        await PublishPartyMatchmakingStarted(leaderUserId, allMembersId, queueKey);

        logger.LogInformation("MM: join group {Count} users for queue {Queue}", allMembersId.Length, queueKey);

        await TryFormMatch(queueKey);

        return new MatchmakingJoinResponse { Result = EMatchmakingJoinStatus.Enqueued };
    }

    public async ValueTask<ServerResult<MatchmakingCancelResponse>> Cancel(Guid userId) {
        if (matched.ContainsKey(userId)) {
            logger.LogWarning("MM: user {UserId} attempted to cancel while already matched", userId);
            return ServerResult.Error(4, "already_matched");
        }

        if (!userToQueue.TryGetValue(userId, out var queueKey)) {
            return new MatchmakingCancelResponse();
        }

        // Determine party leader and members for event publishing
        Guid   leaderId = await grainFactory.GetUserPartyGrain(userId).GetLeader();
        Guid[] members;
        if (leaderId != Guid.Empty) {
            var partyStatus = await grainFactory.GetGrain<IPartyGrain>(leaderId).GetStatus();
            members = partyStatus.Members ?? [leaderId];
        }
        else {
            leaderId = userId;
            members  = [userId];
        }

        var node = queue.First;
        while (node != null) {
            var next = node.Next;

            if (node.Value.QueueKey == queueKey && node.Value.Members.Contains(userId)) {
                foreach (var m in node.Value.Members) {
                    userToQueue.Remove(m);
                    enqueuedAt.Remove(m);
                }

                queue.Remove(node);

                break;
            }

            node = next;
        }

        logger.LogInformation("MM: cancel user {UserId} from queue {Queue}", userId, queueKey);

        // Publish cancel event to party members (including solo players)
        await PublishPartyMatchmakingCanceled(leaderId, members, userId);

        return new MatchmakingCancelResponse();
    }

    public ValueTask<MatchmakingStatusResponse> Status(Guid userId) {
        if (matched.TryGetValue(userId, out var contract)) {
            return ValueTask.FromResult(new MatchmakingStatusResponse {
                Status      = EMatchmakingQueueStatus.Matched,
                GameModeKey = contract.RoomPropsMinimal.TryGetValue("mode", out var mk) ? mk : string.Empty,
                Join        = contract,
            });
        }

        if (userToQueue.TryGetValue(userId, out var qk)) {
            if (!lastTryFormMatch.TryGetValue(qk, out var lastTry) || (DateTime.UtcNow - lastTry).TotalSeconds >= 1) {
                lastTryFormMatch[qk] = DateTime.UtcNow;
                _                    = TryFormMatch(qk);
            }

            if (matched.TryGetValue(userId, out var contract2)) {
                return ValueTask.FromResult(new MatchmakingStatusResponse {
                    Status      = EMatchmakingQueueStatus.Matched,
                    GameModeKey = contract2.RoomPropsMinimal.TryGetValue("mode", out var mk2) ? mk2 : string.Empty,
                    Join        = contract2,
                });
            }

            return ValueTask.FromResult(new MatchmakingStatusResponse {
                Status      = EMatchmakingQueueStatus.Queued,
                GameModeKey = qk,
                Join        = null,
            });
        }

        return ValueTask.FromResult(new MatchmakingStatusResponse {
            Status      = EMatchmakingQueueStatus.Idle,
            GameModeKey = string.Empty,
            Join        = null,
        });
    }

    private async Task TryFormMatch(string queueKey) {
        var required = GetRequiredPlayers(queueKey);

        if (required <= 0) {
            return;
        }

        var collected = new List<MatchmakingGroup>();

        var node = queue.First;
        while (node != null) {
            var g = node.Value;

            if (g.QueueKey == queueKey) {
                collected.Add(g);
            }

            node = node.Next;
        }

        if (collected.Count == 0) {
            return;
        }

        var realPlayers = collected.SelectMany(x => x.Members).Distinct().ToArray();

        if (realPlayers.Length >= required) {
            await FormMatchNow(queueKey, collected.Take(GetGroupCountForPlayers(collected, required)).ToList());
            return;
        }

        var oldestEnqueueTime = DateTime.MaxValue;
        foreach (var uid in realPlayers) {
            if (enqueuedAt.TryGetValue(uid, out var t) && t < oldestEnqueueTime) {
                oldestEnqueueTime = t;
            }
        }

        if (oldestEnqueueTime == DateTime.MaxValue) {
            return;
        }

        var waitTime = DateTime.UtcNow - oldestEnqueueTime;

        if (waitTime.TotalSeconds >= 10) {
            var groupsToMatch = collected.Take(GetGroupCountForPlayers(collected, required)).ToList();
            await FormMatchNow(queueKey, groupsToMatch);
        }
    }

    private int GetGroupCountForPlayers(List<MatchmakingGroup> groups, int required) {
        var total = 0;
        var count = 0;
        foreach (var g in groups) {
            total += g.Members.Length;
            count++;

            if (total >= required) {
                break;
            }
        }

        return count;
    }

    private async Task FormMatchNow(string queueKey, List<MatchmakingGroup> groups) {
        var realPlayers = groups.SelectMany(x => x.Members).Distinct().ToArray();
        var botCount    = 0;
        var matchId     = Guid.NewGuid().ToString("N");
        var region      = SelectRegion();
        var roomName    = $"m_{matchId}";

        var props = new Dictionary<string, string> {
            ["mode"] = queueKey,
            ["bots"] = botCount.ToString(),
        };

        var ticket = GenerateTicket(matchId, roomName, region, queueKey, realPlayers);

        var contract = new MatchJoinContract {
            Region           = region,
            RoomName         = roomName,
            ExpectedUsers    = realPlayers,
            RoomPropsMinimal = props,
            Ticket           = ticket,
        };

        foreach (var uid in realPlayers) {
            matched[uid] = contract;
            userToQueue.Remove(uid);
            enqueuedAt.Remove(uid);
        }

        var toRemove = new HashSet<MatchmakingGroup>(groups);
        var cur      = queue.First;
        while (cur != null) {
            var next = cur.Next;

            if (toRemove.Contains(cur.Value)) {
                queue.Remove(cur);
            }

            cur = next;
        }

        logger.LogInformation("MM: match {MatchId} formed with {Real} players + {Bots} bots for {Queue}",
            matchId, realPlayers.Length, botCount, queueKey);

        // Publish matched event for all parties/players
        await PublishPartyMatchmakingMatched(groups, contract, queueKey);
    }

    private async Task PublishPartyMatchmakingStarted(Guid leaderUserId, Guid[] members, string queueKey) {
        var evt = new PartyMatchmakingStartedAppServerEvent { LeaderUserId = leaderUserId, GameModeKey = queueKey };
        foreach (var uid in members) {
            await this.GetStreamProvider(OrleansConstants.Streams.SERVER_EVENTS)
                .GetStream<IAppServerEvent>(OrleansConstants.Streams.Ids.AppServerEventsForUser(uid))
                .OnNextAsync(evt);
        }
    }

    private async Task PublishPartyMatchmakingCanceled(Guid leaderUserId, Guid[] members, Guid canceledBy) {
        var evt = new PartyMatchmakingCanceledAppServerEvent {
            LeaderUserId     = leaderUserId,
            CanceledByUserId = canceledBy,
        };

        foreach (var uid in members) {
            await this.GetStreamProvider(OrleansConstants.Streams.SERVER_EVENTS)
                .GetStream<IAppServerEvent>(OrleansConstants.Streams.Ids.AppServerEventsForUser(uid))
                .OnNextAsync(evt);
        }
    }

    private async Task PublishPartyMatchmakingMatched(List<MatchmakingGroup> groups, MatchJoinContract contract, string queueKey) {
        foreach (var group in groups) {
            var leaderId = await grainFactory.GetUserPartyGrain(group.Members[0]).GetLeader();
            if (leaderId == Guid.Empty) {
                leaderId = group.Members[0];
            }

            var evt = new PartyMatchmakingMatchedAppServerEvent {
                LeaderUserId  = leaderId,
                RoomName      = contract.RoomName,
                Region        = contract.Region,
                ExpectedUsers = contract.ExpectedUsers,
                GameModeKey   = queueKey,
            };

            foreach (var uid in group.Members) {
                await this.GetStreamProvider(OrleansConstants.Streams.SERVER_EVENTS)
                    .GetStream<IAppServerEvent>(OrleansConstants.Streams.Ids.AppServerEventsForUser(uid))
                    .OnNextAsync(evt);
            }
        }
    }

    private static int GetRequiredPlayers(string queueKey) {
        if (queueKey.Contains("duo", StringComparison.OrdinalIgnoreCase) ||
            queueKey.Contains("2vs2", StringComparison.OrdinalIgnoreCase)) {
            return 4;
        }

        return 6;
    }

    private static string SelectRegion() {
        return "eu";
    }

    public ValueTask ClearMatchedState(Guid userId) {
        matched.Remove(userId);
        return ValueTask.CompletedTask;
    }

    public ValueTask ClearForTesting() {
        userToQueue.Clear();
        queue.Clear();
        matched.Clear();
        enqueuedAt.Clear();
        lastTryFormMatch.Clear();
        logger.LogInformation("MM: cleared all state for testing");
        return ValueTask.CompletedTask;
    }

    private static string GenerateTicket(string matchId, string roomName, string region, string queueKey, Guid[] expected) {
        var payload = matchId + "|" + region + "|" + roomName + "|" + queueKey + "|" + string.Join(",", expected.Select(x => x.ToString("N")));
        var key     = Environment.GetEnvironmentVariable("JWTSIGNINGKEY") ?? "fallback-signing-key";

        using var h = new HMACSHA256(Encoding.UTF8.GetBytes(key));

        var sig   = h.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var token = Convert.ToBase64String(Encoding.UTF8.GetBytes(payload)) + "." + Convert.ToBase64String(sig);

        return token;
    }

    private readonly record struct MatchmakingGroup(string QueueKey, Guid[] Members);
}