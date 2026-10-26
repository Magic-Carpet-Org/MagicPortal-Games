using System;
using UnityEngine;

namespace Kilimanjaro.Portals
{
    /// <summary>Used when there is no portal. Every call is a harmless no-op.</summary>
    public class NullPortal : IGamePortal
    {
        public string Name { get { return "None"; } }
        public PortalPlayer Player { get { return null; } }

#pragma warning disable 67
        public event Action Paused;
        public event Action Resumed;
#pragma warning restore 67

        public void Init(Action<bool> onDone) { if (onDone != null) onDone(true); }
        public void ReportProgress(int score) { }

        public void SubmitFinalScore(int score)
        {
            Debug.Log("[PortalKit] Final score " + score + " (no portal)");
        }

        public void RequestRestart() { }

        public void WaitForNextRun(Action onReady) { if (onReady != null) onReady(); }

        public void RequestLeaderboard(Action<PortalLeaderboardEntry[]> onResult)
        {
            if (onResult != null) onResult(null);
        }
    }
}
