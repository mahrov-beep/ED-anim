namespace Game.ECS.Components.Audio {
    using System;
    using Scellecs.Morpeh;
    using Sirenix.OdinInspector;
    using UnityEngine;

    [Serializable, RequireFieldsInit] public struct AmbientAudioComponent : IComponent {
        [Required] public AudioSource audioSource;
        [Required] public Transform   transform;
    }
}