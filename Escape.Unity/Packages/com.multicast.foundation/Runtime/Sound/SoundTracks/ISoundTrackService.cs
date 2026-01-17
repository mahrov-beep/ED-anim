namespace SoundTracks {
    using JetBrains.Annotations;

    public interface ISoundTrackService {
        [PublicAPI]
        void PlayTrack(string key);
        [PublicAPI]
        void StopTrack(string key);
    }
}
