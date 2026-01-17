namespace Game.ServerRunner.Grains;

public interface IUserPartyGrain : IGrainWithGuidKey {
    ValueTask<Guid> GetLeader();
    ValueTask<bool> TryJoin(Guid leaderUserId);
    ValueTask       Leave();
}
public static class UserPartyGrainExtensions {
    public static Guid            GetUserId(this IUserPartyGrain grain)                      => grain.GetPrimaryKey();
    public static IUserPartyGrain GetUserPartyGrain(this IGrainFactory factory, Guid userId) => factory.GetGrain<IUserPartyGrain>(userId);
}
public sealed class UserPartyGrain : Grain, IUserPartyGrain {
    private Guid leaderId;

    public override Task OnActivateAsync(CancellationToken cancellationToken) {
        leaderId = Guid.Empty;
        return Task.CompletedTask;
    }

    public ValueTask<Guid> GetLeader() {
        return ValueTask.FromResult(leaderId);
    }

    public ValueTask<bool> TryJoin(Guid leaderUserId) {
        if (leaderUserId == Guid.Empty) {
            return ValueTask.FromResult(false);
        }

        if (leaderId == Guid.Empty || leaderId == leaderUserId) {
            leaderId = leaderUserId;
            return ValueTask.FromResult(true);
        }

        return ValueTask.FromResult(false);
    }

    public ValueTask Leave() {
        leaderId = Guid.Empty;
        return ValueTask.CompletedTask;
    }
}