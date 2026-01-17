namespace Game.ECS.Scripts {
    using System;
    using Multicast;
    using UniMob;
    using UnityEngine;

    public class UnitPartyUiDynamicData : ILifetimeScope, IUiDynamicData {
        public Lifetime Lifetime { get; }

        public UnitPartyUiDynamicData(Lifetime lifetime) {
            this.Lifetime = lifetime;
        }

        [Atom] public Vector3 WorldPos { get; set; }
        [Atom] public string  NickName { get; set; }
        [Atom] public Guid    Guid     { get; set; }
        [Atom] public int     Level    { get; set; }
    }
}