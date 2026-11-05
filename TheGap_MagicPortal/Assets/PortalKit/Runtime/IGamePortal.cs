using System;

namespace Kilimanjaro.Portals
{
    [Serializable]
    public class PortalPlayer
    {
        public string id;
        public string name;
        public string avatar;
    }

    [Serializable]
    public class PortalLeaderboardEntry
    {
        public int rank;
        public string name;
        public int score;
    }

    [Serializable]
    public class PortalPurchaseResult
    {
        /// <summary>True only when the item was paid for AND verified/delivered server-side. Grant it now.</summary>
        public bool success;
        public string productId;

        /// <summary>
        /// Empty on success. Otherwise e.g. "cancelled" (player closed the pay sheet),
        /// "busy", "payment_failed", "not_for_sale", "owned", "not_supported",
        /// "claim_failed" (paid but our server couldn't verify yet - kept for retry).
        /// </summary>
        public string error;

        public bool Cancelled { get { return error == "cancelled"; } }
    }

    /// <summary>
    /// What a game needs from any web portal. Games normally don't use this
    /// directly - they call the static helpers on PortalManager.
    ///
    /// To add a new portal: implement this interface, then add its detection
    /// to PortalManager.Detect().
    /// </summary>
    public interface IGamePortal
    {
        /// <summary>Short name for logs, e.g. "MagicGames", "LagosLife", "None".</summary>
        string Name { get; }

        /// <summary>The signed-in player, or null if the portal doesn't provide one.</summary>
        PortalPlayer Player { get; }

        /// <summary>Raised when the portal wants the game frozen (timers + sound).</summary>
        event Action Paused;

        /// <summary>Raised when the portal lets the game continue.</summary>
        event Action Resumed;

        /// <summary>
        /// Called once by PortalManager after this portal becomes active.
        /// Must call onDone exactly once: true if the game may start.
        /// </summary>
        void Init(Action<bool> onDone);

        /// <summary>Live score while a run is in progress (already throttled by PortalManager).</summary>
        void ReportProgress(int score);

        /// <summary>Final score when a run ends.</summary>
        void SubmitFinalScore(int score);

        /// <summary>Called after a run ends so the portal can prepare the next one.</summary>
        void RequestRestart();

        /// <summary>
        /// Calls onReady once the next run may start (e.g. MagicGames has sent a
        /// new session). Portals with nothing to wait for call it immediately.
        /// </summary>
        void WaitForNextRun(Action onReady);

        /// <summary>True if this portal can sell items.</summary>
        bool SupportsPurchases { get; }

        /// <summary>
        /// True if the player already paid for productId but it hasn't been
        /// delivered yet (e.g. the game reloaded mid-purchase). The next
        /// Purchase(productId) uses it instead of charging again.
        /// </summary>
        bool HasUnclaimedPurchase(string productId);

        /// <summary>
        /// Buys productId (opens the portal's own pay sheet). onDone fires once;
        /// grant the item only if result.success is true.
        /// </summary>
        void Purchase(string productId, Action<PortalPurchaseResult> onDone);

        /// <summary>Fetches the leaderboard. onResult gets null if unsupported or failed.</summary>
        void RequestLeaderboard(Action<PortalLeaderboardEntry[]> onResult);
    }
}