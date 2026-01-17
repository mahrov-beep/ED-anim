namespace Tests.E2E.Friends;

using System.Net.Http.Headers;
using Game.Shared;
using Game.Shared.DTO;
using MessagePack;
using Multicast;
public sealed class FriendsActor {
    private const    string     MESSAGEPACK_CONTENT_TYPE = "application/x-msgpack";
    private readonly E2EHost    host;
    private readonly HttpClient client;

    public Guid Id { get; }

    private FriendsActor(E2EHost host, HttpClient client, Guid id) {
        this.host   = host;
        this.client = client;
        Id          = id;
    }

    public static async Task<FriendsActor> CreateAsync(E2EHost host, string deviceId) {
        HttpClient                  client = host.CreateHttpClient();
        (Guid userId, string token) auth   = await AuthGuestAsync(client, deviceId);
        SetBearer(client, auth.token);
        return new FriendsActor(host, client, auth.userId);
    }

    private static async Task<(Guid userId, string token)> AuthGuestAsync(HttpClient client, string deviceId) {
        GuestAuthRequest       req     = new GuestAuthRequest { DeviceId = deviceId };
        byte[]                 bytes   = MessagePackSerializer.Serialize(req);
        using ByteArrayContent content = new ByteArrayContent(bytes);
        content.Headers.ContentType = new MediaTypeHeaderValue(MESSAGEPACK_CONTENT_TYPE);
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MESSAGEPACK_CONTENT_TYPE));
        using HttpResponseMessage resp = await client.PostAsync(SharedConstants.UrlRoutes.Auth.GUEST, content);
        resp.EnsureSuccessStatusCode();
        byte[]                           body   = await resp.Content.ReadAsByteArrayAsync();
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
        byte[]                 bytes   = MessagePackSerializer.Serialize(req);
        using ByteArrayContent content = new ByteArrayContent(bytes);
        content.Headers.ContentType = new MediaTypeHeaderValue(MESSAGEPACK_CONTENT_TYPE);
        using HttpResponseMessage resp = await client.PostAsync(url, content);
        resp.EnsureSuccessStatusCode();
        byte[] body = await resp.Content.ReadAsByteArrayAsync();
        return MessagePackSerializer.Deserialize<ServerResult<TResponse>>(body);
    }

    public Task<ServerResult<FriendsListResponse>> FriendsAsync() {
        return PostMsgPackAsync<FriendsListRequest, FriendsListResponse>(client, SharedConstants.UrlRoutes.Friends.FRIEND_LIST, new FriendsListRequest());
    }

    public Task<ServerResult<IncomingRequestsResponse>> IncomingAsync() {
        return PostMsgPackAsync<IncomingRequestsRequest, IncomingRequestsResponse>(client, SharedConstants.UrlRoutes.Friends.INCOMING_REQUESTS, new IncomingRequestsRequest());
    }

    public Task<ServerResult<FriendAddResponse>> AddAsync(Guid id) {
        return PostMsgPackAsync<FriendAddRequest, FriendAddResponse>(client, SharedConstants.UrlRoutes.Friends.ADD, new FriendAddRequest { Id = id });
    }

    public Task<ServerResult<FriendAcceptResponse>> AcceptAsync(Guid id) {
        return PostMsgPackAsync<FriendAcceptRequest, FriendAcceptResponse>(client, SharedConstants.UrlRoutes.Friends.ACCEPT, new FriendAcceptRequest { Id = id });
    }

    public Task<ServerResult<FriendDeclineResponse>> DeclineAsync(Guid id) {
        return PostMsgPackAsync<FriendDeclineRequest, FriendDeclineResponse>(client, SharedConstants.UrlRoutes.Friends.DECLINE, new FriendDeclineRequest { Id = id });
    }

    public Task<ServerResult<FriendRemoveResponse>> RemoveAsync(Guid id) {
        return PostMsgPackAsync<FriendRemoveRequest, FriendRemoveResponse>(client, SharedConstants.UrlRoutes.Friends.REMOVE, new FriendRemoveRequest { Id = id });
    }

    public Task<ServerResult<FriendsIncomingBulkResponse>> IncomingBulkAsync(FriendsIncomingBulkRequest request) {
        return PostMsgPackAsync<FriendsIncomingBulkRequest, FriendsIncomingBulkResponse>(client, SharedConstants.UrlRoutes.Friends.INCOMING_BULK, request);
    }
}