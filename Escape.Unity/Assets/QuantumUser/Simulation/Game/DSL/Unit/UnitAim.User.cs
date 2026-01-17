namespace Quantum {
  public unsafe partial struct UnitAim {
    public static ComponentHandler<UnitAim> OnAdd => static (f, e, c) => {
      c->AimEntity = f.Create(f.GameModeAiming.unitAim);
    };    
    
    public static ComponentHandler<UnitAim> OnRemove => static (f, e, c) => {
      f.Destroy(c->AimEntity);
    };
  }
}