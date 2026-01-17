namespace Multicast.DirtyDataEditor.GoogleSheets {
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using Google.Apis.Auth.OAuth2;
    using Google.Apis.Sheets.v4;
    using Google.Apis.Util.Store;
    using UnityEngine;

    public class CredentialsProvider {
        public static UserCredential ReadCredentials(DirtyDataEditorSettings settings) {

            var sharedTokenPath = $"{settings.WorkPath}/token.json";
            var localTokenPath  = $"{settings.WorkPath}/token_local.json";

            if (Directory.Exists(sharedTokenPath) && !Directory.Exists(localTokenPath)) {
                Directory.CreateDirectory(localTokenPath);
                File.Copy(sharedTokenPath + "/Google.Apis.Auth.OAuth2.Responses.TokenResponse-user",
                    localTokenPath + "/Google.Apis.Auth.OAuth2.Responses.TokenResponse-user");
            }

            using (var stream =
                new FileStream( $"{settings.CredentialsPath}/credentials.json", FileMode.Open, FileAccess.Read)) {
                var credPath = localTokenPath;
                var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    new List<string> {
                        SheetsService.Scope.Spreadsheets,
                    },
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Debug.Log("Credential file saved to: " + credPath);

                return credential;
            }
        }
    }
}