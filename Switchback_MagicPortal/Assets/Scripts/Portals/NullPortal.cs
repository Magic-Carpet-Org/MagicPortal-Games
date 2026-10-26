using System;
using UnityEngine;

namespace TunnelGame
{
    /// <summary>
    /// Used in the Editor, standalone builds, and when the game is opened
    /// outside any portal. Every call is a harmless no-op.
    /// </summary>
    public class NullPortal : IGamePortal
    {
        public string Name { get { return "None"; } }
        public PortalPlayer Player { get { return null; } }

#pragma warning disable 67 // events are part of the contract but never raised here
        public event Action Paused;
        public event Action Resumed;
#pragma warning restore 67

        public void Init() { }

        public void ReportProgress(int score) { }

        public void SubmitFinalScore(int score)
        {
            Debug.Log("[NullPortal] Final score " + score + " (no portal)");
        }

        public void RequestRestart() { }

        public void RequestLeaderboard(Action<PortalLeaderboardEntry[]> onResult)
        {
            if (onResult != null) onResult(null);
        }
    }
}
