namespace Game.Domain.Game {
    using System.Collections.Generic;
    using Multicast;
    using Quantum;
    using UniMob;
    using UnityEngine;

    public class ListenedCueModel : Model {
        public ListenedCueModel(Lifetime lifetime) : base(lifetime) { }

        public List<CueData> StepsScreenNormalizedDirections  = new(6);
        public List<CueData> ShootsScreenNormalizedDirections = new(20);
    }

    public struct CueData {
        public Vector2   ScreenNormalizedDirection;
        public float     Timer;
        public EntityRef SourceEntityRef;
        public Vector3   WorldPosition;
        public Vector3   ScreenPosition;
        public bool      IsOnScreen;
    }
}