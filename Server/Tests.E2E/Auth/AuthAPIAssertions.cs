namespace Tests.E2E;

using Game.Shared.DTO;
using Multicast;
public static class AuthAPIAssertions {
    public static void ShouldBeNewUser(this ServerResult<GuestAuthResponse> res) {
        Assert.NotNull(res);
        Assert.NotNull(res.Data);
        Assert.True(res.Data.IsNewUser);
        Assert.NotEqual(Guid.Empty, res.Data.UserId);
        Assert.False(string.IsNullOrWhiteSpace(res.Data.AccessToken));
    }

    public static void ShouldBeExistingUser(this ServerResult<GuestAuthResponse> res, Guid expectedUserId) {
        Assert.True(res.DataNotNull());

        Assert.False(res.Data.IsNewUser);
        Assert.Equal(expectedUserId, res.Data.UserId);

        Assert.False(string.IsNullOrWhiteSpace(res.Data.AccessToken));
    }
}