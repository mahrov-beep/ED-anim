using Multicast.DirtyDataEditor;

// Values
[assembly: RegisterDirtyDataParser(typeof(DirtyDataParserBool))]
[assembly: RegisterDirtyDataParser(typeof(DirtyDataParserInt))]
[assembly: RegisterDirtyDataParser(typeof(DirtyDataParserLong))]
[assembly: RegisterDirtyDataParser(typeof(DirtyDataParserFloat))]
[assembly: RegisterDirtyDataParser(typeof(DirtyDataParserDouble))]
[assembly: RegisterDirtyDataParser(typeof(DirtyDataParserString))]

// Nullables
[assembly: RegisterDirtyDataParser(typeof(DirtyDataParserNullableBool))]
[assembly: RegisterDirtyDataParser(typeof(DirtyDataParserNullableInt))]
[assembly: RegisterDirtyDataParser(typeof(DirtyDataParserNullableLong))]
[assembly: RegisterDirtyDataParser(typeof(DirtyDataParserNullableFloat))]
[assembly: RegisterDirtyDataParser(typeof(DirtyDataParserNullableDouble))]

namespace Multicast.DirtyDataEditor {
    // Values

    public sealed class DirtyDataParserBool : DirtyDataParserBase<bool> {
        public override bool Parse(ref DirtyDataStringBuffer input) {
            return DirtyDataPrimitivesParsers.ParseBool(ref input);
        }
    }

    public sealed class DirtyDataParserInt : DirtyDataParserBase<int> {
        public override int Parse(ref DirtyDataStringBuffer input) {
            return (int) DirtyDataPrimitivesParsers.ParseLong(ref input);
        }
    }

    public sealed class DirtyDataParserLong : DirtyDataParserBase<long> {
        public override long Parse(ref DirtyDataStringBuffer input) {
            return DirtyDataPrimitivesParsers.ParseLong(ref input);
        }
    }

    public sealed class DirtyDataParserFloat : DirtyDataParserBase<float> {
        public override float Parse(ref DirtyDataStringBuffer input) {
            return (float) DirtyDataPrimitivesParsers.ParseDouble(ref input);
        }
    }

    public sealed class DirtyDataParserDouble : DirtyDataParserBase<double> {
        public override double Parse(ref DirtyDataStringBuffer input) {
            return DirtyDataPrimitivesParsers.ParseDouble(ref input);
        }
    }

    public sealed class DirtyDataParserString : DirtyDataParserBase<string> {
        public override string Parse(ref DirtyDataStringBuffer input) {
            return DirtyDataPrimitivesParsers.ParseString(ref input);
        }
    }

    // Nullables

    public sealed class DirtyDataParserNullableBool : DirtyDataParserNullable<bool> {
    }

    public sealed class DirtyDataParserNullableInt : DirtyDataParserNullable<int> {
    }

    public sealed class DirtyDataParserNullableLong : DirtyDataParserNullable<long> {
    }

    public sealed class DirtyDataParserNullableFloat : DirtyDataParserNullable<float> {
    }

    public sealed class DirtyDataParserNullableDouble : DirtyDataParserNullable<double> {
    }
}