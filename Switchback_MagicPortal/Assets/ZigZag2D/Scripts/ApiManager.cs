using System;
using System.Collections;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace TunnelGame
{
    [System.Serializable]
    public class GameInitResponse
    {
        public bool success;
        public SessionInfo session;
        public PlayerInfoResponse player;
    }

    [System.Serializable]
    public class SessionInfo
    {
        public string id;
        public string mode;
        public string status;
        public string game_title;
        public string game_slug;
    }

    [System.Serializable]
    public class PlayerInfoResponse
    {
        public string id;
        public string name;
        public string username;
        public string avatar;
    }

    [System.Serializable]
    public class MatchCompleteResponse
    {
        public bool success;
        public string message;
        public bool is_personal_best;
        public int best_score;
    }

    /// <summary>
    /// REST client for the Switchback portal API (GET /game/init, POST
    /// /game/match/complete), signing requests the same way the portal's
    /// Node.js / Python samples do.
    /// </summary>
    public class ApiManager : SingletonComponent<ApiManager>
    {
        #region Constants

        private const string BASE_URL = "https://game-portal-cyan.vercel.app/api";

        #endregion

        #region Public Methods

        public void InitializeGame(string sessionId, Action<bool, GameInitResponse> onComplete)
        {
            StartCoroutine(InitGameRoutine(sessionId, onComplete));
        }

        public void SubmitScore(string sessionId, int score, Action<bool, MatchCompleteResponse> onComplete)
        {
            StartCoroutine(SubmitScoreRoutine(sessionId, score, onComplete));
        }

        #endregion

        #region Coroutines

        private IEnumerator InitGameRoutine(string sessionId, Action<bool, GameInitResponse> onComplete)
        {
            string endpoint = $"{BASE_URL}/game/init?sessionId={sessionId}";

            using (UnityWebRequest request = UnityWebRequest.Get(endpoint))
            {
                yield return request.SendWebRequest();

                string serverMessage = request.downloadHandler != null ? request.downloadHandler.text : "No data";

                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError($"[ApiManager] Init failed: {request.error} | Backend says: {serverMessage}");
                    onComplete?.Invoke(false, null);
                }
                else
                {
                    Debug.Log($"[ApiManager] Init success: {serverMessage}");

                    GameInitResponse response = JsonUtility.FromJson<GameInitResponse>(serverMessage);
                    onComplete?.Invoke(true, response);
                }
            }
        }

        private IEnumerator SubmitScoreRoutine(string sessionId, int score, Action<bool, MatchCompleteResponse> onComplete)
        {
            string endpoint = $"{BASE_URL}/game/match/complete";

            // Matches the portal's samples: json.dumps(payload, separators=(',', ':'))
            // i.e. no spaces - build it by hand so the signature matches exactly.
            string jsonPayload = $"{{\"sessionId\":\"{sessionId}\",\"score\":{score}}}";

            string signature = GenerateHMACSignature(jsonPayload, GetWebhookSecret());

            using (UnityWebRequest request = new UnityWebRequest(endpoint, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();

                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("X-Portal-Signature", signature);

                yield return request.SendWebRequest();

                string serverMessage = request.downloadHandler != null ? request.downloadHandler.text : "No data";

                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError($"[ApiManager] Score submit failed: {request.error} | Backend says: {serverMessage}");
                    onComplete?.Invoke(false, null);
                }
                else
                {
                    Debug.Log($"[ApiManager] Score submit success: {serverMessage}");

                    MatchCompleteResponse response = JsonUtility.FromJson<MatchCompleteResponse>(serverMessage);
                    onComplete?.Invoke(true, response);
                }
            }
        }

        #endregion

        #region Cryptography

        private string GenerateHMACSignature(string payload, string secret)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(secret);
            byte[] messageBytes = Encoding.UTF8.GetBytes(payload);

            using (HMACSHA256 hmac = new HMACSHA256(keyBytes))
            {
                byte[] hashBytes = hmac.ComputeHash(messageBytes);
                StringBuilder hexString = new StringBuilder(hashBytes.Length * 2);

                foreach (byte b in hashBytes)
                {
                    hexString.AppendFormat("{0:x2}", b);
                }

                return hexString.ToString();
            }
        }

        private string GetWebhookSecret()
        {
            string p1 = "4bf63be5";
            string p2 = "-b72d-";
            string p3 = "4342-";
            string p4 = "b3f0-";
            string p5 = "37bdca37f856";

            return p1 + p2 + p3 + p4 + p5;
        }

        #endregion
    }
}