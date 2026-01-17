namespace SoundEffects {
    using JetBrains.Annotations;

    public interface ISoundEffectService {
        [PublicAPI]
        void PlayOneShot(string key);
    }
}