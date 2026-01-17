namespace Multicast.Server {
    using System;
    using System.Globalization;
    using Cysharp.Threading.Tasks;
    using JetBrains.Annotations;
    using UniMob;
    using UnityEngine;
    using UnityEngine.Networking;

    public static class CoreWebRequest {
        [PublicAPI]
        [MustUseReturnValue]
        public static async UniTask<byte[]> SendRequestAsync(Lifetime lifetime, string requestUrl, byte[] requestBytes, CoreWebRequestArgs args) {
            var requestId = Guid.NewGuid().ToString();

            try {
                try {
                    return await SendCoreAsync();
                }
                catch (UnityWebRequestException ex) when (IsAuthenticationRequiredException(ex)) {
                    var authorized = await args.AuthDelegate.Invoke();
                    if (!authorized) {
                        throw;
                    }

                    return await SendCoreAsync();
                }
            }
            catch (UnityWebRequestException ex) {
                App.Analytics.Send(new ServerRequestNetworkErrorAnalyticEvent {
                    errorCategory = ex.Result.ToString(),
                    requestUrl    = requestUrl,
                    errorCode     = (int)ex.ResponseCode,
                    errorMessage  = ex.Error,
                });

                throw new ServerRequestException(ServerRequestFailReason.NetworkError, $"Failed to send web request ({requestUrl}): {ex.Result}, {ex.Error}");
            }

            async UniTask<byte[]> SendCoreAsync() {
                for (var retryIndex = 0; retryIndex <= args.RetryCount; retryIndex++) {
                    try {
                        using var webRequest = new UnityWebRequest(
                            requestUrl,
                            UnityWebRequest.kHttpVerbPOST,
                            new DownloadHandlerBuffer(),
                            new UploadHandlerRaw(requestBytes)
                        );
                        webRequest.uploadHandler.contentType = ServerConstants.CONTENT_TYPE_MSGPACK;

                        webRequest.SetRequestHeader("rid", requestId);

                        var accessToken = args.AccessTokenDelegate();
                        if (!string.IsNullOrEmpty(accessToken.Raw)) {
                            webRequest.SetRequestHeader("Authorization", $"Bearer {accessToken.Raw}");
                        }

                        webRequest.timeout = retryIndex == 0 ? args.TimeoutSeconds
                            : retryIndex <= args.RetryTimeoutSeconds.Length ? args.RetryTimeoutSeconds[retryIndex - 1]
                            : 0;

                        if (Debug.unityLogger.IsLogTypeAllowed(LogType.Log)) {
                            Debug.Log($"Sending web request ({requestUrl}): {webRequest.method}({webRequest.uploadHandler.contentType})" +
                                      $", Payload={requestBytes.Length} bytes" +
                                      $", AccessToken={accessToken}" +
                                      $", Timeout={webRequest.timeout}" +
                                      $", RequestId={requestId}" +
                                      (retryIndex > 0 ? $", Retry={retryIndex}/{args.RetryCount}" : "")
                            );
                        }

                        var webResponse = await webRequest.SendWebRequest().WithCancellation(lifetime);

                        if (webResponse.GetResponseHeader("mc-err") is { } errorMessage) {
                            App.Analytics.Send(new ServerRequestUserErrorAnalyticEvent {
                                requestUrl   = requestUrl,
                                errorMessage = errorMessage,
                            });

                            throw new ServerRequestException(ServerRequestFailReason.UserError, $"Server failed to handle request: {errorMessage}") {
                                UserErrorMessage = errorMessage,
                            };
                        }

                        return webResponse.downloadHandler.data;
                    }
                    catch (UnityWebRequestException ex) when (retryIndex < args.RetryCount && CanRetryRequest(ex)) {
                        Debug.LogWarning($"Failed to send web request ({requestUrl}): {ex.Result}, Error={ex.Error}");

                        if (retryIndex < args.RetryDelays.Length) {
                            await UniTask.Delay(args.RetryDelays[retryIndex], ignoreTimeScale: true, cancellationToken: lifetime);
                        }
                    }
                }

                throw new ServerRequestException(ServerRequestFailReason.Unknown, "Failed to send web request due to internal error (last retry was catch)");
            }

            static bool CanRetryRequest(UnityWebRequestException ex) {
                return ex.Result == UnityWebRequest.Result.ConnectionError;
            }

            static bool IsAuthenticationRequiredException(UnityWebRequestException ex) {
                return ex.Result == UnityWebRequest.Result.ProtocolError && ex.ResponseCode == 401;
            }
        }
    }

    [RequireFieldsInit]
    public struct CoreWebRequestArgs {
        public Func<UniTask<bool>>         AuthDelegate;
        public Func<ServerAccessTokenInfo> AccessTokenDelegate;
        public int                         TimeoutSeconds;
        public int                         RetryCount;
        public TimeSpan[]                  RetryDelays;
        public int[]                       RetryTimeoutSeconds;
    }
}