using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

        // Push the real-time score to the iframe portal
        if (PortalManager.Instance != null)
        {
            PortalManager.Instance.SendScoreUpdate(playScore);
        }
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

        // Notify the portal and submit the REST POST payload
        if (PortalManager.Instance != null)
        {
            PortalManager.Instance.SendMatchComplete(playScore);
        }
    }

    public void RestartGame()
    {
        // Ask the portal for a new session ID via postMessage
        if (PortalManager.Instance != null)
        {
            PortalManager.Instance.RequestRestart();
        }
        else
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }


}