namespace Game.ECS.Components.WorldView {
    using System;
    using Scellecs.Morpeh;

    [Serializable, RequireFieldsInit] public struct WorldVFXRoot : ISingletonComponent {
        public UnityEngine.Transform Root;
    }
}