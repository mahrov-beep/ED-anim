namespace Multicast.DirtyDataEditor {
    using System;
    using System.Collections.Generic;

    public static class DirtyDataParsers {
        private static Dictionary<Type, DirtyDataParserBase> parsers;

        public static void Initialize() {
            if (parsers == null) {
                parsers = CreateParsers();
            }
        }

        public static bool TryGetParser(Type valueType, out DirtyDataParserBase parser) {
            Initialize();

            var found = parsers.TryGetValue(valueType, out parser);

            if (!found && valueType.IsEnum) {
                parser = new EnumParser(valueType);

                parsers.Add(valueType, parser);

                found = true;
            }

            if (!found && valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(List<>)) {
                parser = new ListParser(valueType, valueType.GetGenericArguments()[0]);

                parsers.Add(valueType, parser);

                found = true;
            }
            
            if (!found && valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(Dictionary<,>) &&
                valueType.GetGenericArguments()[0] == typeof(string)) {
                parser = new DictParser(valueType, valueType.GetGenericArguments()[1]);

                parsers.Add(valueType, parser);

                found = true;
            }

            return found;
        }

        private static Dictionary<Type, DirtyDataParserBase> CreateParsers() {
            var dict = new Dictionary<Type, DirtyDataParserBase>();

            foreach (var assembly in DirtyDataUtils.DependantAssemblies) {
                var attrs = assembly.GetCustomAttributes(typeof(RegisterDirtyDataParserAttribute), false);

                if (attrs.Length == 0) {
                    continue;
                }

                foreach (RegisterDirtyDataParserAttribute attr in attrs) {
                    var valueParserInstance = (DirtyDataParserBase) DirtyDataTypeActivator.CreateInstance(attr.ParserType);
                    if (valueParserInstance == null) {
                        throw new Exception($"Failed to create instance on parser: {attr.ParserType.Name}");
                    }

                    if (valueParserInstance.ValueType == null) {
                        throw new Exception($"Failed to add instance on parser: {attr.ParserType.Name}");
                    }
                    
                    dict.Add(valueParserInstance.ValueType, valueParserInstance);

                    var listParserInstance = valueParserInstance.CreateListParser();
                    if (listParserInstance == null) {
                        throw new Exception($"Failed to create instance on list parser: {attr.ParserType.Name}");
                    }

                    if (listParserInstance.ValueType == null) {
                        throw new Exception($"Failed to add instance on list parser: {attr.ParserType.Name}");
                    }

                    dict.Add(listParserInstance.ValueType, listParserInstance);

                    var dictParserInstance = valueParserInstance.CreateDictParser();
                    if (dictParserInstance == null) {
                        throw new Exception($"Failed to create instance on dict parser: {attr.ParserType.Name}");
                    }
                    
                    if (dictParserInstance.ValueType == null) {
                        throw new Exception($"Failed to add instance on dict parser: {attr.ParserType.Name}");
                    }

                    dict.Add(dictParserInstance.ValueType, dictParserInstance);
                }
            }

            return dict;
        }
    }
}