namespace Quantum {
  public unsafe class InteractiveZoneUnitClearSystem : SystemMainThreadFilter<InteractiveZoneUnitClearSystem.Filter> {
    public struct Filter {
      public EntityRef Entity;

      public Unit* Unit;
    }

    public override void Update(Frame f, ref Filter filter) {
      filter.Unit->NearbyInteractiveZone = EntityRef.None;
    }
  }
}