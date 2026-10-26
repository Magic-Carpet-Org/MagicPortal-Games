using System;

namespace TunnelGame
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

    /// <summary>
    /// What the game needs from any web portal. GameManager talks only to this
    /// (through PortalManager.Current) and never to a specific portal.
    ///
    /// To add a new portal: write a class that implements this, then add its
    /// detection to PortalManager.Detect().
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

        /// <summary>Called once by PortalManager after this portal becomes active.</summary>
        void Init();

        /// <summary>Live score while a run is in progress. GameManager throttles calls.</summary>
        void ReportProgress(int score);

        /// <summary>Final score when a run ends.</summary>
        void SubmitFinalScore(int score);

        /// <summary>Called after a run ends so the portal can prepare the next one.</summary>
        void RequestRestart();

        /// <summary>
        /// Fetches the leaderboard. onResult gets null if the portal has no
        /// leaderboard or the request failed.
        /// </summary>
        void RequestLeaderboard(Action<PortalLeaderboardEntry[]> onResult);
    }
}
