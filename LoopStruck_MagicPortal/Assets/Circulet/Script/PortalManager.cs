using System.Collections;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class PortalManager : MonoBehaviour
{
    public static PortalManager Instance { get; private set; }

    [Header("API Configuration")]
    public string baseApiUrl = "https://game-portal-cyan.vercel.app/api/game";
    public string webhookSecret = "0d9916ac-630e-4c58-ba32-c6c30c1e2e8a";
    public string currentSessionId;

    // --- NEW JSLIB IMPORTS ---
    [DllImport("__Internal")] private static extern string GetURLParameter(string paramName);
    [DllImport("__Internal")] private static extern void StartPortalListener(string gameObjectPtr, string callbackMethodPtr);
    [DllImport("__Internal")] private static extern void NotifySessionUpdated(string newSessionId);

    // --- RETAINED JSLIB IMPORTS (Ensure these remain in your final .jslib file!) ---
    [DllImport("__Internal")] private static extern void PortalSendScoreUpdate(int score);
    [DllImport("__Internal")] private static extern void PortalSendMatchComplete(int score);
    [DllImport("__Internal")] private static extern void PortalRequestRestart();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        // 1. Grab the URL parameter using the new dynamic method
        currentSessionId = GetURLParameter("sessionId");
        
        // 2. Start the listener by passing this exact GameObject name and the callback method name
        StartPortalListener(gameObject.name, nameof(OnReceiveRestart));
#else
        // Use the active session ID provided from the database for Editor testing
        currentSessionId = "aef93649-be28-491e-8947-b0a77d85f1e5";
        Debug.Log($"[PortalManager] Editor Mode: Using active Session ID: {currentSessionId}");
#endif
        if (!string.IsNullOrEmpty(currentSessionId))
        {
            StartCoroutine(InitGameSession(currentSessionId));
        }
    }

    public void RequestRestart()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        PortalRequestRestart();
#else
        // Re-use the active ID for Editor testing. 
        // Note: If your database deletes or consumes a session upon match completion, 
        // restarting and submitting a second score with this same ID will result in a 404.
        Debug.Log($"[PortalManager] Editor Mode: Mocking Restart with Session ID: {currentSessionId}");
        OnReceiveRestart(currentSessionId);
#endif
    }

    private IEnumerator InitGameSession(string sessionId)
    {
        string url = $"{baseApiUrl}/init?sessionId={sessionId}";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Session Initialized: " + request.downloadHandler.text);
            }
        }
    }

    public void SendScoreUpdate(int score)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        PortalSendScoreUpdate(score);
#endif
    }

    public void SendMatchComplete(int score)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        PortalSendMatchComplete(score);
#endif
        StartCoroutine(PostMatchScore(score));
    }



    // 3. This callback is triggered by the .jslib when RESTART_GAME is received
    public void OnReceiveRestart(string newSessionId)
    {
        currentSessionId = newSessionId;

#if UNITY_WEBGL && !UNITY_EDITOR
        // Fire the exact payload back to the Next.js window as requested
        NotifySessionUpdated(newSessionId);
#endif

        StartCoroutine(InitGameSession(newSessionId));

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    // 1. Add this small class to guarantee minified JSON formatting
    [System.Serializable]
    private class ScorePayload
    {
        public string sessionId;
        public int score;
    }

    // 2. Replace the PostMatchScore Coroutine
    private IEnumerator PostMatchScore(int score)
    {
        // 1. Safety Check: Don't send if we don't have an ID
        if (string.IsNullOrEmpty(currentSessionId))
        {
            Debug.LogError("Match POST Error: currentSessionId is null or empty! Are you missing the URL parameter?");
            yield break;
        }

        // 2. Log the exact ID we are about to send to the backend
        Debug.Log($"[PortalManager] Attempting to submit score: {score} for Session ID: '{currentSessionId}'");

        string url = $"{baseApiUrl}/match/complete";

        ScorePayload payloadObj = new ScorePayload { sessionId = currentSessionId, score = score };
        string jsonBody = JsonUtility.ToJson(payloadObj);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");

            string signature = CalculateHMAC(jsonBody, webhookSecret);
            request.SetRequestHeader("X-Portal-Signature", signature);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Match POST Error: {request.error} | Server Response: {request.downloadHandler.text}");
            }
            else
            {
                Debug.Log($"Score successfully submitted! Server Response: {request.downloadHandler.text}");
            }
        }
    }

    private string CalculateHMAC(string payload, string secret)
    {
        byte[] secretBytes = Encoding.UTF8.GetBytes(secret);
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

        using (HMACSHA256 hmac = new HMACSHA256(secretBytes))
        {
            byte[] hash = hmac.ComputeHash(payloadBytes);
            StringBuilder hexString = new StringBuilder();
            foreach (byte b in hash)
            {
                hexString.AppendFormat("{0:x2}", b);
            }
            return hexString.ToString();
        }
    }
}