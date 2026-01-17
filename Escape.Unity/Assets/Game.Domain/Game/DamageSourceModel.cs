namespace Game.Domain.Game {
    using System.Collections.Generic;
    using Multicast;
    using Quantum;
    using UniMob;
    using UnityEngine;

    public class DamageSourceModel : Model {
        public DamageSourceModel(Lifetime lifetime) : base(lifetime) { }

        public List<DamageSourceData> DamageSourceScreenNormalizedDirections = new(20);
    }

    public struct DamageSourceData {
        public Vector2   ScreenNormalizedDirection;
        public float     Timer;
        public EntityRef SourceEntityRef;
        public Vector3   WorldPosition;
        public Vector3   ScreenPosition;
        public bool      IsOnScreen;
    }
}