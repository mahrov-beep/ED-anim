namespace Multicast.DirtyDataEditor {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using global::Unity.IL2CPP.CompilerServices;
    using JetBrains.Annotations;
    using Multicast.Pool;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public static class DirtyDataParser {
        public static List<string> Errors { get; } = new List<string>();

        public static List<T> ParseList<T>([NotNull] string input) {
            var list = new List<T>();
            ParseListAppend(list, input);
            return list;
        }

        public static void ParseListAppend<T>([NotNull] List<T> list, [NotNull] string input) {
            if (list == null) {
                throw new ArgumentNullException(nameof(list));
            }

            if (input == null) {
                throw new ArgumentNullException(nameof(input));
            }


            var buffer = new DirtyDataStringBuffer(input);

            buffer.SkipWhiteSpaces();

            var isYaml = input.Length == 0 || buffer.Peek() == '-';

            if (isYaml) {
                ParseYamlList(list, ref buffer, typeof(T));
            }
            else {
                DirtyDataPrimitivesParsers.ParseList(list, ref buffer, typeof(T));
            }
        }

        public static T Parse<T>([NotNull] string input) {
            if (input == null) {
                throw new ArgumentNullException(nameof(input));
            }

            var buffer = new DirtyDataStringBuffer(input);

            buffer.SkipWhiteSpaces();

            return (T)DirtyDataPrimitivesParsers.ParseValue(ref buffer, typeof(T));
        }

        private static IList ParseYamlList(IList list, ref DirtyDataStringBuffer input, Type elementType) {
            while (!input.AtEnd()) {
                input.SkipWhiteSpaces();
                var line = input.ReadLine();
                line.ReadString("---");

                var value = ParseYamlObject(ref input, elementType);
                list.Add(value);
            }

            return list;

            static object ParseYamlObject(ref DirtyDataStringBuffer input, Type type) {
                var typeDefinition = DirtyDataTypeModel.GetTypeDefinitionCached(type);

                if (typeDefinition.IsPolymorphic) {
                    throw new DirtyDataParseException($"Yaml must not contains root level polymorphic types '{typeDefinition.Name}'");
                }

                var instance = typeDefinition.CreateInstance();

                using (ListPool<long>.Get(out var tempProperties)) {
                    while (!input.AtEnd()) {
                        if (input.Peek() == '-') {
                            break;
                        }

                        var lineInput = input.ReadLine();

                        ParseYamlObjectProperty(ref lineInput, type, typeDefinition, instance, tempProperties);
                    }

                    FinishInstance(instance, typeDefinition, tempProperties);
                }

                return instance;
            }

            static void ParseYamlObjectProperty(ref DirtyDataStringBuffer input, Type type,
                DirtyDataTypeDefinition typeDefinition, object instance, List<long> properties) {
                input.SkipWhiteSpaces();

                var nameSpan = input.ReadWhile(DirtyDataPrimitivesParsers.IsValidKeyChar).Span;
                var nameHash = DirtyDataUtils.GetHash(nameSpan);

                if (input.AtEnd()) {
                    // empty line
                    return;
                }

                if (input.Peek() == '#') {
                    // comment
                    input.ReadChar('#');
                    return;
                }

                input.ReadChar(':');
                input.SkipWhiteSpaces();

                if (!typeDefinition.TryGetProperty(nameHash, out var property)) {
                    Errors.Add($"Unknown property '{nameSpan.ToString()}' in '{typeDefinition.Name}'");
                    return;
                }

                object value;
                try {
                    value = DirtyDataPrimitivesParsers.ParseValue(ref input, property.Type);
                }
                catch (Exception ex) {
                    throw new DirtyDataParseException($"Failed to parse '{nameSpan.ToString()}({property.Type.GetNiceName()})' property of '{type.Name}'", ex, input.ToString());
                }

                property.SetValue(instance, value);
                properties.Add(nameHash);
            }
        }

        internal static void FinishInstance(object instance, DirtyDataTypeDefinition typeDefinition, List<long> filledProperties) {
            foreach (var property in typeDefinition.Properties) {
                if (property.Optional) {
                    if (!filledProperties.Contains(property.NameHash)) {
                        var value = DirtyDataParsers.TryGetParser(property.Type, out var parser)
                            ? parser.CastValue(property.DefaultValue)
                            : property.DefaultValue;
                        property.SetValue(instance, value);
                    }

                    continue;
                }

                if (!filledProperties.Contains(property.NameHash)) {
                    Errors.Add($"Missing required property '{property.Name}' in '{typeDefinition.Name}'");
                }
            }
        }
    }
}