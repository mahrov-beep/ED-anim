namespace Multicast.DirtyDataEditor {
    using System;
    using System.Collections.Generic;

    public abstract class DirtyDataParserBase {
        public abstract Type ValueType { get; }

        public abstract object ParseAsObject(ref DirtyDataStringBuffer input);

        internal abstract DirtyDataParserBase CreateListParser();
        internal abstract DirtyDataParserBase CreateDictParser();

        public virtual object CastValue(object value) {
            return value != null ? Convert.ChangeType(value, this.ValueType) : null;
        }
    }

    public abstract class DirtyDataParserBase<T> : DirtyDataParserBase {
        public override Type ValueType => typeof(T);

        public sealed override object ParseAsObject(ref DirtyDataStringBuffer input) {
            return this.Parse(ref input);
        }

        public abstract T Parse(ref DirtyDataStringBuffer input);

        public override object CastValue(object value) => value switch {
            null => null,
            T typedValue => typedValue,
            _ => base.CastValue(value),
        };

        internal sealed override DirtyDataParserBase CreateListParser() {
            return new ListParser(typeof(List<T>), typeof(T));
        }

        internal override DirtyDataParserBase CreateDictParser() {
            return new DictParser(typeof(Dictionary<string, T>), typeof(T));
        }
    }

    public abstract class DirtyDataParserNullable<T> : DirtyDataParserBase<T?>
        where T : struct {
        public override T? Parse(ref DirtyDataStringBuffer input) {
            if (!DirtyDataParsers.TryGetParser(typeof(T), out var parser)) {
                throw new DirtyDataParseException($"Failed to parse '{this.ValueType.Name}' type because parser for type '{typeof(T).Name}' not found");
            }

            return (T) parser.ParseAsObject(ref input);
        }

        public override object CastValue(object value) {
            if (!DirtyDataParsers.TryGetParser(typeof(T), out var parser)) {
                throw new DirtyDataParseException($"Failed to cast '{this.ValueType.Name}' type because parser for type '{typeof(T).Name}' not found");
            }

            return value switch {
                null => default(T?),
                _ => parser.CastValue(value),
            };
        }
    }

    internal sealed class ListParser : DirtyDataParserBase {
        private readonly Type elementType;

        public ListParser(Type listType, Type elementType) {
            this.elementType = elementType;
            this.ValueType   = listType;
        }

        public override Type ValueType { get; }

        public override object ParseAsObject(ref DirtyDataStringBuffer input) {
            return DirtyDataPrimitivesParsers.ParseList(ref input, this.ValueType, this.elementType);
        }

        public override object CastValue(object value) {
            if (value is string strValue && strValue == DDE.Empty) {
                return DirtyDataTypeActivator.CreateInstance(this.ValueType);
            }

            return base.CastValue(value);
        }

        internal override DirtyDataParserBase CreateListParser() {
            throw new InvalidOperationException("Cannot create list of list parser");
        }

        internal override DirtyDataParserBase CreateDictParser() {
            throw new InvalidOperationException("Cannot create dict of list parser");
        }
    }

    internal sealed class DictParser : DirtyDataParserBase {
        private readonly Type elementType;

        public DictParser(Type listType, Type elementType) {
            this.elementType = elementType;
            this.ValueType   = listType;
        }

        public override Type ValueType { get; }

        public override object ParseAsObject(ref DirtyDataStringBuffer input) {
            return DirtyDataPrimitivesParsers.ParseDict(ref input, this.ValueType, this.elementType);
        }

        public override object CastValue(object value) {
            if (value is string strValue && strValue == DDE.Empty) {
                return DirtyDataTypeActivator.CreateInstance(this.ValueType);
            }

            return base.CastValue(value);
        }

        internal override DirtyDataParserBase CreateListParser() {
            throw new InvalidOperationException("Cannot create list of dict parser");
        }

        internal override DirtyDataParserBase CreateDictParser() {
            throw new InvalidOperationException("Cannot create dict of dict parser");
        }
    }

    internal sealed class EnumParser : DirtyDataParserBase {
        public EnumParser(Type valueType) {
            this.ValueType = valueType;
        }

        public override Type ValueType { get; }


        public override object ParseAsObject(ref DirtyDataStringBuffer input) {
            var value = DirtyDataPrimitivesParsers.ParseString(ref input);
            return Enum.Parse(ValueType, value);
        }

        internal override DirtyDataParserBase CreateListParser() {
            throw new InvalidOperationException("Cannot create list of enum parser");
        }

        internal override DirtyDataParserBase CreateDictParser() {
            throw new InvalidOperationException("Cannot create dict of enum parser");
        }
    }
}