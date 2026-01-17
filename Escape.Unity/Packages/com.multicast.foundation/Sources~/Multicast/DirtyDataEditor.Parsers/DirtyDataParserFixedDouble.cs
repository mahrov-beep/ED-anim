using Multicast.DirtyDataEditor;
using Multicast.DirtyDataEditor.Parsers;

[assembly: RegisterDirtyDataParser(typeof(DirtyDataParserFixedDouble))]
[assembly: RegisterDirtyDataParser(typeof(DirtyDataParserNullableFixedDouble))]

namespace Multicast.DirtyDataEditor.Parsers {
    using System;
    using Numerics;

    public class DirtyDataParserFixedDouble : DirtyDataParserBase<FixedDouble> {
        public override FixedDouble Parse(ref DirtyDataStringBuffer input) {
            if (input.Peek() == '"') {
                var str = DirtyDataPrimitivesParsers.ParseString(ref input);
                return (FixedDouble) decimal.Parse(str);
            }

            var num = DirtyDataPrimitivesParsers.ParseDecimal(ref input);
            return (FixedDouble) num;
        }

        public override object CastValue(object value) => value switch {
            int i => (FixedDouble) i,
            long l => (FixedDouble) l,
            float f => (FixedDouble) f,
            decimal m => (FixedDouble) m,
            double d => (FixedDouble) d,
            _ => throw new InvalidCastException($"Cannot cast from '{value?.GetType()}' to '{nameof(FixedDouble)}'"),
        };
    }

    public sealed class DirtyDataParserNullableFixedDouble : DirtyDataParserNullable<FixedDouble> {
    }
}