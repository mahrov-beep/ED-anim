namespace Game.ECS.Components.WorldView {
    using System;
    using Scellecs.Morpeh;
    using UnityEngine;

    [Serializable, RequireFieldsInit] public struct HitMarkComponent : IComponent {
        public Vector3 position;
        public float   alpha;
        public float   damage;
        public float   duration;
        public float   currentTime;
    }
}