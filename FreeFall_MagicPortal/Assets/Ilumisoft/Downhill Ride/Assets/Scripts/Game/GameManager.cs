using Ilumisoft.SceneManagement;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private IScoreSystem _scoreSystem;

    private IHighscoreSystem _highscoreSystem;

    private SceneManager _sceneManager;

    private EventManager _eventManager;

    private GameEvent _gameOverEvent;

    // Portal integration: last score value broadcast via SCORE_UPDATE, and
    // how big a change is required before broadcasting another one.
    private float _lastSentScore;
    private const float ScoreUpdateThreshold = 50f;

    private void Awake()
    {
        _eventManager = FindObjectOfType<EventManager>();
        _sceneManager = FindObjectOfType<SceneManager>();

        _scoreSystem = InterfaceUtilities.FindObjectOfType<IScoreSystem>();
        _highscoreSystem = InterfaceUtilities.FindObjectOfType<IHighscoreSystem>();
    }

    private void Start()
    {
        _gameOverEvent = _eventManager.GetEvent<GameOverEvent>();
        _gameOverEvent.AddListener(OnGameOver);

        _scoreSystem.ResetScore();
        _lastSentScore = 0f;

        Time.timeScale = 1.0f;

        // If we're running inside the portal with a real session, tell the
        // portal we're ready via GET /game/init.
        if (PortalBridge.Instance != null && ApiManager.Instance != null && PortalBridge.Instance.IsPortalSession)
        {
            ApiManager.Instance.InitializeGame(PortalBridge.Instance.CurrentSessionId, (success, response) =>
            {
                if (success)
                {
                    Debug.Log("[GameManager] Portal session initialized for player: " + response.player.username);
                }
                else
                {
                    Debug.LogWarning("[GameManager] Portal session init failed.");
                }
            });
        }
    }

    private void Update()
    {
        _scoreSystem.ModifyScore(5 * Time.deltaTime);

        // Let the portal know about score milestones, but don't spam a postMessage every frame.
        if (PortalBridge.Instance != null && Mathf.Abs(_scoreSystem.Score - _lastSentScore) >= ScoreUpdateThreshold)
        {
            _lastSentScore = _scoreSystem.Score;
            PortalBridge.Instance.SendScoreUpdate(Mathf.RoundToInt(_scoreSystem.Score));
        }
    }

    private void OnGameOver()
    {
        //GameOver should only be triggered once
        _gameOverEvent.RemoveListener(OnGameOver);

        //Stop game time
        Time.timeScale = 0.0f;

        //Update highscore
        if (_scoreSystem.Score > _highscoreSystem.Highscore)
        {
            _highscoreSystem.Highscore = _scoreSystem.Score;
        }

        // Report the match result to the portal.
        if (PortalBridge.Instance != null)
        {
            int finalScore = Mathf.RoundToInt(_scoreSystem.Score);

            PortalBridge.Instance.SendMatchComplete(finalScore);

            // Submit the authoritative, signed score over REST.
            if (ApiManager.Instance != null && PortalBridge.Instance.IsPortalSession)
            {
                ApiManager.Instance.SubmitScore(PortalBridge.Instance.CurrentSessionId, finalScore, (success, response) =>
                {
                    if (success)
                    {
                        Debug.Log("[GameManager] Score submitted. Personal best: " + response.is_personal_best);
                    }
                    else
                    {
                        Debug.LogWarning("[GameManager] Score submission failed.");
                    }
                });
            }

            // Ask for a fresh session now so it's ready by the time the player replays.
            PortalBridge.Instance.RequestRestart();
        }

        //Load game over scene after 1 second
        _sceneManager.LoadSceneDelayed("Game Over", delay: 1.0f);
    }
}