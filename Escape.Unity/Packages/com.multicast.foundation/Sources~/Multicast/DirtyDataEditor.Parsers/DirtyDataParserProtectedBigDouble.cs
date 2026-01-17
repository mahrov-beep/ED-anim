using Multicast.DirtyDataEditor;
using Multicast.DirtyDataEditor.Parsers;

[assembly: RegisterDirtyDataParser(typeof(DirtyDataParserProtectedBigDouble))]
[assembly: RegisterDirtyDataParser(typeof(DirtyDataParserNullableProtectedBigDouble))]

namespace Multicast.DirtyDataEditor.Parsers {
    using System;
    using Numerics;

    public class DirtyDataParserProtectedBigDouble : DirtyDataParserBase<ProtectedBigDouble> {
        public override ProtectedBigDouble Parse(ref DirtyDataStringBuffer input) {
            if (input.Peek() == '"') {
                var str = DirtyDataPrimitivesParsers.ParseString(ref input);
                return BigDouble.Parse(str);
            }

            var num = DirtyDataPrimitivesParsers.ParseDouble(ref input);
            return (BigDouble)num;
        }

        public override object CastValue(object value) => value switch {
            int i => BigDouble.Create(i),
            float f => BigDouble.Create(f),
            _ => throw new InvalidCastException($"Cannot cast from '{value?.GetType()}' to '{nameof(ProtectedBigDouble)}'"),
        };
    }

    public sealed class DirtyDataParserNullableProtectedBigDouble : DirtyDataParserNullable<ProtectedBigDouble> {
    }
}