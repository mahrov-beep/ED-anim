using Multicast.DirtyDataEditor;
using Multicast.DirtyDataEditor.Parsers;

[assembly: RegisterDirtyDataParser(typeof(DirtyDataParserFormulaFloatList))]
[assembly: RegisterDirtyDataParser(typeof(DirtyDataParserFormulaBigDoubleList))]
[assembly: RegisterDirtyDataParser(typeof(DirtyDataParserFormulaFloatList))]
[assembly: RegisterDirtyDataParser(typeof(DirtyDataParserFormulaIntList))]

namespace Multicast.DirtyDataEditor.Parsers {
    using System.Collections.Generic;
    using ExpressionParser;

    public abstract class DirtyDataParserFormulaBaseList<TFormula> : DirtyDataParserBase<TFormula> {
        public override TFormula Parse(ref DirtyDataStringBuffer input) {
            var value = input.Peek() == '['
                ? DirtyDataPrimitivesParsers.ParseList<string>(ref input)
                : new List<string> {DirtyDataPrimitivesParsers.ParseString(ref input)};

            return this.ParseFormula(value);
        }

        protected abstract TFormula ParseFormula(List<string> value);
    }

    public class DirtyDataParserFormulaFloatList : DirtyDataParserFormulaBaseList<FormulaFloatList> {
        protected override FormulaFloatList ParseFormula(List<string> value) => new FormulaFloatList(value);
    }

    public class DirtyDataParserFormulaBigDoubleList : DirtyDataParserFormulaBaseList<FormulaBigDoubleList> {
        protected override FormulaBigDoubleList ParseFormula(List<string> value) => new FormulaBigDoubleList(value);
    }

    public class DirtyDataParserFormulaIntList : DirtyDataParserFormulaBaseList<FormulaIntList> {
        protected override FormulaIntList ParseFormula(List<string> value) => new FormulaIntList(value);
    }
}