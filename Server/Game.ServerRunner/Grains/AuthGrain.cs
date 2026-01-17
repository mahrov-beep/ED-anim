namespace Game.ServerRunner.Grains;

using Core;
using Db;
using Db.Model;
using Microsoft.EntityFrameworkCore;
using Multicast;
using Shared.DTO;

public interface IAuthGrain : IGrainWithGuidKey {
    ValueTask<ServerResult<GuestAuthResponse>> AuthGuest(GuestAuthRequest request);
}

public static class GAuthGrainExtensions {
    public static IAuthGrain GetAuthGrain(this IGrainFactory grainFactory) => grainFactory.GetGrain<IAuthGrain>(Guid.Empty);
}

public class AuthGrain(
    ILogger<AuthGrain> logger,
    IDbContextFactory<GameDbContext> dbContextFactory,
    JwtService jwtService
) : Grain, IAuthGrain {
    public async ValueTask<ServerResult<GuestAuthResponse>> AuthGuest(GuestAuthRequest request) {
        var deviceId = request.DeviceId;

        if (string.IsNullOrEmpty(deviceId)) {
            logger.LogError("AuthGuest failed, DeviceId must be not empty");

            return ServerResult.Error(1, "DeviceId must be not empty");
        }

        if (deviceId.Length >= 100) {
            logger.LogError("AuthGuest failed, DeviceId length >= 100");

            return ServerResult.Error(2, "DeviceId length must be less than 100");
        }

        using var context = dbContextFactory.CreateDbContext();

        var authData = await context.AuthGuest
            .Where(it => it.DeviceId == deviceId)
            .Select(it => new { UserId = it.User.Id })
            .FirstOrDefaultAsync();

        if (authData != null) {
            logger.LogInformation("AuthGuest completed, found existing user (UserId={UserId})", authData.UserId);

            return new GuestAuthResponse {
                UserId      = authData.UserId,
                AccessToken = jwtService.GetAccessToken(authData.UserId),
                IsNewUser   = false,
            };
        }

        var userId = Guid.NewGuid();

        context.AuthGuest.Add(new DbAuthGuest {
            DeviceId = deviceId,
            User = new DbUser {
                Id      = userId,
                Created = DateTime.UtcNow,
            },
        });
        
        await context.SaveChangesAsync();

        logger.LogInformation("AuthGuest completed, created new user (UserId={UserId})", userId);

        return new GuestAuthResponse {
            UserId      = userId,
            AccessToken = jwtService.GetAccessToken(userId),
            IsNewUser   = true,
        };
    }
}