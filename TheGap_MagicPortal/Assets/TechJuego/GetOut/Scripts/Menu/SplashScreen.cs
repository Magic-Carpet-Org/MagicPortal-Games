using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TechJuego.GetOut.Utils;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;
using TechJuego.GetOut.Network;

namespace TechJuego.GetOut
{
	public class SplashScreen : MonoBehaviour
	{
		// Session ID and Token properties
		public static string GlobalSessionId { get; set; }

		// NEW: JWT Token storage so ApiManager can use it later
		public static string GlobalSessionToken { get; set; }

		// Time To wait After Scene is Loaded
		[SerializeField] private float time;

		private AsyncOperation scene;

		// Loading Progress Text
		[SerializeField] private TextMeshProUGUI m_LoadingText;
		[SerializeField] private Slider m_LoadinImage;

		private float progrss;
		private bool apiFinished = false;
		private bool apiSuccess = false;

		// --------------------------------------------------------
		// WebGL Bridge Import
		// --------------------------------------------------------
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string GetURLParameter(string paramName);
#endif

		IEnumerator Start()
		{
			// --- 1. ESTABLISH SESSION ID & JWT TOKEN ---
			// Only grab from the URL if the ID is empty (First Boot)
			// If it's a soft restart, ApiManager already filled this variable.
			if (string.IsNullOrEmpty(GlobalSessionId))
			{
				GlobalSessionId = "a7c3f92c-a19e-4fdb-8ceb-a866cefd468b"; // Editor Fallback Session
				GlobalSessionToken = "EDITOR_TEST_JWT_TOKEN_12345"; // Editor Fallback Token

#if UNITY_WEBGL && !UNITY_EDITOR
                // Extract Session ID
                string browserId = GetURLParameter("sessionId");
                if (!string.IsNullOrEmpty(browserId))
                {
                    GlobalSessionId = browserId;
                }

                // Extract JWT Token
                string browserToken = GetURLParameter("sessionToken");
                if (!string.IsNullOrEmpty(browserToken))
                {
                    GlobalSessionToken = browserToken;
                }
#endif
			}

			// --- 2. START PORTAL API CONNECTION ---
			ApiManager.Instance.InitializeGame(GlobalSessionId, (success, response) =>
				{
					apiSuccess = success;
					apiFinished = true;

					if (success)
						Debug.Log($"[SplashScreen] Connected to Portal! {response}");
					else
						Debug.LogError($"[SplashScreen] Portal Error: {response}");
				});

			// --- 3. START BACKGROUND SCENE LOAD ---
			scene = SceneManager.LoadSceneAsync("Game");
			scene.allowSceneActivation = false;

			while (!scene.isDone)
			{
				if (scene.progress >= .9f)
				{
					bool visualSweepDone = false;

					StartCoroutine(Utility.ChangeValue(0, 100, time, (value) =>
						{
							progrss = value;
							if (value >= 100)
							{
								visualSweepDone = true;
							}
						}));

					while (!visualSweepDone || !apiFinished)
					{
						yield return null;
					}

					if (apiSuccess)
					{
						scene.allowSceneActivation = true;
					}
					else
					{
						m_LoadingText.text = "Network Error!";
						Debug.LogError("[SplashScreen] Cannot start game. API Initialization Failed.");
					}
					break;
				}
				yield return null;
			}
		}

		private void Update()
		{
			if (m_LoadingText.text != "Network Error!")
			{
				m_LoadingText.text = (int)progrss + "%";
			}

			m_LoadinImage.value = (progrss / 100);
		}
	}
}