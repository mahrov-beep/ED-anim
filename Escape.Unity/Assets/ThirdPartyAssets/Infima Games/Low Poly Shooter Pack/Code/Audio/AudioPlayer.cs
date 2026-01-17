namespace InfimaGames.LowPolyShooterPack {
    using System;
    using System.Collections.Generic;
    using Audio;
    using JetBrains.Annotations;
    using Sirenix.OdinInspector;
    using UnityEngine;
    using UnityEngine.Audio;

    public abstract class AudioPlayer<TLayerEnum> : MonoBehaviour
        where TLayerEnum : Enum {
        [SerializeField]
        [TableList(AlwaysExpanded = true, ShowPaging = false)]
        private AudioLayer[] layers = Array.Empty<AudioLayer>();

        private readonly List<(float startTime, AudioLayer layer, AudioClip clip, float volumeScale, float speedNormalized, bool useBoosts)> delayed = new();

        private AudioListener listener;

        private float volumeMultiplierBoost;
        private float maxDistanceMultiplierBoost;

        public void SetBoosts(float volumeMultiplier, float maxDistanceMultiplier) {
            this.volumeMultiplierBoost      = volumeMultiplier;
            this.maxDistanceMultiplierBoost = maxDistanceMultiplier;
        }
        
        private void Start() {
            foreach (var audioLayer in this.layers) {
                audioLayer.Create();
            }
        }

        private void Update() {
            this.SyncAudioListener();
            this.PlayerDelayedAudio();
        }

        [PublicAPI]
        public void PlayOneShot(TLayerEnum layer, AudioClipsSettings settings, float delay = 0f, float speedNormalized = 1f, bool useBoosts = true) {
            this.PlayOneShot(layer, settings.GetClip(), settings.VolumeScale, delay, speedNormalized: speedNormalized, useBoosts: useBoosts);
        }
        
        [PublicAPI]
        public void StopLayer(TLayerEnum layer) {
            var audioLayer = GetLayer(layer);

            audioLayer.Stop();
        }

        [PublicAPI]
        public void PlayOneShot(TLayerEnum layer, AudioClip clip, float volumeScale = 1f, float delay = 0f, float speedNormalized = 1f, bool useBoosts = true) {
            if (clip == null) {
                return;
            }

            var audioLayer = GetLayer(layer);

            if (audioLayer == null) {
                Debug.LogError($"No layer '{layer}' found on {this.GetType().Name} ({this.name})", this.gameObject);
                return;
            }

            if (delay <= 0.01f) {
                audioLayer.PlayOneShot(clip, volumeScale, speedNormalized, useBoosts ? this.volumeMultiplierBoost : 0f, useBoosts ? this.maxDistanceMultiplierBoost : 0f);
            }
            else {
                this.delayed.Add((Time.time + delay, audioLayer, clip, volumeScale, speedNormalized, useBoosts));
            }
        }

        private AudioLayer GetLayer(TLayerEnum layer) {
            foreach (var audioLayer in this.layers) {
                if (!AreEqual(audioLayer.Layer, layer)) {
                    continue;
                }

                return audioLayer;
            }

            return null;
        }

        private void SyncAudioListener() {
            if (this.listener != null) {
                return;
            }

            if (Camera.main == null || !Camera.main.TryGetComponent(out this.listener)) {
                Debug.LogError("No AudioListener found on Camera.main. Use slow FindFirstObjectByType");
                this.listener = FindFirstObjectByType<AudioListener>();
            }

            foreach (var audioLayer in this.layers) {
                audioLayer.UpdateListener(this.listener);
            }
        }

        private void PlayerDelayedAudio() {
            var time = Time.time;

            for (var i = 0; i < this.delayed.Count; i++) {
                var el = this.delayed[i];
                if (el.startTime < time) {
                    continue;
                }

                this.delayed[i] = default;

                el.layer.PlayOneShot(el.clip, el.volumeScale, el.speedNormalized, el.useBoosts ? this.volumeMultiplierBoost : 0f, el.useBoosts ? this.maxDistanceMultiplierBoost : 0f);
            }

            this.delayed.RemoveAll(static it => it.layer == null);
        }

        protected abstract bool AreEqual(TLayerEnum a, TLayerEnum b);

        [Serializable]
        public class AudioLayer {
            [SerializeField]
            private TLayerEnum layer;

            [SerializeField, Required]
            private Transform transform;

            [SerializeField, PropertyRange(0, 256)]
            [Tooltip("0 - Highest, 256 - Lowest")]
            private int priority = 128;

            [SerializeField, PropertyRange(0, 1), FoldoutGroup("Settings")]
            private float volume = 1f;

            [SerializeField, PropertyRange(-3, 3), FoldoutGroup("Settings")]
            private float pitch = 1f;

            [SerializeField, MinValue(0), FoldoutGroup("Settings"), LabelWidth(90)]
            private float minDistance = 2;

            [SerializeField, MinValue(0), FoldoutGroup("Settings"), LabelWidth(90)]
            [Tooltip("Если AudioPlayer находится дальше чем на cullDistance от AudioListener, то звук воспроизводиться не будет")]
            private float cullDistance = 100;

            [SerializeField, FoldoutGroup("Settings")]
            private AudioRolloffMode rollOff = AudioRolloffMode.Logarithmic;

            [SerializeField, FoldoutGroup("Settings")]
            private AudioMixerGroup output;

            [SerializeField, FoldoutGroup("Settings")]
            [Tooltip("Если true то одновременно будет проигрывать только один звук. Остальные звуки будут отброшены")]
            private bool single;

            [SerializeField, FoldoutGroup("Settings")]
            private float minTimer = 0.2f;
            
            [SerializeField, FoldoutGroup("Settings")]
            private float maxTimer = 0.5f;
            
            [SerializeField, FoldoutGroup("Settings")]
            private float minInterval = 0.08f;
            
            private int lastFrame = -1;
            
            private double nextDsp = 0.0;

            private AudioSource audioSource;
            private Transform   audioListenerTransform;

            internal TLayerEnum Layer => this.layer;

            internal void Create() {
                this.audioSource = this.transform.gameObject.AddComponent<AudioSource>();

                this.audioSource.priority              = this.priority;
                this.audioSource.volume                = this.volume;
                this.audioSource.pitch                 = this.pitch;
                this.audioSource.outputAudioMixerGroup = this.output;
                this.audioSource.minDistance           = this.minDistance;
                this.audioSource.maxDistance           = this.cullDistance;
                this.audioSource.rolloffMode           = this.rollOff;
                this.audioSource.spatialBlend          = 1.0f; // Full 3D
            }

            internal void UpdateListener(AudioListener newListener) {
                this.audioListenerTransform = newListener != null ? newListener.transform : null;
            }

            internal void PlayOneShot(AudioClip clip, float volumeScale, float speedNormalized, float volumeMultiplierBoost, float maxDistanceMultiplierBoost) {
                if (this.audioListenerTransform == null) {
                    return;
                }

                if (Vector3.SqrMagnitude(this.audioListenerTransform.position - this.transform.position) > this.cullDistance * this.cullDistance) {
                    return;
                }
                
                var timeFrame = Time.frameCount;

                if (timeFrame == this.lastFrame) {
                    return;
                }

                this.lastFrame = timeFrame;
            
                var gap = Mathf.Lerp(this.maxTimer, this.minTimer, Mathf.Clamp01(speedNormalized));
                gap = Mathf.Max(gap, this.minInterval);

                var now = UnityEngine.AudioSettings.dspTime;
            
                if (now < nextDsp) {
                    return;
                }
            
                this.nextDsp = now + gap;
                
                this.audioSource.volume = this.volume * volumeScale;
                this.audioSource.clip   = clip;

                if (this.single) {
                    if (this.audioSource.isPlaying) {
                        return;
                    }

                    this.audioSource.loop = false;

                    AudioHelper.PlayAudioSourceWithBoost(
                        this.volume * volumeScale,
                        this.minDistance,
                        this.cullDistance,
                        volumeMultiplierBoost,
                        maxDistanceMultiplierBoost,
                        this.priority,
                        this.audioSource,
                        this.audioSource.clip);
                }
                else {
                    AudioHelper.PlayAudioSourceWithBoost(
                        this.volume * volumeScale,
                        this.minDistance,
                        this.cullDistance,
                        volumeMultiplierBoost,
                        maxDistanceMultiplierBoost,
                        this.priority,
                        this.audioSource,
                        this.audioSource.clip);
                }
            }
            
            public void Stop() {
                this.audioSource.Stop();
            }
        }
    }
}