namespace Multicast.DirtyDataEditor {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using global::Unity.IL2CPP.CompilerServices;
    using Multicast.Pool;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public static class DirtyDataPrimitivesParsers {
        private static readonly ObjectPool<StringBuilder> StringBuilderPool =
            new ObjectPool<StringBuilder>(() => new StringBuilder(), actionOnRelease: sb => sb.Clear(), collectionCheck: false);

        public static object ParseValue(ref DirtyDataStringBuffer input, Type type) {
            var result = DirtyDataParsers.TryGetParser(type, out var parser)
                ? parser.ParseAsObject(ref input)
                : ParseObject(ref input, type);

            input.SkipWhiteSpaces();

            if (!input.AtEnd() && input.Peek() != ',' && input.Peek() != ']' && input.Peek() != '}') {
                throw new DirtyDataParseException("Invalid property value");
            }

            return result;
        }

        public static object ParseObject(ref DirtyDataStringBuffer input, Type type) {
            var typeDefinition = DirtyDataTypeModel.GetTypeDefinitionCached(type);

            object instance = null;
            using (ListPool<long>.Get(out var tempProperties)) {
                if (!typeDefinition.IsPolymorphic) {
                    instance = typeDefinition.CreateInstance();
                }

                input.SkipWhiteSpaces();
                input.ReadChar('{');

                while (true) {
                    input.SkipWhiteSpaces();

                    if (input.Peek() == '}') {
                        break;
                    }

                    ReadOnlySpan<char> nameSpan;
                    if (input.Peek() == '"') {
                        input.ReadChar('"');
                        nameSpan = input.ReadWhile(c => c != '"').Span;
                        input.ReadChar('"');
                    }
                    else {
                        nameSpan = input.ReadWhile(IsValidKeyChar).Span;
                    }

                    var nameHash = DirtyDataUtils.GetHash(nameSpan);

                    input.SkipWhiteSpaces();
                    input.ReadChar(':');
                    input.SkipWhiteSpaces();

                    if (!typeDefinition.TryGetProperty(nameHash, out var property)) {
                        throw new DirtyDataParseException($"Unknown property '{nameSpan.ToString()}' in '{typeDefinition.Name}'");
                    }

                    object value;
                    try {
                        value = ParseValue(ref input, property.Type);
                    }
                    catch (Exception ex) {
                        throw new DirtyDataParseException($"Failed to parse '{nameSpan.ToString()}({property.Type.GetNiceName()})' property of '{type.Name}'", ex, input.ToString());
                    }

                    if (instance == null) {
                        if (nameHash != typeDefinition.PolymorphicFieldHash) {
                            throw new DirtyDataParseException($"Property '{typeDefinition.PolymorphicField}' must be first in '{typeDefinition.Name}'");
                        }

                        instance = typeDefinition.CreatePolymorphicInstance(value?.ToString(), out var instanceType);

                        typeDefinition = DirtyDataTypeModel.GetTypeDefinitionCached(instanceType);
                    }

                    property.SetValue(instance, value);
                    tempProperties.Add(nameHash);

                    input.SkipWhiteSpaces();

                    if (input.Peek() == '}') {
                        break;
                    }

                    input.ReadChar(',');
                }

                input.SkipWhiteSpaces();
                input.ReadChar('}');

                if (instance == null) {
                    throw new DirtyDataParseException($"'{typeDefinition.PolymorphicField}' property must be set for polymorphic '{typeDefinition.Name}'");
                }

                DirtyDataParser.FinishInstance(instance, typeDefinition, tempProperties);
            }

            return instance;
        }

        public static object ParseDict(ref DirtyDataStringBuffer input, Type dictType, Type elementType) {
            var dict = (IDictionary) DirtyDataTypeActivator.CreateInstance(dictType);

            input.SkipWhiteSpaces();
            input.ReadChar('{');

            while (true) {
                input.SkipWhiteSpaces();

                if (input.Peek() == '}') {
                    break;
                }

                string name;
                if (input.Peek() == '"') {
                    input.ReadChar('"');
                    name = input.ReadWhile(c => c != '"').ToString();
                    input.ReadChar('"');
                }
                else {
                    name = input.ReadWhile(IsValidKeyChar).ToString();
                }

                input.SkipWhiteSpaces();
                input.ReadChar(':');
                input.SkipWhiteSpaces();

                var value = ParseValue(ref input, elementType);

                dict.Add(name, value);

                input.SkipWhiteSpaces();

                if (input.Peek() == '}') {
                    break;
                }

                input.ReadChar(',');
            }

            input.SkipWhiteSpaces();
            input.ReadChar('}');

            return dict;
        }

        public static List<T> ParseList<T>(ref DirtyDataStringBuffer input) {
            var list = new List<T>();
            ParseList(list, ref input, typeof(T));
            return list;
        }

        public static IList ParseList(ref DirtyDataStringBuffer input, Type listType, Type elementType) {
            var list = (IList) DirtyDataTypeActivator.CreateInstance(listType);
            ParseList(list, ref input, elementType);
            return list;
        }

        public static void ParseList(IList list, ref DirtyDataStringBuffer input, Type elementType) {
            input.SkipWhiteSpaces();
            input.ReadChar('[');

            while (true) {
                input.SkipWhiteSpaces();

                if (input.Peek() == ']') {
                    break;
                }

                var value = ParseValue(ref input, elementType);
                list.Add(value);

                input.SkipWhiteSpaces();

                if (input.Peek() == ']') {
                    break;
                }

                input.ReadChar(',');
            }

            input.SkipWhiteSpaces();
            input.ReadChar(']');
        }

        public static bool ParseBool(ref DirtyDataStringBuffer input) {
            if (input.Peek() == 't') {
                input.ReadString("true");
                return true;
            }

            input.ReadString("false");
            return false;
        }

        public static long ParseLong(ref DirtyDataStringBuffer input) {
            var start = input.Ptr;

            if (!input.AtEnd() && input.Peek() == '-') {
                input.ReadChar('-');
            }

            input.ReadWhile(char.IsDigit);

            var longSpan = input.Slice(start, input.Ptr - start);
            return long.Parse(longSpan, NumberStyles.Any, CultureInfo.InvariantCulture);
        }

        public static double ParseDouble(ref DirtyDataStringBuffer input) {
            var start = input.Ptr;

            if (!input.AtEnd() && input.Peek() == '-') {
                input.ReadChar('-');
            }

            input.ReadWhile(char.IsDigit);

            if (!input.AtEnd() && input.Peek() == '.') {
                input.ReadChar('.');
                input.ReadWhile(char.IsDigit);
            }

            if (!input.AtEnd() && input.Peek() == 'E') {
                input.ReadChar('E');

                if (input.Peek() == '+') {
                    input.ReadChar('+');
                    input.ReadWhile(char.IsDigit);
                }
                else {
                    input.ReadChar('-');
                    input.ReadWhile(char.IsDigit);
                }
            }

            var doubleSpan = input.Slice(start, input.Ptr - start);
            return double.Parse(doubleSpan, NumberStyles.Any, CultureInfo.InvariantCulture);
        }
        
        public static decimal ParseDecimal(ref DirtyDataStringBuffer input) {
            var start = input.Ptr;

            if (!input.AtEnd() && input.Peek() == '-') {
                input.ReadChar('-');
            }

            input.ReadWhile(char.IsDigit);

            if (!input.AtEnd() && input.Peek() == '.') {
                input.ReadChar('.');
                input.ReadWhile(char.IsDigit);
            }

            if (!input.AtEnd() && input.Peek() == 'E') {
                input.ReadChar('E');

                if (input.Peek() == '+') {
                    input.ReadChar('+');
                    input.ReadWhile(char.IsDigit);
                }
                else {
                    input.ReadChar('-');
                    input.ReadWhile(char.IsDigit);
                }
            }

            var doubleSpan = input.Slice(start, input.Ptr - start);
            return decimal.Parse(doubleSpan, NumberStyles.Any, CultureInfo.InvariantCulture);
        }

        public static string ParseString(ref DirtyDataStringBuffer input) {
            using (StringBuilderPool.Get(out var s)) {
                input.ReadChar('"');

                while (!input.AtEnd()) {
                    var c = input.ReadAnyChar();
                    if (c == '"') {
                        break;
                    }

                    switch (c) {
                        case '\\':
                            if (input.AtEnd()) {
                                throw new DirtyDataParseException("Invalid \\ symbol at end");
                            }

                            c = input.ReadAnyChar();

                            switch (c) {
                                case '"':
                                case '\\':
                                case '/':
                                    s.Append(c);
                                    break;
                                case 'b':
                                    s.Append('\b');
                                    break;
                                case 'f':
                                    s.Append('\f');
                                    break;
                                case 'n':
                                    s.Append('\n');
                                    break;
                                case 'r':
                                    s.Append('\r');
                                    break;
                                case 't':
                                    s.Append('\t');
                                    break;
                                case 'u':
                                    var hex = new char[4];

                                    for (var i = 0; i < 4; i++) {
                                        hex[i] = input.Peek();
                                        input.ReadAnyChar();

                                        if ("0123456789ABCDEFabcdef".IndexOf(hex[i]) == -1) {
                                            throw new DirtyDataParseException("Invalid \\u escaped symbol");
                                        }
                                    }

                                    s.Append((char) Convert.ToInt32(new string(hex), 16));
                                    break;
                            }

                            break;

                        default:
                            s.Append(c);
                            break;
                    }
                }

                return s.ToString();
            }
        }

        public static bool IsValidKeyChar(char c) {
            return c == '_' || c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z' || c >= '0' && c <= '9';
        }
    }
}