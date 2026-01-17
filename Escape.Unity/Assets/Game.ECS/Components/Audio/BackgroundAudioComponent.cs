namespace Game.ECS.Components.Audio {
    using System;
    using Scellecs.Morpeh;
    using Sirenix.OdinInspector;
    using UnityEngine;

    [Serializable, RequireFieldsInit] public struct BackgroundAudioComponent : ISingletonComponent {
        [Required] public AudioSource audioSource;

        [NonSerialized] public float defaultVolume;
    }
}