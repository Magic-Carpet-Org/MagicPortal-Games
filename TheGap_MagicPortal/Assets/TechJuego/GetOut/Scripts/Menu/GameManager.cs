using UnityEngine;
using UnityEngine.SceneManagement;
using Kilimanjaro.Portals;

namespace TechJuego.GetOut
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        [Header("Second Chance (Lagos Life purchase)")]
        [Tooltip("Product id - must match the product created in the Lagos Life developer dashboard.")]
        public string secondChanceProductId = "second_chance";

        [Tooltip("How many Second Chances a player may buy in one run.")]
        public int maxSecondChancesPerRun = 1;

        private int secondChancesUsed;

        [Header("Game Elements")]
        public Ball m_Ball;
        public Effect m_Effect;
        public GameObject m_LooseEffect;

        [Header("Game State")]
        private int _encryptedScore;
        private int _secretKey = 8492;
        private bool isRestarting = false; // guards against duplicate "Play Again" clicks

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
            isRestarting = false; // fresh scene load always starts unlocked
            secondChancesUsed = 0;
            Score = 0;

            // Editor testing against a real MagicGames session now lives in
            // PortalSettings > Editor Test Session Id (PortalKit).

            if (Camera.main != null && m_BgColor.Length > 0)
            {
                Camera.main.backgroundColor = m_BgColor[UnityEngine.Random.Range(0, m_BgColor.Length)];
            }
        }

        private void Start()
        {
            DataHandler.Instance.gameState = GameState.CreateLevel;
        }

        // ==========================================
        // SECOND CHANCE
        // ==========================================

        /// <summary>True if the ball just died and the player may buy a Second Chance.</summary>
        public bool CanOfferSecondChance()
        {
            return PortalManager.SupportsPurchases
                && !scoreSubmitted
                && secondChancesUsed < maxSecondChancesPerRun;
        }

        /// <summary>Call only after a successful purchase: resumes the same run, score kept.</summary>
        public void ContinueWithSecondChance()
        {
            secondChancesUsed++;
            isGameEnded = false;

            Debug.Log($"<color=green>[GameManager] Second Chance used ({secondChancesUsed}/{maxSecondChancesPerRun}). Continuing at score {Score}.</color>");

            DataHandler.Instance.gameState = GameState.Playing;
        }

        /// <summary>Player skipped or the offer timed out: normal Game Over.</summary>
        public void DeclineSecondChance()
        {
            DataHandler.Instance.gameState = GameState.LevelFail;
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

                Debug.Log($"<color=yellow>[GameManager] 3. Match Over on {PortalManager.Current.Name}. Final Score: {finalScore}</color>");

                // Submits the score to whichever portal we're on (MagicGames / Lagos Life)
                // and starts fetching the next MagicGames session in the background.
                PortalManager.GameOver(finalScore);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"<color=red>[GameManager] SILENT CRASH CAUGHT! The code broke here:\n{ex.Message}\n{ex.StackTrace}</color>");
            }
        }

        public void OnPlayAgainClicked()
        {
            // block re-entrant clicks while a restart is already in flight
            if (isRestarting)
            {
                Debug.LogWarning("<color=orange>[GameManager] 'Play Again' already processing. Ignoring duplicate click.</color>");
                return;
            }
            isRestarting = true;

            Debug.Log("<color=cyan>[GameManager] 'Play Again' clicked. Waiting for the portal's next run...</color>");

            // MagicGames: waits for the new session (usually already fetched by
            // GameOver). Lagos Life / no portal: runs straight away.
            PortalManager.PlayAgain(() => SceneManager.LoadSceneAsync("Game"));

            // isRestarting is intentionally never reset here. Loading the Game scene
            // tears down this GameManager; the new one resets it in Awake(). If the
            // MagicGames portal never replies, the button stays disabled (see InGameUi)
            // rather than racing a second request against the first.
        }
    }
}