using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kilimanjaro.Portals
{
    /// <summary>
    /// Kilimanjaro PortalKit entry point. Creates itself at startup, works out
    /// which portal the game is running on, and exposes it as Current.
    ///
    /// A game only needs two calls:
    ///   PortalManager.ReportProgress(score);   // every frame/whenever - throttled here
    ///   PortalManager.GameOver(finalScore);    // once when a run ends
    /// Optional, for games with a loading screen or a Play Again button:
    ///   PortalManager.WhenReady(ok => ...);    // portal found and session checked
    ///   PortalManager.PlayAgain(() => ...);    // next run may start (reload scene here)
    ///
    /// Detection order:
    ///   1. MagicGames sessionId in the URL      -> MagicGamesPortal
    ///   2. Inside an iframe and Vatar.init() ok -> LagosLifePortal
    ///   3. Otherwise                            -> MagicGamesPortal without a
    ///      session (postMessage only, REST skipped)
    /// Lagos Life answers asynchronously, so Current starts on (3) and switches
    /// when it connects; PortalChanged fires each time Current changes.
    /// </summary>
    public class PortalManager : MonoBehaviour
    {
        #region Member Variables

        private static PortalManager instance;
        private static IGamePortal current = new NullPortal();

        private PortalSettings settings;
        private MagicGamesBridge magicBridge;
        private MagicGamesApi magicApi;

        private int lastProgressScore;

        private bool isReady;
        private bool readyOk;
        private readonly List<Action<bool>> readyWaiters = new List<Action<bool>>();
        private bool isPaused;
        private float timeScaleBeforePause = 1f;

        #endregion

        #region Events

        /// <summary>Raised whenever Current changes (e.g. once Lagos Life is detected).</summary>
        public static event Action<IGamePortal> PortalChanged;

        /// <summary>Raised when a portal pauses (true) or resumes (false) the game.
        /// Time.timeScale and AudioListener.pause are already handled for you.</summary>
        public static event Action<bool> PauseChanged;

        #endregion

        #region Properties

        /// <summary>The active portal. Never null.</summary>
        public static IGamePortal Current { get { return current; } }

        public static bool IsPaused { get { return instance != null && instance.isPaused; } }

        #endregion

        #region Game API

        /// <summary>
        /// Live score during a run. Safe to call every frame: only sends when the
        /// score has moved by PortalSettings.progressStep since the last send.
        /// </summary>
        public static void ReportProgress(int score)
        {
            if (instance == null) return;

            int step = Mathf.Max(1, instance.settings.progressStep);

            if (Mathf.Abs(score - instance.lastProgressScore) >= step)
            {
                instance.lastProgressScore = score;
                current.ReportProgress(score);
            }
        }

        /// <summary>
        /// Call once when a run ends. Submits the final score and lets the portal
        /// prepare the next run.
        /// </summary>
        public static void GameOver(int finalScore)
        {
            if (instance != null) instance.lastProgressScore = 0;

            current.SubmitFinalScore(finalScore);
            current.RequestRestart();
        }

        /// <summary>
        /// Calls back once the portal is known and its session has been checked
        /// (ok = false only if MagicGames rejected the session). Use it to hold a
        /// loading screen. Calls back immediately if that has already happened.
        /// </summary>
        public static void WhenReady(Action<bool> callback)
        {
            if (callback == null) return;

            if (instance == null)
            {
                callback(true);
                return;
            }

            if (instance.isReady) callback(instance.readyOk);
            else instance.readyWaiters.Add(callback);
        }

        /// <summary>
        /// Call from a Play Again button. onReady runs once the next run may start
        /// (on MagicGames, after the new session arrives; elsewhere, immediately).
        /// Reload the game scene or reset the level inside onReady.
        /// </summary>
        public static void PlayAgain(Action onReady)
        {
            if (instance != null) instance.lastProgressScore = 0;

            current.WaitForNextRun(onReady);
        }

        /// <summary>Fetches the leaderboard from the current portal (null if unsupported).</summary>
        public static void RequestLeaderboard(Action<PortalLeaderboardEntry[]> onResult)
        {
            current.RequestLeaderboard(onResult);
        }

        #endregion

        #region Unity Methods

        // Clears statics so it also works with "Enter Play Mode Options" (no domain reload).
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            instance = null;
            current = new NullPortal();
            PortalChanged = null;
            PauseChanged = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoCreate()
        {
            if (instance == null)
            {
                new GameObject("PortalKit").AddComponent<PortalManager>();
            }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            settings = PortalSettings.Load();

            // MagicGames pieces live on their own objects (the .jslib finds the bridge by name).
            magicBridge = new GameObject(MagicGamesBridge.ObjectName).AddComponent<MagicGamesBridge>();
            magicApi = new GameObject("PortalKit_MagicGamesApi").AddComponent<MagicGamesApi>();
        }

        private void Start()
        {
            Detect();
        }

        #endregion

        #region Detection

        private void Detect()
        {
            MagicGamesPortal magicGames = new MagicGamesPortal(magicBridge, magicApi);

            // 1. A real MagicGames session wins - ready once the backend accepts it.
            if (magicBridge.IsPortalSession || !settings.enableLagosLife)
            {
                SetCurrent(magicGames, MarkReady);
                return;
            }

            // 3. Session-less MagicGames while Lagos Life is checked.
            SetCurrent(magicGames, null);

            // 2. Try Lagos Life (the .jslib bails out at once if we're not in an iframe).
            LagosLifeBridge lagosBridge = new GameObject(LagosLifeBridge.ObjectName).AddComponent<LagosLifeBridge>();

            lagosBridge.Connect(
                () => SetCurrent(new LagosLifePortal(lagosBridge), MarkReady),
                code =>
                {
                    Destroy(lagosBridge.gameObject);
                    MarkReady(true);
                });
        }

        private void MarkReady(bool ok)
        {
            if (isReady) return;

            isReady = true;
            readyOk = ok;

            Debug.Log("[PortalManager] Ready on " + current.Name + (ok ? "" : " (session rejected)"));

            Action<bool>[] waiters = readyWaiters.ToArray();
            readyWaiters.Clear();

            foreach (Action<bool> waiter in waiters) waiter(ok);
        }

        private void SetCurrent(IGamePortal portal, Action<bool> onInitDone)
        {
            current.Paused -= HandlePaused;
            current.Resumed -= HandleResumed;

            // Never leave the game frozen by a portal we're switching away from.
            HandleResumed();

            current = portal;
            current.Paused += HandlePaused;
            current.Resumed += HandleResumed;

            Debug.Log("[PortalManager] Portal: " + current.Name);

            current.Init(ok => { if (onInitDone != null) onInitDone(ok); });

            if (PortalChanged != null) PortalChanged(current);
        }

        #endregion

        #region Pause / Resume (shared by every portal)

        private void HandlePaused()
        {
            if (isPaused) return;

            isPaused = true;
            timeScaleBeforePause = Time.timeScale;
            Time.timeScale = 0f;
            AudioListener.pause = true;

            if (PauseChanged != null) PauseChanged(true);
        }

        private void HandleResumed()
        {
            if (!isPaused) return;

            isPaused = false;
            Time.timeScale = timeScaleBeforePause;
            AudioListener.pause = false;

            if (PauseChanged != null) PauseChanged(false);
        }

        #endregion
    }
}
