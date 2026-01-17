// ReSharper disable InconsistentNaming

namespace InfimaGames.LowPolyShooterPack {
    using System;
    using System.Collections.Generic;
    using Sirenix.OdinInspector;
    using UnityEngine;

    public class CharacterConfig : ScriptableObject {
        [TabGroup("Layers")]
        [ValueDropdown(nameof(GetLayerDropdown)), ValidateInput(nameof(ValidateLayer))]
        public int localViewLayer;

        [TabGroup("Layers")]
        [ValueDropdown(nameof(GetLayerDropdown)), ValidateInput(nameof(ValidateLayer))]
        public int remotePlayerLayer;

        [TabGroup("Camera Heights (LocalView)")]
        [HideLabel]
        public CameraHeights LocalViewCameraHeights;

        [TabGroup("Camera Heights (RemotePlayer)")]
        [HideLabel]
        public CameraHeights RemotePlayerCameraHeight;

        [Title("Walking Multipliers")]
        [Tooltip("Value to multiply the walking speed by when the character is moving forward."), SerializeField]
        [Range(0.0f, 1.0f)]
        public float walkingMultiplierForward = 1.0f;

        [Tooltip("Value to multiply the walking speed by when the character is moving sideways.")]
        [Range(0.0f, 1.0f)]
        public float walkingMultiplierSideways = 1.0f;

        [Tooltip("Value to multiply the walking speed by when the character is moving backwards.")]
        [Range(0.0f, 1.0f)]
        public float walkingMultiplierBackwards = 1.0f;

        [Title("Animation")]
        [Tooltip("Determines how smooth the turning animation is.")]
        public float dampTimeTurning = 0.4f;

        [Tooltip("Determines how smooth the locomotion blendspace is.")]
        public float dampTimeLocomotion = 0.25f;

        [Tooltip("How smoothly we play aiming transitions. Beware that this affects lots of things!")]
        public float dampTimeAiming = 0.08f;

        [Tooltip("Interpolation speed for the running offsets.")]
        public float runningInterpolationSpeed = 5.0f;

        [Tooltip("Determines how fast the character's weapons are aimed.")]
        public float aimingSpeedMultiplier = 0.9f;

        [Title("Field Of View")]
        [Tooltip("Normal world field of view.")]
        public float fieldOfView = 95.0f;

        [Tooltip("Multiplier for the field of view while running.")]
        public float fieldOfViewRunningMultiplier = 1.1f;

        [Tooltip("Weapon-specific field of view.")]
        public float fieldOfViewWeapon = 65.0f;

        [TabGroup("Audio")]
        public AudioClipsSettings audioClipsMelee, audioClipsGrenadeThrow;

        [TabGroup("Audio")]
        public AudioClipsSettings audioClipsStep, audioClipsStepCrouching, audioClipStepRunning;
        
        [TabGroup("Audio")]
        public AudioClipsSettings audioClipsAimStart, audioClipsAimStop;

        [TabGroup("Audio")]
        public AudioClipsSettings audioClipsJumpStart, audioClipsJumpLand;

        [TabGroup("Audio")] public float minimalTimeForGroundedAudio = 0.25f;

        [TabGroup("Audio")]
        public AudioClipsSettings audioClipsHit, audioClipsDied, audioHitEnemy, audioHeadshotEnemy;
        
        [TabGroup("Audio")]
        public AudioClipsSettings audioClipsHealing;
        
        [TabGroup("Audio")]
        public AudioClipsSettings audioClipsCrouchDown, audioClipsCrouchUp;
        
        private bool ValidateLayer(int layer, ref string error, ref InfoMessageType type) => LayersUtil.ValidateLayer(layer, ref error, ref type);

        private IEnumerable<ValueDropdownItem<int>> GetLayerDropdown() => LayersUtil.GetLayerDropdown();

        [Serializable, InlineProperty]
        public class CameraHeights {
            public CameraHeight Normal     = new CameraHeight();
            public CameraHeight CrouchIdle = new CameraHeight();
            public CameraHeight CrouchMove = new CameraHeight();
            public CameraHeight Died       = new CameraHeight();
            public CameraHeight Knocked    = new CameraHeight();
            public CameraHeight Roll       = new CameraHeight();
        }

        [Serializable, InlineProperty(LabelWidth = 60)]
        public class CameraHeight {
            public Vector3        Offset;
            public SpringSettings Spring = SpringSettings.Default();
        }
    }
}