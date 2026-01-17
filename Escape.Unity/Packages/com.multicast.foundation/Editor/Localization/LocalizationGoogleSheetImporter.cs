namespace Multicast.Localization {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using DirtyDataEditor;
    using DirtyDataEditor.GoogleSheets;
    using Google.Apis.Auth.OAuth2;
    using Google.Apis.Services;
    using Google.Apis.Sheets.v4;
    using Google.Apis.Sheets.v4.Data;
    using JetBrains.Annotations;
    using UnityEditor;
    using UnityEngine;

    internal static class LocalizationGoogleSheetImporter {
        [MenuItem("Localization/Import from GoogleSheet", true, priority = 0)]
        public static bool CanImportFromGoogleSheet() {
            return DirtyDataEditorSettings.Instance != null &&
                   DirtyDataEditorSettings.Instance.enabled;
        }

        [MenuItem("Localization/Import from GoogleSheet", priority = 0)]
        public static void ImportFromGoogleSheet() {
            DirtyDataEditorSettings.Instance.ImportLocalizationFromGoogleSheets();
        }

        [InitializeOnLoadMethod]
        private static void Setup() {
            DirtyDataEditorSettings.ImportLocalizationCallback = Import;
        }

        public static void Import(DirtyDataEditorSettings settings) {
            Import(settings, null);
        }

        public static void Import(DirtyDataEditorSettings settings,
            [CanBeNull] Dictionary<string, List<string>> sheetsBySpreadsheetIdFilter) {
            try {
                EditorUtility.DisplayProgressBar("Localization Import", "", 0f);
                AssetDatabase.StartAssetEditing();

                var credential   = CredentialsProvider.ReadCredentials(settings);
                var outputFolder = settings.OutputLocalizationPath;

                var initializer = new BaseClientService.Initializer() {
                    ApplicationName       = "Config Parser",
                    HttpClientInitializer = credential,
                };

                if (!Directory.Exists(outputFolder)) {
                    Directory.CreateDirectory(outputFolder);
                }

                foreach (var sheetId in settings.localizationGoogleSheetIds) {
                    var service = new SheetsService(initializer);
                    var request = service.Spreadsheets.Get(sheetId.googleSheetId);

                    if (sheetsBySpreadsheetIdFilter != null) {
                        if (sheetsBySpreadsheetIdFilter.TryGetValue(sheetId.googleSheetId, out var sheetsFilter) &&
                            sheetsFilter.Count > 0) {
                            request.Ranges = sheetsFilter.Select(it => $"'{it}'").ToList();
                        }
                        else {
                            continue;
                        }
                    }

                    request.IncludeGridData = true;
                    var response = request.Execute();

                    GenerateLocalizationAssets(response, outputFolder, (pageName, progress) => {
                        EditorUtility.DisplayProgressBar($"Localization Import {sheetId.description}", pageName, progress);
                        //
                    });
                }
            }
            catch (Exception ex) {
                Debug.LogException(ex);
            }
            finally {
                AssetDatabase.StopAssetEditing();
                EditorUtility.ClearProgressBar();
            }

            EditorLocalizationSetup.Setup();
        }

        private static void GenerateLocalizationAssets(Spreadsheet response, string outputFolder, Action<string, float> progress) {
            for (int sheetIndex = 0, sheetCount = response.Sheets.Count; sheetIndex < sheetCount; sheetIndex++) {
                var sheet = response.Sheets[sheetIndex];
                if (sheet.Properties.Hidden == true) {
                    continue;
                }

                progress?.Invoke(sheet.Properties.Title, 1f * sheetIndex / sheetCount);

                var sheetData = sheet.Data[0];

                var headers = sheetData.RowData[0]
                    .Values
                    .Select((it, ind) => new {ColumnIndex = ind, Lang = it.FormattedValue})
                    .Where(it => !string.IsNullOrWhiteSpace(it.Lang))
                    .ToList();

                var rows = sheetData.RowData
                    .Skip(1)
                    .Where(row => {
                        if (row.Values == null || row.Values.Count == 0) {
                            return false;
                        }

                        return !string.IsNullOrWhiteSpace(row.Values[0].FormattedValue);
                    })
                    .ToArray();

                foreach (var header in headers) {
                    var tablePath = Path.Combine(outputFolder, $"{sheet.Properties.Title}_{header.Lang}.asset");

                    var table  = LoadOrCreateLocalizationTable(tablePath);
                    var values = Array.ConvertAll(rows, row => row.Values[header.ColumnIndex].FormattedValue);

                    table.SetValues(values);
                    table.SetPage(sheet.Properties.Title);
                    table.SetLang(header.Lang);

                    EditorUtility.SetDirty(table);
                }
            }

            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }

        private static LocalizationTable LoadOrCreateLocalizationTable(string tablePath) {
            var table = AssetDatabase.LoadAssetAtPath<LocalizationTable>(tablePath);
            if (table != null) {
                return table;
            }

            table = ScriptableObject.CreateInstance<LocalizationTable>();
            AssetDatabase.CreateAsset(table, tablePath);
            return table;
        }
    }
}