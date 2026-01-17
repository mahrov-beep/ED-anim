// ReSharper disable RedundantAssignment

namespace Quantum.Prototypes {
  using Photon.Deterministic;
  using Op = HealthAttributeOperation;

  public partial class HealthApplicatorPrototype {
    bool OdinValidateAppliance(HealthAttributeAppliance _, ref string err) => (err = ValidateAppliance()) == null;
    bool OdinValidateOperation(HealthAttributeOperation _, ref string err) => (err = ValidateOperation()) == null;
    bool OdinValidateDamageType(EDamageType _, ref string err)             => (err = ValidateDamageType()) == null;
    bool OdinValidateValue(FP _, ref string err)                           => (err = ValidateValue()) == null;
    bool OdinValidateDuration(FP _, ref string err)                        => (err = ValidateDuration()) == null;

    bool ShowDuration => (HealthAttributeAppliance)Appliance is HealthAttributeAppliance.Continuous or HealthAttributeAppliance.Temporary;
    bool ShowDamageType => (Op)Operation is Op.Damage;
    bool ShowValueIsPercent => (Op)Operation is not (Op.IncreaseMaxHealth or Op.DecreaseMaxHealth);

    string ValidateAppliance() => (HealthAttributeAppliance)Appliance switch {
      HealthAttributeAppliance.None => "Appliance must be not None",
      _ => null,
    };

    string ValidateOperation() => (Op)Operation switch {
      Op.None => "Operation must be not None",
      _ => null,
    };

    string ValidateDamageType() => ((Op)Operation, (EDamageType)DamageType) switch {
      (Op.Damage, EDamageType.None) => "DamageType must be not None",
      _ => null,
    };

    string ValidateValue() => Value switch {
      _ when Value <= FP._0 => "Value must be positive",
      _ when ValueIsPercent && (Value < FP._1 || Value > FP._100) => "Percent value must be in range [1..100]",
      _ => null,
    };

    string ValidateDuration() => Duration switch {
      _ when Duration <= FP._0 => "Duration must be positive",
      _ => null,
    };
  }
}