namespace Tests.E2E;

using System.Net.Http.Headers;
using Game.Shared;
using Game.Shared.DTO;
using MessagePack;
using Multicast;
public sealed class AuthActor {
    private const    string     MESSAGEPACK_CONTENT_TYPE = "application/x-msgpack";
    private readonly HttpClient client;

    private AuthActor(HttpClient client) {
        this.client = client;
    }

    public static AuthActor Create(E2EHost host) {
        var client = host.CreateHttpClient();

        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MESSAGEPACK_CONTENT_TYPE));

        return new AuthActor(client);
    }

    public async Task<ServerResult<GuestAuthResponse>> GuestAsync(string deviceId) {
        var req   = new GuestAuthRequest { DeviceId = deviceId };
        var bytes = MessagePackSerializer.Serialize(req);

        using var content = new ByteArrayContent(bytes);

        content.Headers.ContentType = new MediaTypeHeaderValue(MESSAGEPACK_CONTENT_TYPE);

        using var resp = await client.PostAsync(SharedConstants.UrlRoutes.Auth.GUEST, content);

        resp.EnsureSuccessStatusCode();

        var body = await resp.Content.ReadAsByteArrayAsync();
        return MessagePackSerializer.Deserialize<ServerResult<GuestAuthResponse>>(body);
    }
}