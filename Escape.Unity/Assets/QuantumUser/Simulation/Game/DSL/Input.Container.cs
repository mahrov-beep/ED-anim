namespace Quantum {
  public unsafe partial struct InputContainer {
    public bool ButtonAbilityWasPressed => Input.SecondaryAction.WasPressed;
    public bool ButtonAbilityWasReleased => Input.SecondaryAction.WasReleased;
    public bool ButtonAbilityIsDown     => Input.SecondaryAction.IsDown;

    public void ResetAllInput() {
      Input            = default;
      DesiredDirection = default;
      InputAccelerated = default;
    }
  }
}
