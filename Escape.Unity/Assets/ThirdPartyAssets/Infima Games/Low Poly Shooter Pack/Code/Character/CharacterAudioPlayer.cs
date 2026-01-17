namespace InfimaGames.LowPolyShooterPack {
    public class CharacterAudioPlayer : AudioPlayer<CharacterAudioLayers> {
        protected override bool AreEqual(CharacterAudioLayers a, CharacterAudioLayers b) => a == b;
    }
}