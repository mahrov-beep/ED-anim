namespace Multicast.Sound {
    using GameProperties;

    public static class SoundGameProperties {
        public static readonly BoolGamePropertyName  Sound       = new("Sound", true);
        public static readonly BoolGamePropertyName  Music       = new("Music", true);
        public static readonly FloatGamePropertyName SoundVolume = new("SoundVolume", 0.5f);
        public static readonly FloatGamePropertyName MusicVolume = new("MusicVolume", 0.5f);
    }
}