namespace Game.ECS.Scripts {
    using Multicast;
    using UniMob;
    using UnityEngine;

    public class UnitHealthBarUiDynamicData : ILifetimeScope, IUiDynamicData {
        public Lifetime Lifetime { get; }

        public UnitHealthBarUiDynamicData(Lifetime lifetime) {
            this.Lifetime = lifetime;
        }

        [Atom] public Vector3 WorldPos  { get; set; }
        [Atom] public float   Health    { get; set; }
        [Atom] public float   MaxHealth { get; set; }
        [Atom] public float   KnockHealth { get; set; }
        [Atom] public bool    IsKnocked   { get; set; }
        [Atom] public bool    IsBeingRevived { get; set; }
        [Atom] public bool    IsDead    { get; set; }
        [Atom] public string  NickName  { get; set; }
        [Atom] public float   Alpha     { get; set; }
    }
}