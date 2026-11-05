using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Kilimanjaro.Portals;

public class UIController : MonoBehaviour
{
    [SerializeField]
    GameObject MenuPanel;

    [SerializeField]
    GameObject PausePanel;

    [SerializeField]
    Text highScoreTextPause;

    [SerializeField]
    Text scoreTextGameover;

    [SerializeField]
    Text highScoreTextGameover;

    [SerializeField]
    GameObject GameoverPanel;

    [SerializeField]
    Text scoreText;

    [SerializeField]
    Text TutorialText;

    public int ScoreFactor;

    int playScore;

    Color TutorialTextColor;

    ObstacleSpawner _spawner;

    BackgroundManager _backgroundManager;

    // Guards against double taps on Restart while waiting for the portal.
    bool restarting;

    // Set by PlayAgainNow(): after the scene reloads, skip the menu and start playing.
    static bool startImmediatelyAfterReload;

    private void Start()
    {
        if (startImmediatelyAfterReload)
        {
            startImmediatelyAfterReload = false;
            StartGame();
        }
    }

    private void OnEnable()
    {
        _spawner = FindObjectOfType<ObstacleSpawner>();
        _backgroundManager = FindObjectOfType<BackgroundManager>();
        Time.timeScale = 0f;
    }

    private void Update()
    {
        //Keep second condition, if your menu & gameplay are in the same scene
        if (Input.GetKeyDown(KeyCode.Escape) && TheGlobals.playingMode)
        {
            //uncomment this when you are ready with your exit panel
            OnBackClick();
        }
    }

    void OnBackClick()
    {
        TheGlobals.sManager.allAudio[0].Play();
        if (!PausePanel.activeInHierarchy)
        {
            PauseGame();
        }
        else
        {
            ResumeGame();
        }
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause && TheGlobals.playingMode)
        {
            PauseGame();
        }
    }

    IEnumerator FadeoutTutorial()
    {
        yield return new WaitForSeconds(0.01f);
        TutorialTextColor = TutorialText.color;
        TutorialTextColor.a -= 0.005f;
        TutorialText.color = TutorialTextColor;

        if (TutorialText.color.a > 0f)
        {
            StartCoroutine(FadeoutTutorial());
        }
    }

    public void StartGame()
    {
        GarbageCollectionManager.CollectGarbage();
        Time.timeScale = 1f;
        MenuPanel.SetActive(false);
        TutorialText.gameObject.SetActive(true);
        _spawner.Spawner();
        TheGlobals.playingMode = true;
        StartCoroutine(FadeoutTutorial());
        TheGlobals.sManager.bgMusic.volume = 0.6f;

        if (_backgroundManager != null)
        {
            _backgroundManager.SetRandomBackground();
        }
    }

    public void PauseGame()
    {
        GarbageCollectionManager.CollectGarbage();
        PausePanel.SetActive(true);
        highScoreTextPause.text = "High Score : " + GamePreferences.HighScore.ToString();
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        GarbageCollectionManager.CollectGarbage();
        PausePanel.SetActive(false);
        StartCoroutine(WaitToResume());
    }

    IEnumerator WaitToResume()
    {
        yield return new WaitForEndOfFrame();
        Time.timeScale = 1f;
    }

    public void DisplayScore()
    {
        playScore = playScore + ScoreFactor;
        scoreText.text = playScore.ToString();

        // Live score for the portal (PortalKit throttles it via PortalSettings.progressStep)
        PortalManager.ReportProgress(playScore);
    }

    public void GameOver()
    {
        GarbageCollectionManager.CollectGarbage();
        TheGlobals.playingMode = false;
    }

    public void LoadGameover()
    {
        GameoverPanel.SetActive(true);

        if (playScore > GamePreferences.HighScore)
        {
            GamePreferences.HighScore = playScore;
        }

        scoreTextGameover.text = "Score : " + playScore.ToString();
        highScoreTextGameover.text = "High Score : " + GamePreferences.HighScore.ToString();

        // Submit the final score to whichever portal we're on (MagicGames / Lagos Life)
        // and start fetching the next MagicGames session in the background.
        PortalManager.GameOver(playScore);
    }

    // ==========================================
    // GAME OVER BUTTONS (wire these in the Button's On Click list)
    // ==========================================

    /// <summary>Game Over -> back to the main menu.</summary>
    public void GoToMenu()
    {
        ReloadScene(false);
    }

    /// <summary>Game Over -> start a new run straight away (skips the menu).</summary>
    public void PlayAgainNow()
    {
        ReloadScene(true);
    }

    /// <summary>Kept for buttons already wired to it: same as GoToMenu.</summary>
    public void RestartGame()
    {
        ReloadScene(false);
    }

    void ReloadScene(bool startImmediately)
    {
        if (restarting) return;
        restarting = true;

        // MagicGames: waits for the new session (usually already fetched at Game Over).
        // Lagos Life / no portal: reloads straight away.
        PortalManager.PlayAgain(() =>
        {
            startImmediatelyAfterReload = startImmediately;
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        });
    }


}