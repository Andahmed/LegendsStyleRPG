using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace LegendsRPG.Core
{
    /// <summary>
    /// Backend API client stub — the ONLY place that knows the server address.
    /// The base URL is a single constant and is intentionally empty in the
    /// offline prototype; filling it in later phases requires no gameplay
    /// code changes. Never embed production secrets here.
    /// </summary>
    public static class ApiClient
    {
        // Example: "https://api.your-domain.com"  (set in a later phase)
        public const string BaseUrl = "";

        public const string ApiVersion = "api/v1";

        public static bool IsConfigured
        {
            get { return !string.IsNullOrEmpty(BaseUrl); }
        }

        private static string Url(string path)
        {
            return BaseUrl.TrimEnd('/') + "/" + ApiVersion + "/" + path.TrimStart('/');
        }

        public static IEnumerator PostJson(string path, string jsonBody,
            Action<string> onSuccess, Action<string> onError)
        {
            if (!IsConfigured)
            {
                if (onError != null) onError("ApiClient not configured (offline prototype)");
                yield break;
            }

            using (var request = new UnityWebRequest(Url(path), "POST"))
            {
                byte[] body = Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(body);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.timeout = 10;

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    if (onSuccess != null) onSuccess(request.downloadHandler.text);
                }
                else
                {
                    if (onError != null) onError(request.error + " :: " + request.downloadHandler.text);
                }
            }
        }

        public static IEnumerator GetJson(string path, string bearerToken,
            Action<string> onSuccess, Action<string> onError)
        {
            if (!IsConfigured)
            {
                if (onError != null) onError("ApiClient not configured (offline prototype)");
                yield break;
            }

            using (var request = UnityWebRequest.Get(Url(path)))
            {
                if (!string.IsNullOrEmpty(bearerToken))
                {
                    request.SetRequestHeader("Authorization", "Bearer " + bearerToken);
                }
                request.timeout = 10;

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    if (onSuccess != null) onSuccess(request.downloadHandler.text);
                }
                else
                {
                    if (onError != null) onError(request.error);
                }
            }
        }
    }
}
