namespace Multicast.DirtyDataEditor.GoogleSheets {
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Google.Apis.Services;
    using Google.Apis.Sheets.v4;
    using JetBrains.Annotations;
    using Sirenix.OdinInspector.Editor;
    using Sirenix.Utilities;
    using UnityEditor;
    using UnityEngine;
    using UnityToolbarExtender;

    public static class DirtyDataGoogleSheetImporter {
        [InitializeOnLoadMethod]
        private static void Init() {
            ValidateDirtyDataAsset();

            DirtyDataEditorSettings.ImportConfigsCallback = Import;
        }

        private static void ValidateDirtyDataAsset() {
            AssetDatabase.FindAssets("t:" + typeof(DirtyDataValidationAsset))
                .Select(AssetDatabase.GUIDToAssetPath)
                .ForEach(guid => AssetDatabase.ImportAsset(guid, ImportAssetOptions.ForceUpdate));
        }

        public static void Import(DirtyDataEditorSettings settings) {
            Import(settings, null);
        }

        public static void Import(DirtyDataEditorSettings settings,
            [CanBeNull] Dictionary<string, List<string>> sheetsBySpreadsheetIdFilter) {
            try {
                EditorUtility.DisplayProgressBar("DDE Import", "", 0f);
                AssetDatabase.StartAssetEditing();

                var credential = CredentialsProvider.ReadCredentials(settings);

                var initializer = new BaseClientService.Initializer() {
                    ApplicationName       = "Config Parser",
                    HttpClientInitializer = credential,
                };

                for (int sheetIndex = 0, sheetsCount = settings.sheets.Length; sheetIndex < sheetsCount; sheetIndex++) {
                    var settingsSheet = settings.sheets[sheetIndex];

                    foreach (var googleSheetId in settingsSheet.googleSheetIds) {
                        var service = new SheetsService(initializer);
                        var request = service.Spreadsheets.Get(googleSheetId);

                        if (sheetsBySpreadsheetIdFilter != null) {
                            if (sheetsBySpreadsheetIdFilter.TryGetValue(googleSheetId, out var sheetsFilter) &&
                                sheetsFilter.Count > 0) {
                                request.Ranges = sheetsFilter.Select(it => $"'{it}'").ToList();
                            }
                            else {
                                continue;
                            }
                        }

                        request.IncludeGridData = true;
                        var response = request.Execute();

                        var outputFolder = Path.Combine(settings.OutputConfigsPath, settingsSheet.name);

                        if (!Directory.Exists(outputFolder)) {
                            Directory.CreateDirectory(outputFolder);
                        }

                        SheetConverter.Convert(response, outputFolder, (pageName, p) => {
                            var progress = 1f * (sheetIndex + p) / sheetsCount;
                            EditorUtility.DisplayProgressBar("DDE Import", pageName, progress);
                        });
                    }
                }
            }
            finally {
                AssetDatabase.StopAssetEditing();
                EditorUtility.ClearProgressBar();
            }

            ValidateDirtyDataAsset();
        }
    }
}