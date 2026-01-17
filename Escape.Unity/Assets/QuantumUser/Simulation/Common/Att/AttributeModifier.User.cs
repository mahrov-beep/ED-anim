
namespace Quantum {

using Photon.Deterministic;

public partial struct AttributeModifier {
    public void Init(Frame f) {
        Timer = Duration;
    }

    public void Tick(Frame f, out bool ttlOver) {
        if (Timer > 0 || ModifierAppliance == EModifierAppliance.OneTime) {
            Timer -= f.DeltaTime;

            if (Timer <= 0) {
                ttlOver = true;
                return;
            }
        }

        ttlOver = false;
        return;
    }

    public void Apply(Frame f, EntityRef e, ref FP attributeValue) {
        FP valueToApply = ModifierAppliance == EModifierAppliance.Continuous ? Amount * f.DeltaTime : Amount;

        if (ModifierOperation == EModifierOperation.None) {
            valueToApply = 0;
        }
        else if (ModifierOperation == EModifierOperation.Subtract) {
            valueToApply *= -1;
        }

        if (AmountMultiplier != EAttributeType.None) {
            valueToApply *= AttributesHelper.GetCurrentValue(f, e, AmountMultiplier);
        }

        attributeValue    += valueToApply;
        TotalAppliedValue += valueToApply;
    }

    public void DeApply(Frame f, EntityRef e, ref FP attributeValue) {
        attributeValue    -= TotalAppliedValue;
        TotalAppliedValue =  0;
    }
}
}