using Ilumisoft.SceneManagement;
using Kilimanjaro.Portals;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private IScoreSystem _scoreSystem;

    private IHighscoreSystem _highscoreSystem;

    private SceneManager _sceneManager;

    private EventManager _eventManager;

    private GameEvent _gameOverEvent;

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

        Time.timeScale = 1.0f;

        // Portal detection and session init (MagicGames / Lagos Life) are handled by PortalKit.
    }

    private void Update()
    {
        _scoreSystem.ModifyScore(5 * Time.deltaTime);

        // Live score for the portal (PortalKit throttles it via PortalSettings.progressStep).
        PortalManager.ReportProgress(Mathf.RoundToInt(_scoreSystem.Score));
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

        // Submit the final score to whichever portal we're on (MagicGames / Lagos Life)
        // and start fetching the next MagicGames session for the replay.
        PortalManager.GameOver(Mathf.RoundToInt(_scoreSystem.Score));

        //Load game over scene after 1 second
        _sceneManager.LoadSceneDelayed("Game Over", delay: 1.0f);
    }
}