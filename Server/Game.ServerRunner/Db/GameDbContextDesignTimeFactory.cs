namespace Game.ServerRunner.Db;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class GameDbContextDesignTimeFactory : IDesignTimeDbContextFactory<GameDbContext> {
    public GameDbContext CreateDbContext(string[] args) {
        var options = new DbContextOptionsBuilder<GameDbContext>();
        options.UseNpgsql();

        return new GameDbContext(options.Options);
    }
}