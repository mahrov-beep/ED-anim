namespace Multicast.Modules.SoundEffect {
    using Multicast;
    using Collections;
    using GameProperties;
    using global::Sound;
    using Sound;
    using SoundEffects;
    using UnityEngine;

    public class SoundEffectService : ISoundEffectService {
        private const string AUDIO_ROOT_NAME = "Audio Root";

        private readonly LookupCollection<SoundEffectDef> soundsDef;
        private readonly GamePropertiesModel              properties;
        private readonly ICache<SoundAsset>               soundsCache;

        private AudioSource oneShotRoot;

        public SoundEffectService(LookupCollection<SoundEffectDef> soundsDef, GamePropertiesModel properties, ICache<SoundAsset> soundsCache) {
            this.soundsDef   = soundsDef;
            this.properties  = properties;
            this.soundsCache = soundsCache;
        }

        public void Initialize() {
            this.oneShotRoot = CreateAudioSource();

            this.oneShotRoot.gameObject.name = AUDIO_ROOT_NAME;
            Object.DontDestroyOnLoad(this.oneShotRoot.gameObject);
        }

        public void PlayOneShot(string key) {
            if (!this.CanPlay(key, out var soundDef)) {
                return;
            }

            var soundAsset = this.soundsCache.Get(soundDef.audioFile);

            this.oneShotRoot.PlayOneShot(soundAsset.GetClip(), soundDef.volume * this.properties.Get(SoundGameProperties.SoundVolume));
        }

        private bool CanPlay(string soundKey, out SoundEffectDef soundEffectDef) {
            if (!this.soundsDef.TryGet(soundKey, out soundEffectDef)) {
                Debug.LogErrorFormat("Sound - {0} was not found", soundKey);
                return false;
            }

            if (!this.properties.Get(SoundGameProperties.Sound)) {
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