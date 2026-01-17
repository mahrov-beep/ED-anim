namespace Game.ECS.Scripts {
    using Multicast;
    using UniMob;
    using UnityEngine;

    public class HitMarkUiDynamicData : ILifetimeScope, IUiDynamicData {
        public Lifetime Lifetime { get; }

        public HitMarkUiDynamicData(Lifetime lifetime) {
            this.Lifetime = lifetime;
        }

        [Atom] public Vector3 WorldPos  { get; set; }
        [Atom] public float   Damage    { get; set; }
        [Atom] public float   Alpha     { get; set; }
    }
}