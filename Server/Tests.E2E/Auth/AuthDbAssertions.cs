namespace Tests.E2E;

using Game.ServerRunner.Db;
using Game.ServerRunner.Db.Model;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
public static class AuthDbAssertions {
    [MustDisposeResource]
    private static GameDbContext CreateDb(E2EHost host) {
        var options = new DbContextOptionsBuilder<GameDbContext>().UseNpgsql(host.PublicPostgresConnectionString).Options;
        return new GameDbContext(options);
    }

    public static async Task<DbAuthGuest?> GetAuthGuestAsync(this E2EHost host, string deviceId) {
        await using var db = CreateDb(host);
        return await db.AuthGuest.Include(x => x.User).SingleOrDefaultAsync(x => x.DeviceId == deviceId);
    }

    public static async Task ShouldHaveAuthGuestAsync(this E2EHost host, string deviceId, Guid expectedUserId) {
        var row = await host.GetAuthGuestAsync(deviceId);
        Assert.NotNull(row);
        Assert.NotNull(row.User);
        Assert.Equal(expectedUserId, row.User.Id);
    }
}