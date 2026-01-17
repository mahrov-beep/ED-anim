namespace Multicast.ServerData {
    using System;
    using System.Runtime.CompilerServices;
    using MessagePack;

    public class SdEnumValue<T> : SdValue<T> where T : Enum {
        public SdEnumValue(SdArgs args, T initialValue) : base(args, initialValue) {
        }

        protected override void SerializeValue(ref MessagePackWriter writer, MessagePackSerializerOptions options, T value) {
            SdEnumFormatter<T>.Instance.Serialize(ref writer, options, value);
        }

        protected override T DeserializeValue(ref MessagePackReader reader, MessagePackSerializerOptions options) {
            return SdEnumFormatter<T>.Instance.Deserialize(ref reader, options);
        }

        public static implicit operator SdEnumValue<T>(SdArgs args) {
            return new SdEnumValue<T>(args, SdValueDefaults<T>.DefaultValue);
        }
    }

    internal abstract class SdEnumFormatter<TEnum> where TEnum : Enum {
        public static readonly SdEnumFormatter<TEnum> Instance;

        static SdEnumFormatter() {
            var underlyingType = Enum.GetUnderlyingType(typeof(TEnum));

            Instance = underlyingType switch {
                _ when underlyingType == typeof(byte) => new SdEnumFormatter<TEnum, byte>(),
                _ when underlyingType == typeof(short) => new SdEnumFormatter<TEnum, short>(),
                _ when underlyingType == typeof(int) => new SdEnumFormatter<TEnum, int>(),
                _ when underlyingType == typeof(long) => new SdEnumFormatter<TEnum, long>(),

                _ when underlyingType == typeof(sbyte) => new SdEnumFormatter<TEnum, sbyte>(),
                _ when underlyingType == typeof(ushort) => new SdEnumFormatter<TEnum, ushort>(),
                _ when underlyingType == typeof(uint) => new SdEnumFormatter<TEnum, uint>(),
                _ when underlyingType == typeof(ulong) => new SdEnumFormatter<TEnum, ulong>(),

                _ => throw new ArgumentException($"Enum '{typeof(TEnum).Name}' has unsupported underlying type '{underlyingType.Name}'"),
            };
        }

        public abstract void  Serialize(ref MessagePackWriter writer, MessagePackSerializerOptions options, TEnum value);
        public abstract TEnum Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options);
    }

    internal sealed class SdEnumFormatter<TEnum, TUnderlying> : SdEnumFormatter<TEnum>
        where TEnum : Enum
        where TUnderlying : unmanaged {
        public override void Serialize(ref MessagePackWriter writer, MessagePackSerializerOptions options, TEnum value) {
            var intValue = Unsafe.As<TEnum, TUnderlying>(ref value);
            options.Resolver.GetFormatterWithVerify<TUnderlying>().Serialize(ref writer, intValue, options);
        }

        public override TEnum Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
            var intValue = options.Resolver.GetFormatterWithVerify<TUnderlying>().Deserialize(ref reader, options);
            return Unsafe.As<TUnderlying, TEnum>(ref intValue);
        }
    }
}