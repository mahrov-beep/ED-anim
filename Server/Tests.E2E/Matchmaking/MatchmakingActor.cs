namespace Tests.E2E.Matchmaking;

using System.Net.Http.Headers;
using Game.Shared;
using Game.Shared.DTO;
using MessagePack;
using Multicast;
using Quantum;
public sealed class MatchmakingActor {
    private const    string     MESSAGEPACK_CONTENT_TYPE = "application/x-msgpack";
    private const    string     JOIN_URL                 = "/api/matchmaking/join";
    private const    string     CANCEL_URL               = "/api/matchmaking/cancel";
    private const    string     STATUS_URL               = "/api/matchmaking/status";
    private readonly E2EHost    host;
    private readonly HttpClient client;

    public Guid   Id          { get; }
    public string AccessToken { get; }

    private MatchmakingActor(E2EHost host, HttpClient client, Guid id, string token) {
        this.host   = host;
        this.client = client;
        Id          = id;
        AccessToken = token;
    }

    public static async Task<MatchmakingActor> CreateAsync(E2EHost host, string deviceId) {
        HttpClient client = host.CreateHttpClient();

        (Guid userId, string token) auth = await AuthGuestAsync(client, deviceId);

        SetBearer(client, auth.token);

        return new MatchmakingActor(host, client, auth.userId, auth.token);
    }

    private static async Task<(Guid userId, string token)> AuthGuestAsync(HttpClient client, string deviceId) {
        GuestAuthRequest req   = new GuestAuthRequest { DeviceId = deviceId };
        byte[]           bytes = MessagePackSerializer.Serialize(req);

        using ByteArrayContent content = new ByteArrayContent(bytes);

        content.Headers.ContentType = new MediaTypeHeaderValue(MESSAGEPACK_CONTENT_TYPE);

        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MESSAGEPACK_CONTENT_TYPE));

        using HttpResponseMessage resp = await client.PostAsync(SharedConstants.UrlRoutes.Auth.GUEST, content);

        resp.EnsureSuccessStatusCode();

        byte[] body = await resp.Content.ReadAsByteArrayAsync();

        ServerResult<GuestAuthResponse>? result = MessagePackSerializer.Deserialize<ServerResult<GuestAuthResponse>>(body);

        if (result == null || result.Data == null || string.IsNullOrWhiteSpace(result.Data.AccessToken)) {
            throw new InvalidOperationException("Auth failed");
        }

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

    public Task<ServerResult<MatchmakingJoinResponse>> JoinAsync(string gameModeKey) {
        return PostMsgPackAsync<MatchmakingJoinRequest, MatchmakingJoinResponse>(client, JOIN_URL,
            new MatchmakingJoinRequest { GameModeKey = gameModeKey });
    }

    public Task<ServerResult<MatchmakingCancelResponse>> CancelAsync() {
        return PostMsgPackAsync<MatchmakingCancelRequest, MatchmakingCancelResponse>(client, CANCEL_URL,
            new MatchmakingCancelRequest());
    }

    public Task<ServerResult<MatchmakingStatusResponse>> StatusAsync() {
        return PostMsgPackAsync<MatchmakingStatusRequest, MatchmakingStatusResponse>(client, STATUS_URL,
            new MatchmakingStatusRequest());
    }

    public Task<string> GetAccessTokenAsync() => Task.FromResult(AccessToken);

    /// <summary>
    /// Устанавливает loadout для игрока (тестовый метод).
    /// </summary>
    public async Task<ServerResult<TestSetLoadoutResponse>> SetLoadoutAsync(GameSnapshotLoadout loadout) {
        return await PostMsgPackAsync<TestSetLoadoutRequest, TestSetLoadoutResponse>(client, "/api/user/__test_set_loadout",
            new TestSetLoadoutRequest { UserId = Id, Loadout = loadout });
    }

    /// <summary>
    /// Устанавливает силу игрока (power) путем создания соответствующего loadout.
    /// Convenience метод для тестов.
    /// </summary>
    public async Task SetPlayerPowerAsync(int targetPower) {
        var loadout = CreateLoadoutWithPower(targetPower);
        var result  = await SetLoadoutAsync(loadout);
        if (!result.DataNotNull() || !result.Data.Success) {
            throw new InvalidOperationException($"Failed to set player power for {Id}");
        }
    }

    /// <summary>
    /// Создает loadout с указанными предметами и уровнями для достижения определенной силы игрока.
    /// </summary>
    public static GameSnapshotLoadout CreateLoadoutWithPower(int targetPower) {
        // Простой подход: создаем предметы с определенными уровнями и редкостями
        // Формула power из MatchmakingPower.cs: sum(weight * rarity * level^0.6) * 10
        // Для простоты используем один primary weapon с нужным уровнем

        var slots = new GameSnapshotLoadoutItem[CharacterLoadoutSlotsExtension.CHARACTER_LOADOUT_SLOTS];

        // Расчет нужного уровня для достижения целевого power
        // targetPower = weight * rarity * level^0.6 * 10
        // Для primary weapon: weight=1.0, используем Rare rarity=1.25
        // targetPower = 1.0 * 1.25 * level^0.6 * 10
        // level^0.6 = targetPower / 12.5
        // level = (targetPower / 12.5) ^ (1/0.6)

        double requiredScaledLevel = targetPower / 12.5;
        int    weaponLevel         = (int)Math.Pow(requiredScaledLevel, 1.0 / 0.6);
        weaponLevel = Math.Max(1, Math.Min(weaponLevel, 100)); // Ограничиваем диапазон

        // Primary weapon
        slots[CharacterLoadoutSlots.PrimaryWeapon.ToInt()] = new GameSnapshotLoadoutItem {
            ItemGuid          = Guid.NewGuid().ToString(),
            ItemKey           = SharedConstants.Game.Items.WEAPON_AR,
            WeaponAttachments = null,
            IndexI            = 0,
            IndexJ            = 0,
            Rotated           = false,
            Used              = (ushort)weaponLevel, // Используем Used как level
        };

        // Secondary weapon
        slots[CharacterLoadoutSlots.SecondaryWeapon.ToInt()] = new GameSnapshotLoadoutItem {
            ItemGuid          = Guid.NewGuid().ToString(),
            ItemKey           = SharedConstants.Game.Items.WEAPON_PISTOL_COMMON,
            WeaponAttachments = null,
            IndexI            = 0,
            IndexJ            = 0,
            Rotated           = false,
            Used              = 1,
        };

        // Skin (обязательный)
        slots[CharacterLoadoutSlots.Skin.ToInt()] = new GameSnapshotLoadoutItem {
            ItemGuid          = Guid.NewGuid().ToString(),
            ItemKey           = SharedConstants.Game.Items.SKIN_DEFAULT,
            WeaponAttachments = null,
            IndexI            = 0,
            IndexJ            = 0,
            Rotated           = false,
            Used              = 0,
        };

        return new GameSnapshotLoadout {
            SlotItems  = slots,
            TrashItems = Array.Empty<GameSnapshotLoadoutItem>(),
        };
    }
}