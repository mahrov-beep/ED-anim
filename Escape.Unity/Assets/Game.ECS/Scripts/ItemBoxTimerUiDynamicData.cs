namespace Game.ECS.Scripts {
    using Multicast;
    using UniMob;
    using UnityEngine;

    public class ItemBoxTimerUiDynamicData : ILifetimeScope, IUiDynamicData {
        public Lifetime Lifetime { get; }

        public ItemBoxTimerUiDynamicData(Lifetime lifetime) {
            this.Lifetime = lifetime;
        }

        [Atom] public Vector3 WorldPos { get; set; }
        [Atom] public float   Progress { get; set; }
        [Atom] public int     Timer    { get; set; }
    }
}