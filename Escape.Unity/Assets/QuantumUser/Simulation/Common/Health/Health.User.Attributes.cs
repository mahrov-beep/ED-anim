namespace Quantum {
  using Photon.Deterministic;
  using static HealthAttributeAppliance;

  public unsafe partial struct Health {
    public void Init(Frame f, EntityRef entityRef) {
      SelfEntity   = entityRef;
      CurrentValue = FPMath.Clamp(InitialValue, 0, MaxValue);

      OnInit(f);
    }

    void OnInit(Frame f) {
      ApplyModifiers(f, Temporary);
      ApplyModifiers(f, OneTime);

      InitModifiers(f);
    }

    void InitModifiers(Frame frame) {
      if (Modifiers.Ptr == default) {
        Modifiers = frame.AllocateList<HealthAttributeModifier>();
        return;
      }

      var modifiersList = frame.ResolveList(Modifiers);

      if (modifiersList.Count == 0) {
        return;
      }

      for (int i = 0; i < modifiersList.Count; i++) {
        var modifier = modifiersList.GetPointer(i);
        modifier->Init(frame);
      }
    }

    public void Update(Frame f, EntityRef e) {
      ApplyModifiers(f, Continuous);
      TickModifiers(f);
    }

    public void AddModifier(Frame f, HealthAttributeModifier modifier) {
      if (Modifiers.Ptr == default) {
        return;
      }

      var modifiersList = f.ResolveList(Modifiers);
      modifier.Init(f);
      modifiersList.Add(modifier);

      if (modifier.Appliance is not (Temporary or OneTime)) {
        return;
      }

      modifier.Apply(f, ref this);
    }

    public void RemoveModifier(Frame f, HealthAttributeModifier modifier) {
      if (Modifiers.Ptr == default) {
        return;
      }

      var modifiersList = f.ResolveList(Modifiers);
      modifiersList.Remove(modifier);

      if (modifier.Appliance != Temporary) {
        return;
      }

      modifier.DeApply(f, ref this);
    }

    void ApplyModifiers(Frame f, HealthAttributeAppliance desiredType) {
      if (Modifiers.Ptr == default) {
        return;
      }

      var modifiersList = f.ResolveList(Modifiers);

      if (modifiersList.Count == 0) {
        return;
      }

      for (int i = 0; i < modifiersList.Count; i++) {
        var modifier = modifiersList.GetPointer(i);
        if (modifier->Appliance != desiredType) {
          continue;
        }

        modifier->Apply(f, ref this);
      }
    }

    void TickModifiers(Frame f) {
      if (Modifiers.Ptr == default) {
        return;
      }

      var modifiersList = f.ResolveList(Modifiers);

      if (modifiersList.Count == 0) {
        return;
      }

      for (int i = modifiersList.Count - 1; i >= 0; i--) {
        var modifier = modifiersList.GetPointer(i);
        modifier->Tick(f, out bool isOver);

        if (isOver != true) {
          continue;
        }

        RemoveModifier(f, *modifier);
      }
    }
  }
}