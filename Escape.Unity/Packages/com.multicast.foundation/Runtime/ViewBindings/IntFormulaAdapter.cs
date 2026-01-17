namespace Multicast {
    using System;
    using CodeWriter.ViewBinding;
    using ExpressionParser;
    using JetBrains.Annotations;
    using TriInspector;
    using UniMob;
    using UnityEngine;

    [DrawWithTriInspector]
    [AddComponentMenu("View Binding/Adapters/[Binding] Int Formula Adapter")]
    public class IntFormulaAdapter : SingleResultAdapterBase<int, ViewVariableInt> {
        [SerializeField]
        [ValidateInput(nameof(ValidateExpression))]
        private string expression;

        [Required]
        [SerializeField]
        [ViewContextCollection]
        [OnValueChanged(nameof(ResetContext))]
        private ViewContextBase[] extraContexts = Array.Empty<ViewContextBase>();

        [NonSerialized] private FormulaInt          formula;
        [NonSerialized] private FormulaContext<int> context;
        [NonSerialized] private int                 lastValue;

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

        protected override int Adapt() {
            using (Atom.NoWatch) {
                if (this.context == null) {
                    if (this.lifetimeController == null) {
                        this.lifetimeController = new LifetimeController();
                    }

                    this.context = new FormulaContext<int>(this.lifetimeController.Lifetime);

                    var numerator = new ViewContextArrayVariablesEnumerator(this.extraContexts);

                    while (numerator.TryGetNextVariable(out var variable)) {
                        switch (variable) {
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
                    this.formula = new FormulaInt(this.expression);
                }
            }

            return this.formula.Calc(this.context);
        }

        private void ResetContext() {
            this.context = null;
        }
    }
}