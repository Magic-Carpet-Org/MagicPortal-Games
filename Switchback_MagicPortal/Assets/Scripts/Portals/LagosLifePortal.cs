using System;
using System.Collections.Generic;

namespace TunnelGame
{
    /// <summary>
    /// Adapter for the Lagos Life (Vatar) portal. Wraps LagosLifeBridge, which
    /// does the actual JavaScript calls through LagosLifeBridge.jslib.
    /// </summary>
    public class LagosLifePortal : IGamePortal
    {
        private readonly LagosLifeBridge bridge;
        private readonly Queue<Action<PortalLeaderboardEntry[]>> leaderboardCallbacks = new Queue<Action<PortalLeaderboardEntry[]>>();

        public LagosLifePortal(LagosLifeBridge bridge)
        {
            this.bridge = bridge;

            bridge.Paused += () => { if (Paused != null) Paused(); };
            bridge.Resumed += () => { if (Resumed != null) Resumed(); };
            bridge.LeaderboardReceived += OnLeaderboard;
            bridge.LeaderboardFailed += () => OnLeaderboard(null);
        }

        public string Name { get { return "LagosLife"; } }

        public PortalPlayer Player
        {
            get
            {
                LagosPlayer p = bridge.Player;
                return p == null ? null : new PortalPlayer { id = p.playerId, name = p.playerName, avatar = p.avatar };
            }
        }

        public event Action Paused;
        public event Action Resumed;

        // Vatar.init() already succeeded before PortalManager picked this portal.
        public void Init() { }

        // Lagos Life has no live-score channel; only final scores are submitted.
        public void ReportProgress(int score) { }

        public void SubmitFinalScore(int score)
        {
            bridge.SubmitScore(score);
        }

        // Nothing to prepare - Lagos Life sessions don't need restarting per run.
        public void RequestRestart() { }

        public void RequestLeaderboard(Action<PortalLeaderboardEntry[]> onResult)
        {
            leaderboardCallbacks.Enqueue(onResult);
            bridge.RequestLeaderboard();
        }

        private void OnLeaderboard(LagosLeaderboard board)
        {
            if (leaderboardCallbacks.Count == 0) return;

            Action<PortalLeaderboardEntry[]> callback = leaderboardCallbacks.Dequeue();
            if (callback == null) return;

            if (board == null || board.top == null)
            {
                callback(null);
                return;
            }

            PortalLeaderboardEntry[] entries = new PortalLeaderboardEntry[board.top.Length];

            for (int i = 0; i < board.top.Length; i++)
            {
                entries[i] = new PortalLeaderboardEntry { rank = board.top[i].rank, name = board.top[i].name, score = board.top[i].score };
            }

            callback(entries);
        }
    }
}
