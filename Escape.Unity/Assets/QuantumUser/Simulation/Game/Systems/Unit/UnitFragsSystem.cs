namespace Quantum {
  public unsafe class UnitFragsSystem : SystemSignalsOnly, ISignalOnUnitDead {
    public void OnUnitDead(Frame f, EntityRef e) {
      if (!f.TryGetPointer(e, out CharacterStateDead* deadState)) {
        return;
      }

      if (!f.TryGetPointer(deadState->KilledBy, out Unit* killerUnit)) {
        return;
      }

      killerUnit->Frags += 1;
    }
  }
}