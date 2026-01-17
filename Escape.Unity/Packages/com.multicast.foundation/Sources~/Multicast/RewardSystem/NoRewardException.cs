namespace Multicast.RewardSystem {
    using System;

    public class NoRewardException : Exception {
        public NoRewardException(RewardDef def)
            : base($"No reward at '{def.GetType().Name}'") {
        }
    }
}