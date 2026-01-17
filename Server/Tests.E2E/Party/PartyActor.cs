namespace Tests.E2E.Party;

using System.Net.Http.Headers;
using Game.Shared;
using Game.Shared.DTO;
using MessagePack;
using Multicast;
public sealed class PartyActor {
    private const    string     MESSAGEPACK_CONTENT_TYPE = "application/x-msgpack";
    private readonly E2EHost    host;
    private readonly HttpClient client;

    public Guid Id { get; }

    private PartyActor(E2EHost host, HttpClient client, Guid id) {
        this.host   = host;
        this.client = client;
        Id          = id;
    }

    public static async Task<PartyActor> CreateAsync(E2EHost host, string deviceId) {
        HttpClient                  client = host.CreateHttpClient();
        (Guid userId, string token) auth   = await AuthGuestAsync(client, deviceId);
        SetBearer(client, auth.token);
        return new PartyActor(host, client, auth.userId);
    }

    private static async Task<(Guid userId, string token)> AuthGuestAsync(HttpClient client, string deviceId) {
        GuestAuthRequest req = new GuestAuthRequest { DeviceId = deviceId };

        byte[] bytes = MessagePackSerializer.Serialize(req);

        using ByteArrayContent content = new ByteArrayContent(bytes);

        content.Headers.ContentType = new MediaTypeHeaderValue(MESSAGEPACK_CONTENT_TYPE);

        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MESSAGEPACK_CONTENT_TYPE));

        using HttpResponseMessage resp = await client.PostAsync(SharedConstants.UrlRoutes.Auth.GUEST, content);

        resp.EnsureSuccessStatusCode();

        byte[] body = await resp.Content.ReadAsByteArrayAsync();

        ServerResult<GuestAuthResponse>? result = MessagePackSerializer.Deserialize<ServerResult<GuestAuthResponse>>(body);

        if (result == null || result.Data == null || string.IsNullOrWhiteSpace(result.Data.AccessToken))
            throw new InvalidOperationException("Auth failed");

        return (result.Data.UserId, result.Data.AccessToken);
    }

    private static void SetBearer(HttpClient client, string token) {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MESSAGEPACK_CONTENT_TYPE));
    }

    private static async Task<ServerResult<TResponse>> PostMsgPackAsync<TRequest, TResponse>(HttpClient client, string url, TRequest req)
        where TResponse : class, IServerResponse {
        byte[] bytes = MessagePackSerializer.Serialize(req);

        using ByteArrayContent content = new ByteArrayContent(bytes);

        content.Headers.ContentType = new MediaTypeHeaderValue(MESSAGEPACK_CONTENT_TYPE);

        using HttpResponseMessage resp = await client.PostAsync(url, content);

        resp.EnsureSuccessStatusCode();

        byte[] body = await resp.Content.ReadAsByteArrayAsync();

        return MessagePackSerializer.Deserialize<ServerResult<TResponse>>(body);
    }

    public Task<ServerResult<PartyInviteResponse>> InviteAsync(Guid targetUserId) {
        return PostMsgPackAsync<PartyInviteRequest, PartyInviteResponse>(client, SharedConstants.UrlRoutes.Party.INVITE,
            new PartyInviteRequest { TargetUserId = targetUserId });
    }

    public Task<ServerResult<PartyAcceptInviteResponse>> AcceptAsync(Guid leaderUserId) {
        return PostMsgPackAsync<PartyAcceptInviteRequest, PartyAcceptInviteResponse>(client, SharedConstants.UrlRoutes.Party.ACCEPT,
            new PartyAcceptInviteRequest { LeaderUserId = leaderUserId });
    }

    public Task<ServerResult<PartyDeclineInviteResponse>> DeclineAsync(Guid leaderUserId) {
        return PostMsgPackAsync<PartyDeclineInviteRequest, PartyDeclineInviteResponse>(client, SharedConstants.UrlRoutes.Party.DECLINE,
            new PartyDeclineInviteRequest { LeaderUserId = leaderUserId });
    }

    public Task<ServerResult<PartyLeaveResponse>> LeaveAsync(Guid leaderUserId) {
        return PostMsgPackAsync<PartyLeaveRequest, PartyLeaveResponse>(client, SharedConstants.UrlRoutes.Party.LEAVE,
            new PartyLeaveRequest { LeaderUserId = leaderUserId });
    }

    public Task<ServerResult<PartyStatusResponse>> StatusAsync() {
        return PostMsgPackAsync<PartyStatusRequest, PartyStatusResponse>(client, SharedConstants.UrlRoutes.Party.STATUS,
            new PartyStatusRequest { });
    }

    public Task<ServerResult<PartyKickResponse>> KickAsync(Guid leaderUserId, Guid targetUserId) {
        return PostMsgPackAsync<PartyKickRequest, PartyKickResponse>(client, SharedConstants.UrlRoutes.Party.KICK,
            new PartyKickRequest { LeaderUserId = leaderUserId, TargetUserId = targetUserId });
    }

    public Task<ServerResult<PartyMakeLeaderResponse>> MakeLeaderAsync(Guid leaderUserId, Guid targetUserId) {
        return PostMsgPackAsync<PartyMakeLeaderRequest, PartyMakeLeaderResponse>(client, SharedConstants.UrlRoutes.Party.MAKE_LEADER,
            new PartyMakeLeaderRequest { LeaderUserId = leaderUserId, TargetUserId = targetUserId });
    }

    public Task<ServerResult<PartyStartGameResponse>> StartAsync(string gameModeKey) {
        return PostMsgPackAsync<PartyStartGameRequest, PartyStartGameResponse>(client, SharedConstants.UrlRoutes.Party.START,
            new PartyStartGameRequest { GameModeKey = gameModeKey });
    }

    public Task<ServerResult<PartySetReadyResponse>> SetReadyAsync(Guid leaderUserId, bool isReady) {
        return PostMsgPackAsync<PartySetReadyRequest, PartySetReadyResponse>(client, SharedConstants.UrlRoutes.Party.READY_SET,
            new PartySetReadyRequest { LeaderUserId = leaderUserId, IsReady = isReady });
    }
}