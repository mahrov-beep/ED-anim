using Multicast.DirtyDataEditor;
using Multicast.DirtyDataEditor.Parsers;

[assembly: RegisterDirtyDataParser(typeof(DirtyDataParserBoostTag))]
[assembly: RegisterDirtyDataParser(typeof(DirtyDataParserNullableBoostTag))]

namespace Multicast.DirtyDataEditor.Parsers {
    using System;
    using Boosts;

    public class DirtyDataParserBoostTag : DirtyDataParserBase<BoostTag> {
        public override BoostTag Parse(ref DirtyDataStringBuffer input) {
            var str = DirtyDataPrimitivesParsers.ParseString(ref input);
            return new BoostTag(str);
        }

        public override object CastValue(object value) => value switch {
            string str => new BoostTag(str),
            _ => throw new InvalidCastException($"Cannot cast from '{value?.GetType()}' to '{nameof(BoostTag)}'"),
        };
    }

    public sealed class DirtyDataParserNullableBoostTag : DirtyDataParserNullable<BoostTag> {
    }
}