namespace Multicast.Server {
    using System;
    using System.Text;
    using JetBrains.Annotations;
    using UnityEngine;
    using Utilities;

    internal static class ServerAccessTokenDecoder {
        public static ServerAccessTokenState TryDecode(string jwt, out DateTime? expiration, out Guid? userId, [CanBeNull] out string environment) {
            userId      = null;
            expiration  = null;
            environment = null;

            try {
                if (string.IsNullOrEmpty(jwt)) {
                    return ServerAccessTokenState.Null;
                }

                var tokenParts = jwt.Split('.');
                if (tokenParts.Length != 3) {
                    return ServerAccessTokenState.Malformed;
                }

                var payloadJson = Encoding.UTF8.GetString(Convert.FromBase64String(FixBase64String(tokenParts[1])));
                var payload     = JsonUtility.FromJson<JwtPayload>(payloadJson);

                if (payload.exp > 0) {
                    expiration = DateTimeUtils.FromUnixTime(payload.exp);
                }

                if (Guid.TryParse(payload.nameid, out var parsedUserId)) {
                    userId = parsedUserId;
                }

                if (payload.env is { } parsedEnv) {
                    environment = parsedEnv;
                }

                var allFieldsSet = userId != null && environment != null;

                return !allFieldsSet ? ServerAccessTokenState.Malformed
                    : DateTime.UtcNow > expiration ? ServerAccessTokenState.Expired
                    : ServerAccessTokenState.Valid;
            }
            catch (Exception ex) {
                Debug.LogException(ex);
                return ServerAccessTokenState.Malformed;
            }
        }

        private static string FixBase64String(string str) {
            return str.Replace("-", "+").Replace("_", "/") + (str.Length % 4) switch {
                0 => "",
                2 => "==",
                3 => "=",
                _ => throw new InvalidOperationException("Malformed JWT token, invalid length"),
            };
        }

        private struct JwtPayload {
            // ReSharper disable InconsistentNaming
            // ReSharper disable IdentifierTypo
            public string nameid;
            public long   nbf;
            public long   exp;
            public string iss;
            public string aud;
            public string env;
            // ReSharper restore IdentifierTypo
            // ReSharper restore InconsistentNaming
        }
    }
}