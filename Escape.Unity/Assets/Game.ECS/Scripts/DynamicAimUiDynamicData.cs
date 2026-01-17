namespace Game.ECS.Scripts {
    using Multicast;
    using UniMob;
    using UnityEngine;

    public class DynamicAimUiDynamicData : ILifetimeScope, IUiDynamicData {
        public Lifetime Lifetime { get; }

        public DynamicAimUiDynamicData(Lifetime lifetime) {
            this.Lifetime = lifetime;
        }

        [Atom] public Vector3 TargetAimWorldPos  { get; set; }
        [Atom] public Vector3 ForwardAimWorldPos { get; set; }

        [Atom] public int   Bullets         { get; set; }
        [Atom] public int   MaxBullets      { get; set; }
        [Atom] public float ShootingSpread  { get; set; }
        [Atom] public float Quality         { get; set; }
        [Atom] public bool  HasTarget       { get; set; }
        [Atom] public bool  IsReloading     { get; set; }
        [Atom] public bool  IsTargetBlocked { get; set; }
        [Atom] public float AimPercent      { get; set; }
        [Atom] public bool  Deactivated     { get; set; }
    }
}