namespace Quantum {
  using Photon.Deterministic;

  public unsafe partial struct UnitFeatureSprintWithStamina {
    public FP Normalized(SprintSettings settings) => this.current / settings.maxStamina;

    public bool IsDepleted => this.current <= FP._0;

    public bool IsFull(SprintSettings settings) => this.current >= settings.maxStamina;

    public bool InRegenCooldown => this.regenTimer > FP._0;

    /// <summary>
    /// Можно ли прямо сейчас регенерировать (кд не идёт и есть недостающая энергия).
    /// </summary>
    public bool CanRegenerate(SprintSettings settings) =>
      regenTimer <= FP._0 &&
      current < settings.maxStamina;

    /// <summary>
    /// Достаточно ли энергии, чтобы НАЧАТЬ спринт,
    /// и не истёк ли ещё кул-даун регенерации.
    /// </summary>
    public bool CanStartSprint(SprintSettings settings) =>
      current >= settings.maxStamina * settings.minStartRatio;
  }
}