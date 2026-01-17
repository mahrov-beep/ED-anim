using Multicast.DirtyDataEditor;
using Multicast.DirtyDataEditor.Parsers;

[assembly: RegisterDirtyDataParser(typeof(DirtyDataParserBigDouble))]
[assembly: RegisterDirtyDataParser(typeof(DirtyDataParserNullableBigDouble))]

namespace Multicast.DirtyDataEditor.Parsers {
    using System;
    using Numerics;

    public class DirtyDataParserBigDouble : DirtyDataParserBase<BigDouble> {
        public override BigDouble Parse(ref DirtyDataStringBuffer input) {
            if (input.Peek() == '"') {
                var str = DirtyDataPrimitivesParsers.ParseString(ref input);
                return BigDouble.Parse(str);
            }

            var num = DirtyDataPrimitivesParsers.ParseDouble(ref input);
            return num;
        }

        public override object CastValue(object value) => value switch {
            int i => BigDouble.Create(i),
            float f => BigDouble.Create(f),
            _ => throw new InvalidCastException($"Cannot cast from '{value?.GetType()}' to '{nameof(BigDouble)}'"),
        };
    }

    public sealed class DirtyDataParserNullableBigDouble : DirtyDataParserNullable<BigDouble> {
    }
}