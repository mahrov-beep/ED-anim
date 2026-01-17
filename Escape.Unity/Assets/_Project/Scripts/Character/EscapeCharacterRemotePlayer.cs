using InfimaGames.LowPolyShooterPack;

public class EscapeCharacterRemotePlayer : EscapeCharacterBehaviour {
    public override CharacterTypes GetCharacterType() => CharacterTypes.RemotePlayer;

    protected override void Start() {
        base.Start();

        this.GetCameraDepth().enabled = false;
        this.GetCameraWorld().enabled = false;
    }
}