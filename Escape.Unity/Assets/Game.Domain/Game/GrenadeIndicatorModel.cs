namespace Game.Domain.Game {
    using System.Collections.Generic;
    using Multicast;
    using Scellecs.Morpeh;
    using UniMob;
    using UnityEngine;

    public class GrenadeIndicatorModel : Model {
        public GrenadeIndicatorModel(Lifetime lifetime) : base(lifetime) { }

        public List<GrenadeIndicatorData> GrenadeIndicators { get; set; } = new(8);
    }

    public struct GrenadeIndicatorData {
        public Vector3 WorldPosition;
        public Vector2 ScreenPosition;
        public float ExplosionRadius;
        public bool IsOnScreen;
    }
}

