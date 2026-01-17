namespace Tests.Auth;

using E2E;
using Game.Shared.DTO;
using Multicast;
using Xunit;
[Collection("e2e-host")]
public sealed class Runner(E2EHost host) {
    [Fact]
    public async Task GuestAuth_NewUser_CreatesUserAndReturnsToken() {
        AuthActor actor = AuthActor.Create(host);

        string deviceId = "e2e-auth-" + Guid.NewGuid().ToString("N");

        ServerResult<GuestAuthResponse> res = await actor.GuestAsync(deviceId);

        res.ShouldBeNewUser();

        await host.ShouldHaveAuthGuestAsync(deviceId, res.Data!.UserId);
    }

    [Fact]
    public async Task GuestAuth_RepeatDeviceID_SameUser_IsNewFalse() {
        AuthActor actor = AuthActor.Create(host);

        string deviceId = "e2e-auth-" + Guid.NewGuid().ToString("N");

        ServerResult<GuestAuthResponse> first  = await actor.GuestAsync(deviceId);
        ServerResult<GuestAuthResponse> second = await actor.GuestAsync(deviceId);

        first.ShouldBeNewUser();
        second.ShouldBeExistingUser(first.Data.UserId);

        await host.ShouldHaveAuthGuestAsync(deviceId, first.Data.UserId);
    }

    [Fact]
    public async Task GuestAuth_EmptyDevice_ReturnsError() {
        AuthActor actor = AuthActor.Create(host);

        ServerResult<GuestAuthResponse> res = await actor.GuestAsync("");

        res.AssertDataIsNull();
    }

    [Fact]
    public async Task GuestAuth_TooLongDevice_ReturnsError() {
        AuthActor actor = AuthActor.Create(host);

        string deviceId = new string('x', 100);

        ServerResult<GuestAuthResponse> res = await actor.GuestAsync(deviceId);

        res.AssertDataIsNull();
    }
}