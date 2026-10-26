using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TechJuego.GetOut.Utils;
using TechJuego.GetOut.Sound;
using UnityEngine.SceneManagement;

namespace TechJuego.GetOut
{
    public class InGameUi : MonoBehaviour
    {
        [SerializeField] private GameObject m_HUDUI;
        [SerializeField] private Button m_SettingButton;
        [SerializeField] private TextMeshProUGUI m_CurrentScore;
        [SerializeField] private InGameSettingsPanel m_InGameSettingsPanel;

        [Header("Game Over UI Settings")]
        // The canvas to hold your new Unity-controlled "Play Again" button
        [SerializeField] private GameObject m_LevelFailPanel;

        [Tooltip("Text element for the final score on the Game Over screen")]
        [SerializeField] private TextMeshProUGUI m_GameOverScoreText;

        [Tooltip("Text element for the best score on the Game Over screen")]
        [SerializeField] private TextMeshProUGUI m_GameOverBestScoreText;

        [Tooltip("The 'Play Again' button on the Game Over screen. Wire this here instead of hooking GameManager.OnPlayAgainClicked() directly in the Inspector, so the click can be gated in one place.")]
        [SerializeField] private Button m_PlayAgainButton; // NEW

        private void Awake()
        {
            m_InGameSettingsPanel.gameObject.SetActive(false);

            // Hide the Game Over panel when the level starts
            if (m_LevelFailPanel != null)
            {
                m_LevelFailPanel.SetActive(false);
            }

            m_HUDUI.gameObject.SetActive(true);
        }

        private void OnEnable()
        {
            GameEvents.OnLevelFail += GameEvents_OnLevelFail;
            GameEvents.OnLevelUpdate += GameEvents_OnLevelUpdate;
            Utility.SetButton(m_SettingButton, OnClickSettingButton);

            // NEW: route Play Again through this class so we can disable it on click
            if (m_PlayAgainButton != null)
            {
                Utility.SetButton(m_PlayAgainButton, OnClickPlayAgainButton);
                m_PlayAgainButton.interactable = true; // make sure it's armed whenever the UI (re)enables
            }
        }

        private void OnDisable()
        {
            GameEvents.OnLevelFail -= GameEvents_OnLevelFail;
            GameEvents.OnLevelUpdate -= GameEvents_OnLevelUpdate;
        }

        void OnClickSettingButton()
        {
            m_InGameSettingsPanel.gameObject.SetActive(true);
            SoundEvents.OnPlaySingleShotSound?.Invoke("Click");
        }

        // NEW: disables the button immediately so a fast double-click / double-tap
        // can't fire OnPlayAgainClicked() twice while the restart is in flight.
        void OnClickPlayAgainButton()
        {
            if (m_PlayAgainButton != null)
            {
                m_PlayAgainButton.interactable = false;
            }

            SoundEvents.OnPlaySingleShotSound?.Invoke("Click");
            GameManager.Instance.OnPlayAgainClicked();
        }

        private void GameEvents_OnLevelUpdate()
        {
            GameManager.Instance.Score += 1;
            m_CurrentScore.text = GameManager.Instance.Score.ToString();
            Kilimanjaro.Portals.PortalManager.ReportProgress(GameManager.Instance.Score);
        }

        private void GameEvents_OnLevelFail()
        {
            // NEW: re-arm the button for this fresh Game Over screen
            // (relevant if this InGameUi instance ever gets reused/re-shown
            // without a full scene reload; harmless no-op otherwise)
            if (m_PlayAgainButton != null)
            {
                m_PlayAgainButton.interactable = true;
            }

            // 1. Calculate and save the Highscore locally before displaying it
            if (GameManager.Instance.Score > DataHandler.Instance.Highscore)
            {
                DataHandler.Instance.Highscore = GameManager.Instance.Score;
            }

            // 2. Show the Unity Game Over UI
            if (m_LevelFailPanel != null)
            {
                m_LevelFailPanel.SetActive(true);

                // 3. Update the text fields!
                if (m_GameOverScoreText != null)
                    m_GameOverScoreText.text = GameManager.Instance.Score.ToString();

                if (m_GameOverBestScoreText != null)
                    m_GameOverBestScoreText.text = DataHandler.Instance.Highscore.ToString();
            }

            // 4. Instantly fire the score to the portal in the background
            GameManager.Instance.TriggerGameOver();
        }
    }
}