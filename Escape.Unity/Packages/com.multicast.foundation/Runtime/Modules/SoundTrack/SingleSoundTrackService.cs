namespace Multicast.Modules.SoundTrack {
    using System;
    using DG.Tweening;
    using Multicast;
    using Collections;
    using GameProperties;
    using global::Sound;
    using Sound;
    using SoundEffect;
    using SoundTracks;
    using UniMob;
    using UnityEngine;
    using Object = UnityEngine.Object;

    public class SingleSoundTrackService : ISoundTrackService {
        private const string AUDIO_SOURCE_NAME  = "Sound Track Source";
        private const float  TRANSITION_DURATION = 3.5f;

        private AudioSource audioSource;

        private string currentTrack = null;
        private float  targetVolume = 1f;

        private readonly Lifetime                        lifetime;
        private readonly LookupCollection<SoundTrackDef> soundTracksDef;
        private readonly GamePropertiesModel             properties;
        private readonly ICache<SoundAsset>              clipsCache;

        private Tween transition;

        public void Initialize() {
            this.audioSource      = CreateAudioSource();
            this.audioSource.name = AUDIO_SOURCE_NAME;
            this.audioSource.loop = true;
            Object.DontDestroyOnLoad(this.audioSource.gameObject);

            Atom.Reaction(this.lifetime, () => this.properties.Get(SoundGameProperties.Music), (soundEnabled) => this.ToggleTrack(soundEnabled));
        }

        private void ToggleTrack(bool soundEnabled) {
            this.transition?.Kill();
            this.audioSource.volume = soundEnabled ? this.targetVolume : 0;
        }

        public SingleSoundTrackService(Lifetime lifetime, LookupCollection<SoundTrackDef> soundsDef, GamePropertiesModel properties, ICache<SoundAsset> clipsCache) {
            this.lifetime       = lifetime;
            this.soundTracksDef = soundsDef;
            this.properties     = properties;
            this.clipsCache     = clipsCache;
        }

        public void PlayTrack(string key) {
            if (!this.CanPlay(key, out var soundTrackDef)) {
                return;
            }

            if (this.currentTrack == key) {
                return;
            }

            this.transition?.Kill();

            if (this.audioSource.isPlaying) {
                this.SwitchTrackSmoothly(soundTrackDef);
            }
            else {
                this.PlayTrackSmoothly(soundTrackDef);
            }
        }

        private void SwitchTrackSmoothly(SoundTrackDef soundEffectDef) {
            this.StartVolumeTransition(0, () => this.PlayTrackSmoothly(soundEffectDef));
        }

        private void StartVolumeTransition(float targetVolume, Action callback = null) {
            this.transition = DOTween.To(() => this.audioSource.volume, it => { this.audioSource.volume = it; }, targetVolume, TRANSITION_DURATION)
                .OnComplete(() => callback?.Invoke());
        }

        private void PlayTrackSmoothly(SoundTrackDef soundTrackDef) {
            this.currentTrack = soundTrackDef.key;
            var soundAsset = this.clipsCache.Get(soundTrackDef.audioFile);
            this.audioSource.volume = 0;

            this.targetVolume = soundTrackDef.volume * this.properties.Get(SoundGameProperties.MusicVolume);

            this.StartVolumeTransition(this.targetVolume * (this.properties.Get(SoundGameProperties.Music) ? 1f : 0f));

            this.audioSource.clip = soundAsset.GetClip();
            this.audioSource.Play();
        }

        public void StopTrack(string key) {
            if (key == this.currentTrack && this.audioSource.isPlaying) {
                this.transition?.Kill();
                this.audioSource.Stop();
            }
        }

        private bool CanPlay(string soundKey, out SoundTrackDef soundEffectDef) {
            if (!this.soundTracksDef.TryGet(soundKey, out soundEffectDef)) {
                Debug.LogErrorFormat("Sound - {0} was not found", soundKey);
                return false;
            }

            return true;
        }

        private static AudioSource CreateAudioSource() {
            var gameObject = new GameObject();
            var source     = gameObject.AddComponent<AudioSource>();
            source.volume = 1;
            return source;
        }
    }
}