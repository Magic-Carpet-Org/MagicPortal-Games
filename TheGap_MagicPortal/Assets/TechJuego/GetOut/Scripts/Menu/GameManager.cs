using UnityEngine;
using TechJuego.GetOut.Network;

namespace TechJuego.GetOut
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        [Header("Restart API Settings")]
        [Tooltip("Paste the UUID from Eniibukun here")]
        public string gameId = "YOUR_GAME_UUID_HERE";

        [Tooltip("Type 'practice' or 'tournament'")]
        public string mode = "practice";

        [Tooltip("Only required if mode is 'tournament'")]
        public string tournamentId = "";

        [Header("Game Elements")]
        public Ball m_Ball;
        public Effect m_Effect;
        public GameObject m_LooseEffect;

        [Header("Game State")]
        private int _encryptedScore;
        private int _secretKey = 8492;
        private bool isRestarting = false; // NEW: guards against duplicate "Play Again" clicks

        public int Score
        {
            get { return _encryptedScore ^ _secretKey; }
            set { _encryptedScore = value ^ _secretKey; }
        }

        public bool isGameEnded;
        public Color[] m_BgColor;
        private bool scoreSubmitted = false;

        private void Awake()
        {
            Instance = this;
            isGameEnded = false;
            scoreSubmitted = false;
            isRestarting = false; // NEW: fresh scene load always starts unlocked
            Score = 0;

            // ==========================================
            // NEW: FAKE SESSION ID & TOKEN FOR EDITOR TESTING
            // ==========================================
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(SplashScreen.GlobalSessionId))
            {
                SplashScreen.GlobalSessionId = "a7c3f92c-a19e-4fdb-8ceb-a866cefd468b";
                SplashScreen.GlobalSessionToken = "EDITOR_TEST_JWT_TOKEN_12345";

                Debug.Log("<color=magenta>[Editor Debug] Injected LIVE Session ID and Test Token!</color>");
            }
#endif
            // ==========================================

            if (Camera.main != null && m_BgColor.Length > 0)
            {
                Camera.main.backgroundColor = m_BgColor[UnityEngine.Random.Range(0, m_BgColor.Length)];
            }
        }

        private void Start()
        {
            DataHandler.Instance.gameState = GameState.CreateLevel;
        }

        public void TriggerGameOver()
        {
            try
            {
                Debug.Log("<color=yellow>[GameManager] 1. TriggerGameOver() was called!</color>");

                if (scoreSubmitted)
                {
                    Debug.LogWarning("[GameManager] 2. Score already submitted. Ignoring duplicate call.");
                    return;
                }

                scoreSubmitted = true;
                isGameEnded = true;

                int finalScore = Score;
                string sessionId = SplashScreen.GlobalSessionId;

                Debug.Log($"<color=yellow>[GameManager] 3. Match Over. Session ID is: '{sessionId}'. Final Score: {finalScore}</color>");

                if (TechJuego.GetOut.Network.ApiManager.Instance == null)
                {
                    Debug.LogError("<color=red>[GameManager] FATAL: ApiManager.Instance is NULL! Did you start the game from the Splash scene?</color>");
                    return;
                }

                if (!string.IsNullOrEmpty(sessionId))
                {
                    Debug.Log("<color=yellow>[GameManager] 4. Firing SubmitScore payload to ApiManager...</color>");

                    TechJuego.GetOut.Network.ApiManager.Instance.SubmitScore(sessionId, finalScore, (success, response) =>
                    {
                        if (success)
                        {
                            Debug.Log($"<color=green>[GameManager] 5. SUCCESS! Score accepted by portal: {response}</color>");
                        }
                        else
                        {
                            Debug.LogError($"<color=red>[GameManager] 5. FAILED! Score submission rejected: {response}</color>");
                        }
                    });
                }
                else
                {
                    Debug.LogWarning("<color=orange>[GameManager] No active session ID found. Skipping submission.</color>");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"<color=red>[GameManager] SILENT CRASH CAUGHT! The code broke here:\n{ex.Message}\n{ex.StackTrace}</color>");
            }
        }

        public void OnPlayAgainClicked()
        {
            // NEW: block re-entrant clicks while a restart is already in flight
            if (isRestarting)
            {
                Debug.LogWarning("<color=orange>[GameManager] 'Play Again' already processing. Ignoring duplicate click.</color>");
                return;
            }
            isRestarting = true; // NEW

            Debug.Log("<color=cyan>[GameManager] 'Play Again' clicked. Requesting fast restart via postMessage...</color>");

            if (TechJuego.GetOut.Network.ApiManager.Instance == null)
            {
                Debug.LogError("<color=red>[GameManager] FATAL: ApiManager is missing!</color>");
                isRestarting = false; // NEW: let them retry since nothing was actually sent
                return;
            }

            // Fast path: postMessage REQUEST_RESTART -> portal replies RESTART_GAME
            // -> StartPortalListener forwards it to OnPortalRestart, which still
            // does one GET /game/init call before loading the Game scene, but skips
            // the authenticated GET /game/session round-trip this used to start with.
            TechJuego.GetOut.Network.ApiManager.Instance.RequestFastRestart();

            // NOTE: isRestarting is intentionally never reset back to false here.
            // A successful restart calls SceneManager.LoadSceneAsync("Game"), which
            // tears down this GameManager instance; the new one runs Awake() fresh
            // and resets isRestarting = false there. If RequestFastRestart's WebGL
            // round-trip stalls or the portal never replies, the button stays
            // disabled (see InGameUi) until the scene actually reloads, which is
            // the correct behavior for "processing" — we don't want a second
            // request racing the first one.
        }
    }
}