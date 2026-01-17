namespace Multicast {
    using JetBrains.Annotations;
    using Numerics;

    public interface ITimeService {
        [PublicAPI]
        GameTime Now { get; }

        [PublicAPI]
        [MustUseReturnValue]
        bool InPast(GameTime time);

        [PublicAPI]
        [MustUseReturnValue]
        bool InFuture(GameTime time);
    }
}