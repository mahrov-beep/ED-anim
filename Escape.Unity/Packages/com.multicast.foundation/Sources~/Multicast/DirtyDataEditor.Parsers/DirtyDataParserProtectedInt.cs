using Multicast.DirtyDataEditor;
using Multicast.DirtyDataEditor.Parsers;

[assembly: RegisterDirtyDataParser(typeof(DirtyDataParserProtectedInt))]
[assembly: RegisterDirtyDataParser(typeof(DirtyDataParserNullableProtectedInt))]

namespace Multicast.DirtyDataEditor.Parsers {
    using System;
    using Numerics;

    public class DirtyDataParserProtectedInt : DirtyDataParserBase<ProtectedInt> {
        public override ProtectedInt Parse(ref DirtyDataStringBuffer input) {
            var num = (int)DirtyDataPrimitivesParsers.ParseLong(ref input);
            return num;
        }

        public override object CastValue(object value) => value switch {
            int i => new ProtectedInt(i),
            _ => throw new InvalidCastException($"Cannot cast from '{value?.GetType()}' to '{nameof(ProtectedInt)}'"),
        };
    }

    public sealed class DirtyDataParserNullableProtectedInt : DirtyDataParserNullable<ProtectedInt> {
    }
}