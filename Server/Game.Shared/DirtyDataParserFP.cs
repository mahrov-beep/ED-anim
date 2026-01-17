using System;
using Multicast.DirtyDataEditor;
using Photon.Deterministic;
using Quantum;

[assembly: RegisterDirtyDataParser(typeof(DirtyDataParserFP))]
[assembly: RegisterDirtyDataParser(typeof(DirtyDataParserNullableFP))]

public class DirtyDataParserFP : DirtyDataParserBase<FP> {
    public override FP Parse(ref DirtyDataStringBuffer input) {
        try {
            var str = DirtyDataPrimitivesParsers.ParseString(ref input);
            return FP.FromString(str);
        }
        catch {
            throw new DirtyDataParseException("Failed to parse FPDef");
        }
    }

    public override object CastValue(object value) => value switch {
        int integer => (FP)integer,
        string str => FP.FromString(str),
        _ => throw new InvalidCastException($"Cannot cast from '{value?.GetType()}' to '{nameof(FP)}'"),
    };
}

public sealed class DirtyDataParserNullableFP : DirtyDataParserNullable<FP> {
}