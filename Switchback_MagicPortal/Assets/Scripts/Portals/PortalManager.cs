using System;
using UnityEngine;

namespace TunnelGame
{
    /// <summary>
    /// Picks the portal the game is running on and exposes it as
    /// PortalManager.Current. The rest of the game only talks to Current.
    ///
    /// Detection order:
    ///   1. MagicGames sessionId in the URL      -> MagicGamesPortal
    ///   2. Inside an iframe and Vatar.init() ok -> LagosLifePortal
    ///   3. Otherwise                            -> MagicGamesPortal if a
    ///      PortalBridge exists (same as the old behaviour), else NullPortal
    ///
    /// Lagos Life detection is asynchronous, so Current starts on the fallback
    /// and switches over when Vatar.init() answers. PortalChanged fires then.
    ///
    /// No setup needed: this creates itself at startup.
    /// </summary>
    public class PortalManager : MonoBehaviour
    {
        #region Member Variables

        private static PortalManager instance;
        private static IGamePortal current = new NullPortal();

        private bool isPaused;
        private float timeScaleBeforePause = 1f;

        #endregion

        #region Events

        /// <summary>Raised whenever Current changes (e.g. after Lagos Life is detected).</summary>
        public static event Action<IGamePortal> PortalChanged;

        #endregion

        #region Properties

        /// <summary>The active portal. Never null.</summary>
        public static IGamePortal Current { get { return current; } }

        public static bool IsPaused { get { return instance != null && instance.isPaused; } }

        #endregion

        #region Unity Methods

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoCreate()
        {
            if (instance == null)
            {
                new GameObject("PortalManager").AddComponent<PortalManager>();
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
        }

        private void Start()
        {
            // Start (not Awake) so PortalBridge has already read its sessionId.
            Detect();
        }

        #endregion

        #region Detection

        private void Detect()
        {
            PortalBridge magicBridge = PortalBridge.Instance;

            // 1. MagicGames with a real session.
            if (magicBridge != null && magicBridge.IsPortalSession)
            {
                SetCurrent(new MagicGamesPortal(magicBridge));
                return;
            }

            // 3. Fallback while we wait (matches what the game did before adapters).
            SetCurrent(magicBridge != null ? (IGamePortal)new MagicGamesPortal(magicBridge) : new NullPortal());

            // 2. Try Lagos Life. The .jslib bails out immediately if we're not in an iframe.
            LagosLifeBridge lagosBridge = new GameObject(LagosLifeBridge.ObjectName).AddComponent<LagosLifeBridge>();

            lagosBridge.Connect(
                () => SetCurrent(new LagosLifePortal(lagosBridge)),
                code => Destroy(lagosBridge.gameObject));
        }

        private void SetCurrent(IGamePortal portal)
        {
            if (current != null)
            {
                current.Paused -= HandlePaused;
                current.Resumed -= HandleResumed;
            }

            // Never leave the game frozen by a portal we're switching away from.
            HandleResumed();

            current = portal;
            current.Paused += HandlePaused;
            current.Resumed += HandleResumed;

            Debug.Log("[PortalManager] Portal: " + current.Name);

            current.Init();

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
        }

        private void HandleResumed()
        {
            if (!isPaused) return;

            isPaused = false;
            Time.timeScale = timeScaleBeforePause;
            AudioListener.pause = false;
        }

        #endregion
    }
}
