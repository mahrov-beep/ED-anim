namespace Game.ServerRunner.Db;

using Microsoft.EntityFrameworkCore;
using Model;

public class GameDbContext(DbContextOptions<GameDbContext> options) : DbContext(options) {
    public DbSet<DbAuthGuest>   AuthGuest    { get; set; }
    public DbSet<DbUser>        Users        { get; set; }
    public DbSet<DbUserProfile> UserProfiles { get; set; }
    public DbSet<DbFriendship>  Friendship   { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        // for case-insensitive nicknames https://www.postgresql.org/docs/current/citext.html
        modelBuilder.HasPostgresExtension("citext");

        DbAuthGuest.OnModelCreating(modelBuilder.Entity<DbAuthGuest>());
        DbUser.OnModelCreating(modelBuilder.Entity<DbUser>());
        DbUserProfile.OnModelCreating(modelBuilder.Entity<DbUserProfile>());
        DbFriendship.OnModelCreating(modelBuilder.Entity<DbFriendship>());
    }
}