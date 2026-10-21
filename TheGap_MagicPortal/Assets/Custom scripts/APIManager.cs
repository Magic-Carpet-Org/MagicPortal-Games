using System;
using System.Collections;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace TechJuego.GetOut.Network
{
	[System.Serializable]
	public class SessionResponse
	{
		public string sessionId;
	}

	public class ApiManager : Singleton<ApiManager>
	{
		private const string BASE_URL = "https://game-portal-cyan.vercel.app/api";

		// --- WebGL Listener Import ---
#if UNITY_WEBGL && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void StartPortalListener(string gameObjectName, string callbackMethodName);

        // Sync function to talk back to the Next.js portal
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void NotifySessionUpdated(string newSessionId);

        // Fast restart: fires REQUEST_RESTART over postMessage instead of an
        // authenticated REST round-trip. The portal's reply (RESTART_GAME)
        // comes back through the listener registered in StartPortalListener,
        // which already calls into OnPortalRestart below.
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void RequestRestartFromGame();
#endif

		private void Start()
		{
#if UNITY_WEBGL && !UNITY_EDITOR
            StartPortalListener(gameObject.name, "OnPortalRestart");
#endif
		}

		// ==========================================
		// NEW: FETCH NEW SESSION WITH JWT AUTH
		// ==========================================
		public void RequestNewSession(string gameId, string mode, string tournamentId, Action<bool, string> onComplete)
		{
			StartCoroutine(RequestNewSessionRoutine(gameId, mode, tournamentId, onComplete));
		}

		private IEnumerator RequestNewSessionRoutine(string gameId, string mode, string tournamentId, Action<bool, string> onComplete)
		{
			string endpoint = $"{BASE_URL}/game/session?gameId={gameId}&mode={mode}";

			if (mode == "tournament" && !string.IsNullOrEmpty(tournamentId))
			{
				endpoint += $"&tournamentId={tournamentId}";
			}

			Debug.Log($"[ApiManager] Requesting new session ticket: {endpoint}");

			using (UnityWebRequest request = UnityWebRequest.Get(endpoint))
			{
				// Attach the JWT token to the HTTP Headers
				string token = SplashScreen.GlobalSessionToken;
				request.SetRequestHeader("Authorization", $"Bearer {token}");

				yield return request.SendWebRequest();

				if (request.result == UnityWebRequest.Result.Success)
				{
					string jsonText = request.downloadHandler.text;
					Debug.Log($"[ApiManager] Success! Server replied: {jsonText}");

					SessionResponse response = JsonUtility.FromJson<SessionResponse>(jsonText);

					if (!string.IsNullOrEmpty(response.sessionId))
					{
#if UNITY_WEBGL && !UNITY_EDITOR
                        NotifySessionUpdated(response.sessionId);
#endif

						OnPortalRestart(response.sessionId);
						onComplete?.Invoke(true, "Restarting...");
					}
					else
					{
						Debug.LogError("[ApiManager] The API worked, but JSON didn't contain a 'sessionId'!");
						onComplete?.Invoke(false, "Invalid JSON");
					}
				}
				else
				{
					Debug.LogError($"[ApiManager] Failed to fetch new session: {request.error}");
					onComplete?.Invoke(false, request.error);
				}
			}
		}

		// ==========================================
		// FAST RESTART (postMessage, skips the GET /game/session round-trip)
		// ==========================================
		public void RequestFastRestart()
		{
#if UNITY_WEBGL && !UNITY_EDITOR
			Debug.Log("[ApiManager] Requesting fast restart via postMessage (REQUEST_RESTART)...");
			RequestRestartFromGame();
#else
			// No real portal in the Editor, so there's nothing to reply with a
			// genuine new session id. A random Guid.NewGuid() looks plausible
			// but doesn't exist on the real backend, so GET /game/init 404s on
			// it and OnPortalRestart's existing failure path sends the player
			// all the way back to Splash instead of restarting the Game scene.
			//
			// Reuse the same seeded editor test session instead (see
			// GameManager.Awake()) - the backend already treats it as a valid,
			// idempotent session, so "Play Again" actually succeeds locally.
			Debug.Log("[ApiManager] REQUEST_RESTART (simulated outside WebGL build - reusing editor test session " + SplashScreen.GlobalSessionId + ")");
			OnPortalRestart(SplashScreen.GlobalSessionId);
#endif
		}

		// --- Instant Soft Restart ---
		public void OnPortalRestart(string newSessionId)
		{
			Debug.Log($"[ApiManager] Soft Restart Triggered! New Session ID: {newSessionId}");

			SplashScreen.GlobalSessionId = newSessionId;

			// Load the Game scene right away instead of waiting on GET /game/init
			// to come back first. The session id came straight from the portal's
			// own RESTART_GAME reply (it already validated it before sending that),
			// so there's nothing to gain by re-checking it before letting the
			// player start playing again - that wait was the last real source of
			// restart delay. LoadSceneAsync also avoids the hitch a synchronous
			// LoadScene causes once the wait is gone.
			UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Game");

			// Still refresh session/player info, but in the background - if this
			// fails, log it and move on rather than interrupting gameplay that's
			// already starting. A genuinely invalid session will still surface
			// naturally when the score submission at the end of this run fails.
			InitializeGame(newSessionId, (success, response) =>
			{
				if (success)
				{
					Debug.Log("[ApiManager] Session info refreshed after restart.");
				}
				else
				{
					Debug.LogWarning($"[ApiManager] Could not refresh session info after restart (continuing anyway): {response}");
				}
			});
		}

		// ==========================================
		// INITIALIZATION & SCORE SUBMISSION
		// ==========================================
		public void InitializeGame(string sessionId, Action<bool, string> onComplete)
		{
			StartCoroutine(InitGameRoutine(sessionId, onComplete));
		}

		private IEnumerator InitGameRoutine(string sessionId, Action<bool, string> onComplete)
		{
			string endpoint = $"{BASE_URL}/game/init?sessionId={sessionId}";

			using (UnityWebRequest request = UnityWebRequest.Get(endpoint))
			{
				yield return request.SendWebRequest();

				string serverMessage = request.downloadHandler != null ? request.downloadHandler.text : "No data";

				if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
				{
					Debug.LogError($"[ApiManager] Init Failed: {request.error} | Backend Says: {serverMessage}");
					onComplete?.Invoke(false, serverMessage);
				}
				else
				{
					Debug.Log($"[ApiManager] Init Success: {serverMessage}");
					onComplete?.Invoke(true, serverMessage);
				}
			}
		}

		public void SubmitScore(string sessionId, int score, Action<bool, string> onComplete)
		{
			StartCoroutine(SubmitScoreRoutine(sessionId, score, onComplete));
		}

		private IEnumerator SubmitScoreRoutine(string sessionId, int score, Action<bool, string> onComplete)
		{
			string endpoint = $"{BASE_URL}/game/match/complete";
			string jsonPayload = $"{{\"sessionId\":\"{sessionId}\",\"score\":{score}}}";

			string signature = GenerateHMACSignature(jsonPayload, GetObfuscatedSecret());

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
					Debug.LogError($"[ApiManager] Score Submit Failed: {request.error} | Backend: {serverMessage}");
					onComplete?.Invoke(false, serverMessage);
				}
				else
				{
					Debug.Log($"[ApiManager] Score Submit Success: {serverMessage}");
					onComplete?.Invoke(true, serverMessage);
				}
			}
		}

		// ==========================================
		// CRYPTOGRAPHY UTILITY
		// ==========================================
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

		private string GetObfuscatedSecret()
		{
			string p1 = "0de42f8c";
			string p2 = "-0c16-";
			string p3 = "48ca-";
			string p4 = "9242-";
			string p5 = "5e20be5223e1";

			return p1 + p2 + p3 + p4 + p5;
		}
	}
}