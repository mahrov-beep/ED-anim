namespace Game.ECS.Components.Unit {
    using System;
    using Scellecs.Morpeh;

    [Serializable, RequireFieldsInit] public struct UnitHealthBarMarker : IComponent {
        public float alpha;
    }
}