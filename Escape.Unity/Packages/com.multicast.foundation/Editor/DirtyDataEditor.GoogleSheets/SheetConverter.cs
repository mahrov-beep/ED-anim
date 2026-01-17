namespace Multicast.DirtyDataEditor.GoogleSheets {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Google.Apis.Sheets.v4.Data;
    using UnityEditor;

    public static class SheetConverter {
        public static void Convert(Spreadsheet spreadsheet, string outputFolder, Action<string, float> progress) {
            for (int sheetIndex = 0, sheetCount = spreadsheet.Sheets.Count; sheetIndex < sheetCount; sheetIndex++) {
                var sheet = spreadsheet.Sheets[sheetIndex];
                if (sheet.Properties.Hidden == true) {
                    continue;
                }

                progress?.Invoke(sheet.Properties.Title, 1f * sheetIndex / sheetCount);

                var headers = sheet.Data[0].RowData[0].Values
                    .Select(it => it.FormattedValue)
                    .Where(it => !string.IsNullOrEmpty(it))
                    .Select(it => Array.ConvertAll(it.Split(':'), p => p.Trim().Replace(' ', '_')))
                    .ToList();

                var types = sheet.Data[0].RowData[1].Values
                    .Take(headers.Count)
                    .Select(it => it.FormattedValue)
                    .ToList();

                var valuesList = new List<List<string>>();

                foreach (var row in sheet.Data[0].RowData.Skip(2)) {
                    if (row.Values == null) {
                        continue;
                    }

                    var values = row.Values.Take(headers.Count).Select(it => it.FormattedValue).ToList();

                    valuesList.Add(values);
                }

                var name = sheet.Properties.Title
                    .Replace(' ', '_')
                    .Replace('/', '$');

                var outputFilePath = Path.Combine(outputFolder, name + ".yaml");
                var resultList     = SheetParser.Parse(name, headers, types, valuesList);

                using (var file = new StreamWriter(outputFilePath)) {
                    foreach (var results in resultList) {
                        file.WriteLine("---");
                        foreach (var (key, value) in results) {
                            file.Write(key);
                            file.Write(": ");
                            file.WriteLine(Json.Serialize(value));
                        }
                    }
                }

                AssetDatabase.ImportAsset(outputFilePath);
            }
        }
    }
}