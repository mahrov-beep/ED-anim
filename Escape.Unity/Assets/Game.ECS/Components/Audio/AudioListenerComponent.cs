namespace Game.ECS.Components.Audio {
    using System;
    using Scellecs.Morpeh;
    using UnityEngine;

    [Serializable, RequireFieldsInit] public struct AudioListenerComponent : ISingletonComponent {
        public AudioListener audioListener;
    }
}