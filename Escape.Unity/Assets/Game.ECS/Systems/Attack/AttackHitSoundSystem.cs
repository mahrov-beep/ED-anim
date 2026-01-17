namespace Game.ECS.Systems.Attack {
    using System;
    using System.Collections.Generic;
    using Multicast;
    using Player;
    using Quantum;
    using Services.Photon;
    using UnityEngine;
    using SystemBase = Scellecs.Morpeh.SystemBase;

    public class AttackHitSoundSystem : SystemBase {
        [Inject] private LocalPlayerSystem localPlayerSystem;
        [Inject] private PhotonService    photonService;

        private class ClipSoundData {
            public AudioSource        AudioSource;
            public float              InitialVolume;
            public float              FadeOutTimer;
            public float              FadeOutDuration;
            public bool               IsFadingOut;
            public HashSet<EntityRef> ActiveAttacks = new HashSet<EntityRef>();
        }

        private readonly Dictionary<AudioClip, ClipSoundData> clipSounds   = new Dictionary<AudioClip, ClipSoundData>(4);
        private readonly Dictionary<EntityRef, AudioClip>     attackToClip = new Dictionary<EntityRef, AudioClip>(8);
        private          GameObject                           poolRoot;

        private IDisposable attackHitSubscription;
        private IDisposable attackExitSubscription;
        private IDisposable attackPerformedSubscription;

        private const float FadeOutDuration = 0.3f;


        public override void OnAwake() {
            attackHitSubscription = QuantumEvent.SubscribeManual<EventAttackHitSynced>(OnAttackHit);
            attackExitSubscription = QuantumEvent.SubscribeManual<EventAttackHitExited>(OnAttackExit);
            attackPerformedSubscription = QuantumEvent.SubscribeManual<EventAttackPerformedSynced>(OnPerfomedAttack);

            poolRoot = new GameObject("AttackHitSoundPool");
            UnityEngine.Object.DontDestroyOnLoad(poolRoot);
        }

        private void OnPerfomedAttack(EventAttackPerformedSynced evt) {
            if (evt.attackAsset != null && evt.attackAsset.enableHitSound && evt.attackAsset.hitSound != null) {
                var clip = evt.attackAsset.hitSound;
                if (clip && clip.loadState == AudioDataLoadState.Unloaded) {
                    clip.LoadAudioData();
                }
            }
        }

        private unsafe void OnAttackHit(EventAttackHitSynced evt) {
            if (localPlayerSystem.HasNotLocalEntityRef(out var localPlayerRef)) {
                return;
            }

            if (evt.targetRef != localPlayerRef) {
                return;
            }

            if (evt.attackAsset == null || !evt.attackAsset.enableHitSound || evt.attackAsset.hitSound == null) {
                return;
            }

            var attackRef = evt.attackRef;
            var clip      = evt.attackAsset.hitSound;
            var volume    = evt.attackAsset.hitSoundVolume.AsFloat;

            if (!clipSounds.TryGetValue(clip, out var clipData)) {
                var src = CreateAudioSource(clip, volume);
                if (src == null) {
                    return;
                }
                clipData = new ClipSoundData {
                    AudioSource     = src,
                    InitialVolume   = volume,
                    FadeOutTimer    = 0f,
                    FadeOutDuration = 0f,
                    IsFadingOut     = false,
                    ActiveAttacks   = new HashSet<EntityRef>()
                };
                clipSounds[clip] = clipData;
            }

            clipData.IsFadingOut            = false;
            clipData.FadeOutTimer           = 0f;
            clipData.AudioSource.volume     = clipData.InitialVolume = volume;
            if (!clipData.AudioSource.isPlaying) {
                clipData.AudioSource.Play();
                //   Debug.Log($"Playing audio source: {clipData.AudioSource.name}");
            }
            clipData.ActiveAttacks.Add(attackRef);
            attackToClip[attackRef] = clip;
        }

        private void OnAttackExit(EventAttackHitExited evt) {
            if (localPlayerSystem.HasNotLocalEntityRef(out var localPlayerRef)) {
                return;
            }

            if (evt.targetRef != localPlayerRef) {
                return;
            }

            var attackRef = evt.attackRef;
            if (!attackToClip.TryGetValue(attackRef, out var clip)) {
                return;
            }
            if (!clipSounds.TryGetValue(clip, out var clipData)) {
                return;
            }
            clipData.ActiveAttacks.Remove(attackRef);
            attackToClip.Remove(attackRef);
            if (clipData.ActiveAttacks.Count == 0 && !clipData.IsFadingOut) {
                clipData.IsFadingOut     = true;
                clipData.FadeOutTimer    = 0f;
                clipData.FadeOutDuration = FadeOutDuration;
            }
        }

        public override unsafe void OnUpdate(float deltaTime) {
            foreach (var kvp in clipSounds) {
                var clipData = kvp.Value;

                if (clipData.IsFadingOut) {
                    clipData.FadeOutTimer += deltaTime;
                    float t = clipData.FadeOutTimer / clipData.FadeOutDuration;

                    if (clipData.AudioSource != null) {
                        clipData.AudioSource.volume = Mathf.Lerp(clipData.InitialVolume, 0f, t);
                    }

                    if (t >= 1f) {
                        if (clipData.AudioSource != null) {
                            clipData.AudioSource.Stop();
                            //  Debug.Log($"Stopping audio source: {clipData.AudioSource.name}");
                        }
                        clipData.IsFadingOut = false;
                    }
                }
            }
        }

        public override void Dispose() {
            attackHitSubscription?.Dispose();
            attackExitSubscription?.Dispose();
            attackPerformedSubscription?.Dispose();

            foreach (var kvp in clipSounds) {
                if (kvp.Value?.AudioSource != null) {
                    UnityEngine.Object.Destroy(kvp.Value.AudioSource.gameObject);
                }
            }
            clipSounds.Clear();
            attackToClip.Clear();

            if (poolRoot != null) {
                UnityEngine.Object.Destroy(poolRoot);
                poolRoot = null;
            }
        }

        private AudioSource CreateAudioSource(AudioClip clip, float volume) {
            if (clip == null) {
                return null;
            }

            var go = new GameObject($"AttackHitSound_{clip.name}");
            go.transform.SetParent(poolRoot.transform, false);

            var audioSource = go.AddComponent<AudioSource>();
            audioSource.clip         = clip;
            audioSource.loop         = true;
            audioSource.volume       = volume;
            audioSource.priority     = 128;
            audioSource.spatialBlend = 0f;
            audioSource.rolloffMode  = AudioRolloffMode.Linear;
            audioSource.playOnAwake  = false;
            audioSource.mute         = false;

            return audioSource;
        }
    }
}
