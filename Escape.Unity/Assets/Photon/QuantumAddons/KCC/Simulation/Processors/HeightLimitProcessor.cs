namespace Quantum {
    using Photon.Deterministic;

    public unsafe class HeightLimitProcessor : KCCProcessor, IBeforeMove {
        public FP heightLimit = -FP._1;
        [RangeEx(1, 2)]
        public FPVector3 worldCenter = FPVector3.Up * 10;
        [RangeEx(0, 1)]
        public FP directionDotThreshold = FP._0_33;

        public void BeforeMove(KCCContext context, KCCProcessorInfo processorInfo) {
            var     f    = context.Frame;
            ref var data = ref context.KCC->Data;

            var velocity = (data.DynamicVelocity + data.KinematicVelocity);
            var nextPos  = data.BasePosition + velocity * f.DeltaTime;

            if (nextPos.Y >= heightLimit)
                return;

            var horizontalVel = new FPVector3(velocity.X, FP._0, velocity.Z);
            if (horizontalVel != default) {
                var dir      = horizontalVel.Normalized;
                var toCenter = worldCenter - data.BasePosition;
                toCenter.Y = FP._0;

                if (toCenter != default && FPVector3.Dot(dir, toCenter.Normalized) > directionDotThreshold) {
                    data.DynamicVelocity.Y = (heightLimit - data.BasePosition.Y) / f.DeltaTime;
                    return;
                }
            }

            data.DynamicVelocity   = default;
            data.KinematicVelocity = default;
        }
    }
}