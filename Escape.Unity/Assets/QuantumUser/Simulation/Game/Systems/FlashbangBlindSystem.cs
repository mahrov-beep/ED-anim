namespace Quantum {
  public unsafe class FlashbangBlindSystem : SystemMainThreadFilter<FlashbangBlindSystem.Filter> {
    public struct Filter {
      public EntityRef EntityRef;
      public FlashbangBlindEffect* BlindEffect;
    }

    public override void Update(Frame f, ref Filter filter) {
      var blindEffect = filter.BlindEffect;

      if (!blindEffect->IsActive(f)) {
        f.Remove<FlashbangBlindEffect>(filter.EntityRef);
        return;
      }

      if (blindEffect->StartTime+5 == f.Number) {
        f.Events.FlashbangBlindUpdate(filter.EntityRef, blindEffect->GetCurrentStrength(f));
      }
    }
  }
}
