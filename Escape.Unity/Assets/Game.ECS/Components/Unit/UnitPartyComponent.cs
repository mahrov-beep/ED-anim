namespace Game.ECS.Components.Unit {
    using System;
    using Scellecs.Morpeh;
    using UnityEngine;

    [Serializable, RequireFieldsInit]
    public struct UnitPartyComponent : IComponent {
        public Transform transform;
        
        public string nickName;

        public Guid guid;

        public int level;
    }
}
