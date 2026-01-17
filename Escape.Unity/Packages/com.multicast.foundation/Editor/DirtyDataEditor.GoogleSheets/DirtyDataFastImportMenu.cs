namespace Multicast.DirtyDataEditor.GoogleSheets {
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Google.Apis.Services;
    using Google.Apis.Sheets.v4;
    using Sirenix.OdinInspector.Editor;
    using UnityEngine;

    public class DirtyDataFastImportMenu {
        public static async void ShowLoadSheetDialog(Rect rect) {
            var cts                         = new CancellationTokenSource();
            var sheetNameToSpreadsheetIdMap = new Dictionary<string, string>();
            var selector                    = new GenericSelector<string>("DDE", supportsMultiSelect: true);

            selector.SelectionTree.SortMenuItemsByName();
            selector.SelectionCancelled += () => cts.Dispose();
            selector.SelectionConfirmed += s => {
                cts.Dispose();
                ImportSelectedOnly(s.ToList());
            };
            var selectorWindow = selector.ShowInPopup(rect, 300f);

            await Task.Yield();

            var ct = cts.Token;

            await foreach (var sheetName in FetchAllSheetNamesAsync(ct, sheetNameToSpreadsheetIdMap)) {
                ct.ThrowIfCancellationRequested();

                selector.SelectionTree.Add(sheetName, sheetName);
                selector.SelectionTree.SortMenuItemsByName();
                selector.SelectionTree.MarkDirty();

                selectorWindow.Repaint();
            }

            void ImportSelectedOnly(List<string> selectedSheets) {
                var sheetNamesBySpreadsheetIdFilter = sheetNameToSpreadsheetIdMap
                    .Where(it => selectedSheets.Contains(it.Key))
                    .GroupBy(it => it.Value)
                    .ToDictionary(gr => gr.Key, gr => gr.Select(it => it.Key).ToList());

                DirtyDataGoogleSheetImporter.Import(DirtyDataEditorSettings.Instance, sheetNamesBySpreadsheetIdFilter);
            }
        }

        public static async IAsyncEnumerable<string> FetchAllSheetNamesAsync(
            [EnumeratorCancellation] CancellationToken ct,
            Dictionary<string, string> sheetNameToSpreadsheetIdMap) {
            var settings   = DirtyDataEditorSettings.Instance;
            var credential = CredentialsProvider.ReadCredentials(settings);

            var initializer = new BaseClientService.Initializer() {
                ApplicationName       = "Config Parser",
                HttpClientInitializer = credential,
            };

            for (int sheetIndex = 0, sheetsCount = settings.sheets.Length; sheetIndex < sheetsCount; sheetIndex++) {
                var settingsSheet = settings.sheets[sheetIndex];

                foreach (var googleSheetId in settingsSheet.googleSheetIds) {
                    var service  = new SheetsService(initializer);
                    var request  = service.Spreadsheets.Get(googleSheetId);
                    var response = await request.ExecuteAsync(ct);

                    foreach (var responseSheet in response.Sheets) {
                        sheetNameToSpreadsheetIdMap[responseSheet.Properties.Title] = googleSheetId;
                        yield return responseSheet.Properties.Title;
                    }
                }
            }
        }
    }
}