using Multicast.DirtyDataEditor;
using Multicast.DirtyDataEditor.Parsers;

[assembly: RegisterDirtyDataParser(typeof(DirtyDataParserGameTime))]
[assembly: RegisterDirtyDataParser(typeof(DirtyDataParserNullableGameTime))]

namespace Multicast.DirtyDataEditor.Parsers {
    using System;
    using Numerics;

    public class DirtyDataParserGameTime : DirtyDataParserBase<GameTime> {
        public override GameTime Parse(ref DirtyDataStringBuffer input) {
            var str = DirtyDataPrimitivesParsers.ParseString(ref input);

            if (!GameTime.TryParse(str, out var gt)) {
                throw new DirtyDataParseException("Failed to parse GameTime");
            }

            return gt;
        }

        public override object CastValue(object value) => value switch {
            _ => throw new InvalidCastException($"Cannot cast from '{value?.GetType()}' to '{nameof(ProtectedInt)}'"),
        };
    }

    public sealed class DirtyDataParserNullableGameTime : DirtyDataParserNullable<GameTime> {
    }
}