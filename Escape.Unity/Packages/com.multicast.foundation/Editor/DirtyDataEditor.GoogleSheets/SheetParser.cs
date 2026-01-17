namespace Multicast.DirtyDataEditor.GoogleSheets {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using UnityEngine;

    public static class SheetParser {
        public static List<Dictionary<string, object>> Parse(
            string sheetName,
            List<string[]> headers,
            List<string> types,
            List<List<string>> valuesList
        ) {
            if (sheetName == null) {
                throw new InvalidOperationException("Sheet name is null");
            }

            foreach (var headerParts in headers) {
                if (headerParts == null || headerParts.Length == 0) {
                    throw new InvalidOperationException($"One of headers is invalid in '{sheetName}'");
                }
            }

            foreach (var type in types) {
                if (string.IsNullOrWhiteSpace(type)) {
                    throw new InvalidOperationException($"One of types is invalid in '{sheetName}'");
                }
            }

            var resultList = new List<Dictionary<string, object>>();

            foreach (var itemValues in valuesList.GroupBy(line => line[0])) {
                var writeTargets = new object[headers.Count];
                var results      = new Dictionary<string, object>();

                object debugItemName = "";

                foreach (var values in itemValues) {
                    if (values.All(string.IsNullOrEmpty)) {
                        break;
                    }

                    for (var i = 0; i < values.Count; i++) {
                        var header      = headers[i];
                        var valueString = values[i];
                        var type        = types[i];
                        var writeTarget = (Dictionary<string, object>) writeTargets[i];

                        if (header[0].StartsWith("#")) {
                            continue;
                        }

                        if (type == "#") {
                            continue;
                        }

                        if (i == 0 && valueString.StartsWith("#")) {
                            goto SKIP_ITEM;
                        }

                        if (string.IsNullOrEmpty(valueString)) {
                            continue;
                        }

                        if (writeTarget == null) {
                            writeTargets[i] = writeTarget = GenerateWriteTarget(results, header);
                        }

                        object value = null;
                        try {
                            if (type.StartsWith("float")) {
                                value = float.Parse(valueString, NumberStyles.Any, CultureInfo.InvariantCulture);
                            }
                            else if (type.StartsWith("int")) {
                                value = int.Parse(valueString, NumberStyles.Any, CultureInfo.InvariantCulture);
                            }
                            else if (type.StartsWith("bool")) {
                                value = bool.Parse(valueString);
                            }
                            else if (type.StartsWith("json")) {
                                value = Json.Deserialize(valueString);
                            }
                            else if (type.StartsWith("string")) {
                                value = valueString;
                            }
                            else {
                                throw new Exception($"[DDE] Unexpected type '{type}' at '{values[0]}/{string.Join(':', header)}'");
                            }
                        }
                        catch (Exception ex) {
                            Debug.LogException(ex);
                        }

                        if (value == null) {
                            throw new Exception(
                                $"[DDE] Value at '{values[0]}/{string.Join(':', header)}' at '{sheetName}/{debugItemName}' is NULL. " +
                                $"Please check that value is correct" +
                                $"\n\n{valueString}\n");
                        }

                        var key = header.Last();

                        if (type.EndsWith("[]")) {
                            if (writeTarget.TryGetValue(key, out var item) && item is IList list) {
                                list.Add(value);
                            }
                            else {
                                writeTarget[key] = new List<object> {value};
                            }
                        }
                        else {
                            if (key != "key" && writeTarget.ContainsKey(key)) {
                                Debug.LogError($"Broken value for '{string.Join(':', header)}' at '{sheetName}/{debugItemName}'");
                            }

                            writeTarget[key] = value;
                        }

                        if (key == "key") {
                            debugItemName = value;
                        }
                    }
                }

                if (results.Count > 0) {
                    resultList.Add(results);
                }

                SKIP_ITEM: ;
            }

            return resultList;
        }

        private static Dictionary<string, object> GenerateWriteTarget(
            Dictionary<string, object> result, string[] header) {
            var writeTarget = result;

            for (var headerPartIndex = 0; headerPartIndex < header.Length - 1; headerPartIndex++) {
                var headerPart = header[headerPartIndex];

                if (!writeTarget.TryGetValue(headerPart, out var it)) {
                    writeTarget[headerPart] = it = new Dictionary<string, object>();
                }

                writeTarget = (Dictionary<string, object>) it;
            }

            return writeTarget;
        }
    }
}