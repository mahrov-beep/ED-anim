namespace Tests.E2E.Friends;

using System;
using System.Threading.Tasks;
using Game.ServerRunner.Db;
using Game.ServerRunner.Db.Model;
using Microsoft.EntityFrameworkCore;
using Xunit;
public static class FriendsDbAssertions {
    private static GameDbContext CreateDb(E2EHost host) {
        DbContextOptions<GameDbContext> options = new DbContextOptionsBuilder<GameDbContext>().UseNpgsql(host.PublicPostgresConnectionString).Options;
        return new GameDbContext(options);
    }

    public static async Task<DbFriendship?> GetFriendshipRowAsync(this E2EHost host, Guid user1, Guid user2) {
        await using GameDbContext db = CreateDb(host);
        return await db.Friendship.SingleOrDefaultAsync(x =>
            x.RequesterId == user1 && x.AddresseeId == user2 ||
            x.RequesterId == user2 && x.AddresseeId == user1);
    }

    public static async Task ShouldNotHaveFriendshipAsync(this E2EHost host, Guid user1, Guid user2) {
        DbFriendship? row = await host.GetFriendshipRowAsync(user1, user2);
        Assert.Null(row);
    }

    public static async Task ShouldHavePendingFromToAsync(this E2EHost host, Guid requesterId, Guid addresseeId) {
        DbFriendship? row = await host.GetFriendshipRowAsync(requesterId, addresseeId);
        Assert.NotNull(row);
        Assert.Equal(EFriendStatus.Pending, row.Status);
        Assert.Equal(requesterId, row.RequesterId);
        Assert.Equal(addresseeId, row.AddresseeId);
        Assert.Null(row.AcceptedAt);
    }

    public static async Task ShouldHaveAcceptedFriendshipAsync(this E2EHost host, Guid user1, Guid user2) {
        DbFriendship? row = await host.GetFriendshipRowAsync(user1, user2);
        Assert.NotNull(row);
        Assert.Equal(EFriendStatus.Accepted, row.Status);
        Assert.NotNull(row.AcceptedAt);
    }
}