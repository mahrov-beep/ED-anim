namespace Multicast.DirtyDataEditor.Patcher {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Google.Apis.Services;
    using Google.Apis.Sheets.v4;
    using Google.Apis.Sheets.v4.Data;
    using GoogleSheets;
    using UnityEditor;
    using UnityEngine;
    using Color = Google.Apis.Sheets.v4.Data.Color;

    public partial class DdePatcher<TGameDef> {
        private const int COLUMNS_OFFSET = 1;
        private const int ROWS_OFFSET    = 2;

        public void Upload() {
            try {
                this.UploadInternal();
            }
            catch (Exception ex) {
                Debug.LogException(ex);
            }
            finally {
                EditorUtility.ClearProgressBar();
            }
        }

        private void UploadInternal() {
            if (!this.ShowUploadConfirmationDialog()) {
                return;
            }

            ShowProgressBar("Fetching remote sheets...");

            var changedSheets = this.builder.EnumeratePatchedTables().ToList();

            var settings   = DirtyDataEditorSettings.Instance;
            var credential = CredentialsProvider.ReadCredentials(settings);
            var initializer = new BaseClientService.Initializer() {
                ApplicationName       = "Config Parser",
                HttpClientInitializer = credential,
            };
            var service = new SheetsService(initializer);

            var sheetNameToSpreadsheetIdMap = FetchAllSheetNames(settings, service);

            foreach (var sheetName in changedSheets) {
                if (!sheetNameToSpreadsheetIdMap.TryGetValue(sheetName, out var map)) {
                    Debug.LogError($"Sheet '{sheetName}' not exist in remote dde table");
                    continue;
                }

                ShowProgressBar($"Fetch keys and properties from {sheetName}");

                var googleSheetId = map.googleSheetId;
                var sheetId       = map.sheetId;

                if (!this.TryFetchKeysAndPropertiesFromSheet(service, googleSheetId, sheetName, out var keys, out var properties)) {
                    Debug.LogError($"Failed to fetch sheet '{sheetName}'");
                    continue;
                }

                var patches = this.builder.EnumerateAllByTableName(sheetName).ToList();
                var newKeys = patches.Select(it => it.key).Except(keys).ToList();

                if (newKeys.Count != 0) {
                    if (!this.ShowInsertKeysConfirmationDialog(newKeys.Count, sheetName)) {
                        continue;
                    }

                    ShowProgressBar($"Inserting new keys to {sheetName}");

                    var appendRange = new ValueRange {
                        Range          = $"'{sheetName}'!A{ROWS_OFFSET + keys.Count + 1}",
                        Values         = new List<IList<object>> {newKeys.Cast<object>().ToList(),},
                        MajorDimension = "COLUMNS",
                    };

                    var appendKeysRequest = service.Spreadsheets.Values.Append(appendRange, googleSheetId, appendRange.Range);
                    appendKeysRequest.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;
                    appendKeysRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.RAW;
                    appendKeysRequest.Execute();

                    Debug.Log($"Inserted {newKeys.Count} keys to '{sheetName}':{keys.Aggregate("", (s, it) => s + Environment.NewLine + it)}");

                    if (!this.TryFetchKeysAndPropertiesFromSheet(service, googleSheetId, sheetName, out keys, out properties)) {
                        Debug.LogError($"Failed to re-fetch sheet '{sheetName}' after insertion");
                        continue;
                    }
                }

                var batchUpdateValues = new BatchUpdateValuesRequest {
                    ValueInputOption = "RAW",
                    Data             = new List<ValueRange>(),
                };

                var batchUpdateSpreadsheet = new BatchUpdateSpreadsheetRequest {
                    Requests = new List<Request>(),
                };

                var bgColor = this.backgroundColor.GetValueOrDefault(new UnityEngine.Color(1.0f, 0.75f, 0.55f, 1.0f));
                var patcherCellFormat = new CellFormat {
                    BackgroundColor = new Color {
                        Red   = bgColor.r,
                        Green = bgColor.g,
                        Blue  = bgColor.b,
                        Alpha = bgColor.a,
                    },
                };

                ShowProgressBar($"Generating patch for {sheetName}");

                foreach (var patch in patches) {
                    var keyIndex      = keys.IndexOf(patch.key);
                    var propertyIndex = properties.IndexOf(patch.property);

                    if (keyIndex == -1) {
                        Debug.LogError($"Failed to patch {patch.key} at '{sheetName}'. Key not exist in remote sheet");
                        continue;
                    }

                    if (propertyIndex == -1) {
                        Debug.LogError($"Failed to patch '{sheetName}'/'{patch.property}'. Property not exist in remote sheet");
                        continue;
                    }

                    batchUpdateValues.Data.Add(new ValueRange {
                        Range          = $"'{sheetName}'!{GetExcelColumnName(COLUMNS_OFFSET + propertyIndex + 1)}{ROWS_OFFSET + keyIndex + 1}",
                        Values         = new List<IList<object>> {new List<object> {patch.value}},
                        MajorDimension = "COLUMNS",
                    });

                    batchUpdateSpreadsheet.Requests.Add(new Request {
                        RepeatCell = new RepeatCellRequest {
                            Range = new GridRange {
                                SheetId          = sheetId,
                                StartColumnIndex = COLUMNS_OFFSET + propertyIndex,
                                EndColumnIndex   = COLUMNS_OFFSET + propertyIndex + 1,
                                StartRowIndex    = ROWS_OFFSET + keyIndex,
                                EndRowIndex      = ROWS_OFFSET + keyIndex + 1,
                            },
                            Cell = new CellData {
                                UserEnteredFormat = patcherCellFormat,
                            },
                            Fields = "UserEnteredFormat(BackgroundColor)",
                        },
                    });
                }

                if (batchUpdateValues.Data.Count > 0) {
                    ShowProgressBar($"Apply patch values on {sheetName}");

                    var batchUpdateValuesRequest = service.Spreadsheets.Values.BatchUpdate(batchUpdateValues, googleSheetId);
                    batchUpdateValuesRequest.Execute();
                }

                if (batchUpdateSpreadsheet.Requests.Count > 0) {
                    ShowProgressBar($"Apply patch formatting on {sheetName}");

                    var batchUpdateSpreadsheetRequest = service.Spreadsheets.BatchUpdate(batchUpdateSpreadsheet, googleSheetId);
                    batchUpdateSpreadsheetRequest.Execute();
                }

                Debug.Log("Updated values:" + batchUpdateValues.Data.Aggregate("", (s, it) => s + Environment.NewLine + it.Range + " = " + it.Values[0][0]));
            }

            ShowProgressBar($"Import changes from DDE");

            var sheetNamesBySpreadsheetIdFilter = sheetNameToSpreadsheetIdMap
                .Where(it => changedSheets.Contains(it.Key))
                .GroupBy(it => it.Value.googleSheetId)
                .ToDictionary(gr => gr.Key, gr => gr.Select(it => it.Key).ToList());

            DirtyDataGoogleSheetImporter.Import(settings, sheetNamesBySpreadsheetIdFilter);
        }

        private bool TryFetchKeysAndPropertiesFromSheet(
            SheetsService service, string googleSheetId, string sheetName,
            out List<string> keys, out List<string> properties) {
            var getRequest = service.Spreadsheets.Get(googleSheetId);
            getRequest.Ranges          = new List<string> {$"'{sheetName}'!A3:A", $"'{sheetName}'!B1:1"};
            getRequest.IncludeGridData = true;
            var response = getRequest.Execute();

            if (response.Sheets.Count != 1 || response.Sheets[0].Data.Count != 2) {
                keys       = null;
                properties = null;
                return false;
            }

            var keysData       = response.Sheets[0].Data[0].RowData ?? new List<RowData>();
            var propertiesData = response.Sheets[0].Data[1].RowData ?? new List<RowData>();

            keys = keysData
                .SelectMany(it => it.Values)
                .Select(it => it.FormattedValue)
                .Reverse()
                .SkipWhile(it => string.IsNullOrEmpty(it))
                .Reverse()
                .ToList();

            properties = propertiesData
                .SelectMany(it => it.Values)
                .Select(it => it.FormattedValue)
                .Reverse()
                .SkipWhile(it => string.IsNullOrEmpty(it))
                .Reverse()
                .Select(it => FixPropertyName(it))
                .ToList();

            //Debug.LogError("KEYS: " + keys.Aggregate("", (s, it) => s + Environment.NewLine + it));
            //Debug.LogError("PROPERTIES: " + properties.Aggregate("", (s, it) => s + Environment.NewLine + it));

            return true;
        }

        public static Dictionary<string, (string googleSheetId, int? sheetId)> FetchAllSheetNames(
            DirtyDataEditorSettings settings, SheetsService service) {
            var sheetNameToSpreadsheetIdMap = new Dictionary<string, (string googleSheetId, int? sheetId)>();

            for (int sheetIndex = 0, sheetsCount = settings.sheets.Length; sheetIndex < sheetsCount; sheetIndex++) {
                var settingsSheet = settings.sheets[sheetIndex];

                foreach (var googleSheetId in settingsSheet.googleSheetIds) {
                    ShowProgressBar($"Fetching {settingsSheet.name} ({googleSheetId})");

                    var request  = service.Spreadsheets.Get(googleSheetId);
                    var response = request.Execute();

                    foreach (var responseSheet in response.Sheets) {
                        sheetNameToSpreadsheetIdMap[responseSheet.Properties.Title] = (googleSheetId, responseSheet.Properties.SheetId);
                    }
                }
            }

            return sheetNameToSpreadsheetIdMap;
        }

        private bool ShowUploadConfirmationDialog() {
            var changesCount     = this.builder.PatchesCount;
            var changesSheetsStr = this.builder.EnumeratePatchedTables().Aggregate("", (s, it) => s + Environment.NewLine + it);

            var message = $"Do you really want to upload {changesCount} changes to DDE?" +
                          $"{Environment.NewLine}{Environment.NewLine}" +
                          $"Modified sheets:{changesSheetsStr}";

            return EditorUtility.DisplayDialog("DDE Patch", message, "UPLOAD", "Cancel");
        }

        private bool ShowInsertKeysConfirmationDialog(int countToAppend, string sheetName) {
            var message = $"Do you really want to insert {countToAppend} NEW ROWS to {sheetName}?";

            return EditorUtility.DisplayDialog("DDE Patch", message, "INSERT", "Cancel");
        }

        private static void ShowProgressBar(string message) {
            EditorUtility.DisplayProgressBar("DDE Patch", message, 1f);
        }

        private static string GetExcelColumnName(int columnNumber) {
            var columnName = "";

            while (columnNumber > 0) {
                var modulo = (columnNumber - 1) % 26;
                columnName   = Convert.ToChar('A' + modulo) + columnName;
                columnNumber = (columnNumber - modulo) / 26;
            }

            return columnName;
        }

        private static string FixPropertyName(string property) {
            if (property.IndexOf(':') != -1) {
                property = property.Split(':', StringSplitOptions.RemoveEmptyEntries)
                    .Select(it => it.Trim())
                    .Aggregate((a, b) => a + ":" + b);
            }

            property = property.Trim();
            property = property.Replace(' ', '_');

            return property;
        }
    }
}