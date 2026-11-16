using UnityEngine;
using System;
using System.Runtime.InteropServices;

namespace Kilimanjaro.Portals
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
    /// PortalManager creates the "PortalKit_LagosLife" GameObject when needed; the
    /// .jslib delivers results to it by that exact name via SendMessage.
    /// Only client-side SDK calls are used, so no Lagos Life secret is in the build.
    /// </summary>
    public class LagosLifeBridge : MonoBehaviour
    {
        #region Native Bindings

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void PKLL_Init();
        [DllImport("__Internal")] private static extern void PKLL_SubmitScore(int score);
        [DllImport("__Internal")] private static extern void PKLL_GetLeaderboard();
        [DllImport("__Internal")] private static extern void PKLL_Purchase(string productId, string claimUrl);
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

        public const string ObjectName = "PortalKit_LagosLife";

        private bool isConnected;
        private LagosPlayer player;
        private Action onConnected;
        private Action<string> onConnectFailed;

        private Action<PortalPurchaseResult> onPurchaseDone;
        private string[] unclaimedProductIds = new string[0];

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
            PKLL_Init();
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
            PKLL_SubmitScore(score);
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
            PKLL_GetLeaderboard();
#endif
        }

        /// <summary>
        /// Opens the Lagos Life pay sheet for productId (or reuses an unclaimed paid
        /// one), then has claimUrl (our server) verify and deliver it.
        /// onDone fires exactly once.
        /// </summary>
        public void Purchase(string productId, string claimUrl, Action<PortalPurchaseResult> onDone)
        {
            if (!isConnected)
            {
                if (onDone != null) onDone(new PortalPurchaseResult { success = false, productId = productId, error = "not_ready" });
                return;
            }

            if (onPurchaseDone != null)
            {
                if (onDone != null) onDone(new PortalPurchaseResult { success = false, productId = productId, error = "busy" });
                return;
            }

            onPurchaseDone = onDone;

#if UNITY_WEBGL && !UNITY_EDITOR
            PKLL_Purchase(productId, claimUrl);
#else
            OnLagosPurchaseResult(JsonUtility.ToJson(new PortalPurchaseResult { success = false, productId = productId, error = "not_webgl" }));
#endif
        }

        public bool HasUnclaimed(string productId)
        {
            return Array.IndexOf(unclaimedProductIds, productId) >= 0;
        }

        #endregion

        #region Incoming (called by LagosLifeBridge.jslib via SendMessage - keep names)

        public void OnLagosPurchaseResult(string json)
        {
            PortalPurchaseResult result = JsonUtility.FromJson<PortalPurchaseResult>(json);

            if (result.success)
                Debug.Log("[LagosLifeBridge] Purchase delivered: " + result.productId);
            else
                Debug.Log("[LagosLifeBridge] Purchase not completed (" + result.productId + "): " + result.error);

            Action<PortalPurchaseResult> cb = onPurchaseDone;
            onPurchaseDone = null;
            if (cb != null) cb(result);
        }

        [Serializable]
        private class UnclaimedList { public string[] productIds = new string[0]; }

        public void OnLagosUnclaimed(string json)
        {
            UnclaimedList list = JsonUtility.FromJson<UnclaimedList>(json);
            unclaimedProductIds = (list != null && list.productIds != null) ? list.productIds : new string[0];

            if (unclaimedProductIds.Length > 0)
                Debug.Log("[LagosLifeBridge] Paid but not yet delivered: " + string.Join(", ", unclaimedProductIds));
        }

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
