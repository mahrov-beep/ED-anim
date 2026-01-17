namespace Quantum.Commands {
  using Photon.Deterministic;

  public class SetSystemEnabledCommand<T> : CharacterCommandBase where T : SystemBase {
    public bool Enabled;

    public override void Serialize(BitStream stream) {
      stream.Serialize(ref this.Enabled);
    }

    public override void Execute(Frame f, EntityRef characterEntity) {
      if (this.Enabled) {
        f.SystemEnable<T>();
      } else {
        f.SystemDisable<T>();
      }
    }
  }
}

