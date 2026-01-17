namespace Game.ServerRunner.Core;

using Multicast;
using Multicast.Numerics;

public class ServerTimeService : ITimeService {
    public GameTime Now => GameTime.FromUtcDateTime_UNSAFE(DateTime.UtcNow);

    public bool InPast(GameTime time) {
        return time < this.Now;
    }

    public bool InFuture(GameTime time) {
        return time > this.Now;
    }
}