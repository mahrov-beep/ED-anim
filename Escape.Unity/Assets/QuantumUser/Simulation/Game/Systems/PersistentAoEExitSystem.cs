using UnityEngine.Scripting;
using UnityEngine;
using Photon.Deterministic;

namespace Quantum {
    [Preserve]
    public unsafe class PersistentAoEExitSystem : SystemMainThreadFilter<PersistentAoEExitSystem.Filter> {
        public struct Filter {
            public EntityRef             Entity;
            public InsidePersistentAoE* InsideAoE;
        }

        private const int CheckEveryNTicks = 3;

        public override void Update(Frame f, ref Filter filter) {
            var insideAoE = filter.InsideAoE;
            var targetRef = filter.Entity;
            var attackRef = insideAoE->AttackRef;

            bool shouldExit = false;

            if (!f.Exists(attackRef)) {
                shouldExit = true;
            }
            else if (f.Number - insideAoE->LastUpdateTick >= CheckEveryNTicks) {
                shouldExit = true;

                if (f.TryGetPointer(attackRef, out Attack* attack) &&
                    f.TryGetPointer(attackRef, out Transform3D* attackTransform) &&
                    f.TryGetPointer(targetRef, out Transform3D* targetTransform)) {

                    var attackData = f.FindAsset<AttackData>(attack->AttackData);
                    if (attackData is PersistentAreaOfEffectAttackData aoeData) {
                        var distance = FPVector3.Distance(targetTransform->Position, attackTransform->Position);
                        if (distance <= aoeData.radius) {
                            shouldExit = false;
                        }
                    }
                }
            }

            if (shouldExit) {
                f.Events.AttackHitExited(attackRef, targetRef);
                f.Remove<InsidePersistentAoE>(targetRef);
            }
        }
    }
}

