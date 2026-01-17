namespace Quantum {
  public unsafe partial struct CharacterSafe {
    public (int width, int height) GetSafeParameters(Frame f) {
      return (this.Width, this.Height);
    }
  }
}


