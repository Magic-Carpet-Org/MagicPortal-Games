using System;
using System.Collections;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Kilimanjaro.Portals
{
    [Serializable]
    public class MagicGamesInitResponse
    {
        public bool success;
        public MagicGamesSessionInfo session;
        public MagicGamesPlayerInfo player;
    }

    [Serializable]
    public class MagicGamesSessionInfo
    {
        public string id;
        public string mode;
        public string status;
        public string game_title;
        public string game_slug;
    }

    [Serializable]
    public class MagicGamesPlayerInfo
    {
        public string id;
        public string name;
        public string username;
        public string avatar;
    }

    [Serializable]
    public class MagicGamesMatchResponse
    {
        public bool success;
        public string message;
        public bool is_personal_best;
        public int best_score;
    }

    /// <summary>
    /// REST client for the MagicGames (Switchback) API: GET /game/init and
    /// POST /game/match/complete, HMAC-signed like the portal's samples.
    /// Base URL and secret come from PortalSettings. Created by PortalManager.
    /// </summary>
    public class MagicGamesApi : MonoBehaviour
    {
        private PortalSettings settings;

        public bool HasSecret { get { return !string.IsNullOrEmpty(settings.magicGamesWebhookSecret); } }

        private void Awake()
        {
            settings = PortalSettings.Load();
            DontDestroyOnLoad(gameObject);
        }

        public void InitializeGame(string sessionId, Action<bool, MagicGamesInitResponse> onComplete)
        {
            StartCoroutine(InitRoutine(sessionId, onComplete));
        }

        public void SubmitScore(string sessionId, int score, Action<bool, MagicGamesMatchResponse> onComplete)
        {
            if (!HasSecret)
            {
                Debug.LogWarning("[MagicGamesApi] No webhook secret in PortalSettings - score not submitted over REST.");
                if (onComplete != null) onComplete(false, null);
                return;
            }

            StartCoroutine(SubmitRoutine(sessionId, score, onComplete));
        }

        private IEnumerator InitRoutine(string sessionId, Action<bool, MagicGamesInitResponse> onComplete)
        {
            string endpoint = settings.magicGamesApiBaseUrl + "/game/init?sessionId=" + Uri.EscapeDataString(sessionId);

            using (UnityWebRequest request = UnityWebRequest.Get(endpoint))
            {
                yield return request.SendWebRequest();

                string body = request.downloadHandler != null ? request.downloadHandler.text : "No data";

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("[MagicGamesApi] Init failed: " + request.error + " | Backend says: " + body);
                    if (onComplete != null) onComplete(false, null);
                }
                else
                {
                    if (onComplete != null) onComplete(true, JsonUtility.FromJson<MagicGamesInitResponse>(body));
                }
            }
        }

        private IEnumerator SubmitRoutine(string sessionId, int score, Action<bool, MagicGamesMatchResponse> onComplete)
        {
            string endpoint = settings.magicGamesApiBaseUrl + "/game/match/complete";

            // Built by hand (no spaces) so the signature matches the portal's samples exactly.
            string json = "{\"sessionId\":\"" + sessionId + "\",\"score\":" + score + "}";

            using (UnityWebRequest request = new UnityWebRequest(endpoint, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("X-Portal-Signature", Sign(json, settings.magicGamesWebhookSecret));

                yield return request.SendWebRequest();

                string body = request.downloadHandler != null ? request.downloadHandler.text : "No data";

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("[MagicGamesApi] Score submit failed: " + request.error + " | Backend says: " + body);
                    if (onComplete != null) onComplete(false, null);
                }
                else
                {
                    if (onComplete != null) onComplete(true, JsonUtility.FromJson<MagicGamesMatchResponse>(body));
                }
            }
        }

        private static string Sign(string payload, string secret)
        {
            using (HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
            {
                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
                StringBuilder hex = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash) hex.AppendFormat("{0:x2}", b);

                return hex.ToString();
            }
        }
    }
}
