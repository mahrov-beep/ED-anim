using Multicast.DirtyDataEditor;
using Multicast.DirtyDataEditor.Parsers;

[assembly: RegisterDirtyDataParser(typeof(DirtyDataParserFormulaFloat))]
[assembly: RegisterDirtyDataParser(typeof(DirtyDataParserFormulaBigDouble))]
[assembly: RegisterDirtyDataParser(typeof(DirtyDataParserFormulaInt))]
[assembly: RegisterDirtyDataParser(typeof(DirtyDataParserFormulaPredicate))]

namespace Multicast.DirtyDataEditor.Parsers {
    using ExpressionParser;

    public abstract class DirtyDataParserFormulaBase<TFormula> : DirtyDataParserBase<TFormula> {
        public override TFormula Parse(ref DirtyDataStringBuffer input) {
            var str = DirtyDataPrimitivesParsers.ParseString(ref input);
            return this.ParseFormula(str);
        }

        protected abstract TFormula ParseFormula(string value);
    }

    public class DirtyDataParserFormulaFloat : DirtyDataParserFormulaBase<FormulaFloat> {
        protected override FormulaFloat ParseFormula(string value) => new FormulaFloat(value);
    }

    public class DirtyDataParserFormulaBigDouble : DirtyDataParserFormulaBase<FormulaBigDouble> {
        protected override FormulaBigDouble ParseFormula(string value) => new FormulaBigDouble(value);
    }

    public class DirtyDataParserFormulaInt : DirtyDataParserFormulaBase<FormulaInt> {
        protected override FormulaInt ParseFormula(string value) => new FormulaInt(value);
    }

    public class DirtyDataParserFormulaPredicate : DirtyDataParserFormulaBase<FormulaPredicate> {
        protected override FormulaPredicate ParseFormula(string value) => new FormulaPredicate(value);
    }
}