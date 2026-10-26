using UnityEngine;
using System;
using System.Runtime.InteropServices;

namespace TunnelGame
{
    [Serializable]
    public class LagosPlayer
    {
        public string playerId;
        public string playerName;
        public string avatar;
        public string mode;     // "live" or "test" (sandbox)
    }

    [Serializable]
    public class LagosScoreResult
    {
        public int best;
        public bool accepted;
    }

    [Serializable]
    public class LagosLeaderboardEntry
    {
        public int rank;
        public string name;
        public int score;
    }

    [Serializable]
    public class LagosLeaderboard
    {
        public LagosLeaderboardEntry[] top;
    }

    /// <summary>
    /// Low-level Unity <-> JavaScript link for Lagos Life (Vatar SDK).
    /// The game never uses this directly - LagosLifePortal wraps it.
    ///
    /// PortalManager creates the "LagosLifeBridge" GameObject when needed; the
    /// .jslib delivers results to it by that exact name via SendMessage.
    /// Only client-side SDK calls are used, so no Lagos Life secret is in the build.
    /// </summary>
    public class LagosLifeBridge : MonoBehaviour
    {
        #region Native Bindings

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void LL_Init();
        [DllImport("__Internal")] private static extern void LL_SubmitScore(int score);
        [DllImport("__Internal")] private static extern void LL_GetLeaderboard();
#endif

        #endregion

        #region Events

        public event Action Paused;
        public event Action Resumed;
        public event Action<LagosScoreResult> ScoreSubmitted;
        public event Action<LagosLeaderboard> LeaderboardReceived;
        public event Action LeaderboardFailed;

        #endregion

        #region Member Variables

        public const string ObjectName = "LagosLifeBridge";

        private bool isConnected;
        private LagosPlayer player;
        private Action onConnected;
        private Action<string> onConnectFailed;

        #endregion

        #region Properties

        public bool IsConnected { get { return isConnected; } }
        public LagosPlayer Player { get { return player; } }

        #endregion

        #region Unity Methods

        private void Awake()
        {
            // The .jslib finds us by exact name, and SendMessage needs us active.
            gameObject.name = ObjectName;

            if (transform.parent != null)
            {
                transform.SetParent(null, false);
            }

            DontDestroyOnLoad(gameObject);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads the Vatar SDK (only if we're inside an iframe) and calls
        /// Vatar.init(). Exactly one of the callbacks fires.
        /// </summary>
        public void Connect(Action onSuccess, Action<string> onFailure)
        {
            onConnected = onSuccess;
            onConnectFailed = onFailure;

#if UNITY_WEBGL && !UNITY_EDITOR
            LL_Init();
#else
            OnLagosInitFailed("not_webgl");
#endif
        }

        /// <summary>
        /// Lagos Life keeps the player's best and allows one submission per
        /// 20 s; the .jslib queues the best score if we're inside that window.
        /// </summary>
        public void SubmitScore(int score)
        {
            if (!isConnected) return;

#if UNITY_WEBGL && !UNITY_EDITOR
            LL_SubmitScore(score);
#endif
        }

        public void RequestLeaderboard()
        {
            if (!isConnected)
            {
                if (LeaderboardFailed != null) LeaderboardFailed();
                return;
            }

#if UNITY_WEBGL && !UNITY_EDITOR
            LL_GetLeaderboard();
#endif
        }

        #endregion

        #region Incoming (called by LagosLifeBridge.jslib via SendMessage - keep names)

        public void OnLagosInit(string json)
        {
            player = JsonUtility.FromJson<LagosPlayer>(json);
            isConnected = true;

            Debug.Log(string.Format("[LagosLifeBridge] Connected ({0}) as {1}", player.mode, player.playerName));

            Action cb = onConnected;
            onConnected = null;
            onConnectFailed = null;
            if (cb != null) cb();
        }

        public void OnLagosInitFailed(string code)
        {
            isConnected = false;
            Debug.Log("[LagosLifeBridge] Not on Lagos Life (" + code + ")");

            Action<string> cb = onConnectFailed;
            onConnected = null;
            onConnectFailed = null;
            if (cb != null) cb(code);
        }

        public void OnLagosPause(string _)
        {
            if (Paused != null) Paused();
        }

        public void OnLagosResume(string _)
        {
            if (Resumed != null) Resumed();
        }

        public void OnLagosScoreResult(string json)
        {
            LagosScoreResult result = JsonUtility.FromJson<LagosScoreResult>(json);
            Debug.Log(string.Format("[LagosLifeBridge] Score sent. Best={0} accepted={1}", result.best, result.accepted));

            if (ScoreSubmitted != null) ScoreSubmitted(result);
        }

        public void OnLagosScoreFailed(string code)
        {
            Debug.LogWarning("[LagosLifeBridge] Score submit failed: " + code);
        }

        public void OnLagosLeaderboard(string json)
        {
            LagosLeaderboard board = JsonUtility.FromJson<LagosLeaderboard>(json);
            if (LeaderboardReceived != null) LeaderboardReceived(board);
        }

        public void OnLagosLeaderboardFailed(string code)
        {
            Debug.LogWarning("[LagosLifeBridge] Leaderboard failed: " + code);
            if (LeaderboardFailed != null) LeaderboardFailed();
        }

        #endregion
    }
}
