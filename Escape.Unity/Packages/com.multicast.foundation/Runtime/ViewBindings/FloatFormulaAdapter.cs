namespace Multicast {
    using System;
    using CodeWriter.ViewBinding;
    using ExpressionParser;
    using JetBrains.Annotations;
    using TriInspector;
    using UniMob;
    using UnityEngine;

    [DrawWithTriInspector]
    [AddComponentMenu("View Binding/Adapters/[Binding] Float Formula Adapter")]
    public class FloatFormulaAdapter : SingleResultAdapterBase<float, ViewVariableFloat> {
        [SerializeField]
        [ValidateInput(nameof(ValidateExpression))]
        private string expression;

        [Required]
        [SerializeField]
        [ViewContextCollection]
        [OnValueChanged(nameof(ResetContext))]
        private ViewContextBase[] extraContexts = Array.Empty<ViewContextBase>();

        [NonSerialized] private FormulaFloat          formula;
        [NonSerialized] private FormulaContext<float> context;
        [NonSerialized] private float                 lastValue;

        [NonSerialized, CanBeNull] private LifetimeController lifetimeController;

        protected virtual void OnDestroy() {
            this.lifetimeController?.Dispose();
        }

        private TriValidationResult ValidateExpression() {
            try {
                this.Adapt();

                return TriValidationResult.Valid;
            }
            catch (Exception ex) {
                return TriValidationResult.Error(ex.Message);
            }
        }

        protected override float Adapt() {
            using (Atom.NoWatch) {
                if (this.context == null) {
                    if (this.lifetimeController == null) {
                        this.lifetimeController = new LifetimeController();
                    }

                    this.context = new FormulaContext<float>(this.lifetimeController.Lifetime);

                    var numerator = new ViewContextArrayVariablesEnumerator(this.extraContexts);

                    while (numerator.TryGetNextVariable(out var variable)) {
                        switch (variable) {
                            case ViewVariableFloat variableFloat:
                                this.context.RegisterVariable(variable.Name, () => variableFloat.Value);
                                break;

                            case ViewVariableBigDouble variableBigDouble:
                                this.context.RegisterVariable(variable.Name, () => variableBigDouble.Value.ToFloatOrThrow());
                                break;

                            case ViewVariableInt variableInt:
                                this.context.RegisterVariable(variable.Name, () => variableInt.Value);
                                break;

                            case ViewVariableBool variableBool:
                                this.context.RegisterVariable(variable.Name, () => variableBool.Value ? 1 : 0);
                                break;
                        }
                    }
                }

                if (this.formula == null || this.formula.Expression != this.expression) {
                    this.formula = new FormulaFloat(this.expression);
                }
            }

            return this.formula.Calc(this.context);
        }

        private void ResetContext() {
            this.context = null;
        }
    }
}