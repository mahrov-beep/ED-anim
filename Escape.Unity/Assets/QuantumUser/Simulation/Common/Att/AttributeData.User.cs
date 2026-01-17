namespace Quantum {
public unsafe partial struct AttributeData {
    public bool IsNotZero => CurrentValue > 0;

    public void Init(Frame f, EntityRef e) {
        CurrentValue = InitialValue;

        OnInit(f, e);
    }

    private void OnInit(Frame f, EntityRef e) {
        ApplyModifiers(f, e, EModifierAppliance.Temporary);
        ApplyModifiers(f, e, EModifierAppliance.OneTime);

        InitModifiers(f);
    }

    public bool Update(Frame f, EntityRef e) {
        ApplyModifiers(f, e, EModifierAppliance.Continuous);
        return TickModifiers(f, e);
    }

    private void InitModifiers(Frame f) {
        if (Modifiers.Ptr == default) {
            Modifiers = f.AllocateList<AttributeModifier>();
            return;
        }

        var modifiersList = f.ResolveList(Modifiers);

        if (modifiersList.Count == 0) {
            return;
        }

        for (int i = 0; i < modifiersList.Count; i++) {
            AttributeModifier* modifier = modifiersList.GetPointer(i);
            modifier->Init(f);
        }
    }

    public void AddModifier(Frame f, EntityRef e, AttributeModifier modifierTemplate) {
        if (Modifiers.Ptr == default) {
            return;
        }

        var modifiersList = f.ResolveList(Modifiers);
        modifiersList.Add(modifierTemplate);

        var                modifierIndex = modifiersList.Count - 1;
        AttributeModifier* modifier      = modifiersList.GetPointer(modifierIndex);

        modifier->Init(f);

        if (modifier->ModifierAppliance == EModifierAppliance.Temporary || modifier->ModifierAppliance == EModifierAppliance.OneTime) {
            modifier->Apply(f, e, ref CurrentValue);
        }

        if (modifier->ModifierAppliance == EModifierAppliance.OneTime) {
          modifiersList.RemoveAt(modifierIndex);
        }
    }

    public void RemoveModifierAt(Frame f, EntityRef e, int modifierIndex) {
        if (Modifiers.Ptr == default) {
            return;
        }

        var modifiersList = f.ResolveList(Modifiers);

        AttributeModifier* modifier = modifiersList.GetPointer(modifierIndex);

        if (modifier->ModifierAppliance == EModifierAppliance.Temporary) {
            modifier->DeApply(f, e, ref CurrentValue);
        }

        modifiersList.RemoveAt(modifierIndex);
    }

    private void ApplyModifiers(Frame f, EntityRef e, EModifierAppliance desiredType) {
        if (Modifiers.Ptr == default) {
            return;
        }

        var modifiersList = f.ResolveList(Modifiers);

        if (modifiersList.Count == 0) {
            return;
        }

        for (int i = 0; i < modifiersList.Count; i++) {
            AttributeModifier* modifier = modifiersList.GetPointer(i);
            if (modifier->ModifierAppliance == desiredType) {
                modifier->Apply(f, e, ref CurrentValue);
            }
        }
    }

    private bool TickModifiers(Frame f, EntityRef e) {
        if (Modifiers.Ptr == default) {
            return false;
        }

        var modifiersList = f.ResolveList(Modifiers);

        if (modifiersList.Count == 0) {
            return false;
        }

        for (int i = modifiersList.Count - 1; i >= 0; i--) {
            AttributeModifier* modifier = modifiersList.GetPointer(i);
            modifier->Tick(f, out bool isOver);

            if (isOver == true) {
                RemoveModifierAt(f, e, i);
            }
        }

        return true;
    }
}
}