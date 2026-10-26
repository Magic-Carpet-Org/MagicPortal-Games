using System;
using UnityEngine;

namespace TunnelGame
{
    /// <summary>
    /// Adapter for the MagicGames portal. Wraps the existing PortalBridge
    /// (postMessage) and ApiManager (REST) without changing either, and
    /// reproduces exactly what GameManager used to do with them.
    /// </summary>
    public class MagicGamesPortal : IGamePortal
    {
        private readonly PortalBridge bridge;
        private PortalPlayer player;

        public MagicGamesPortal(PortalBridge bridge)
        {
            this.bridge = bridge;
        }

        public string Name { get { return "MagicGames"; } }
        public PortalPlayer Player { get { return player; } }

#pragma warning disable 67 // MagicGames has no pause/resume messages
        public event Action Paused;
        public event Action Resumed;
#pragma warning restore 67

        /// <summary>True when the URL carried a sessionId (a real portal session).</summary>
        private bool HasSession { get { return bridge != null && bridge.IsPortalSession; } }

        public void Init()
        {
            if (!HasSession || ApiManager.Instance == null) return;

            ApiManager.Instance.InitializeGame(bridge.CurrentSessionId, (success, response) =>
            {
                if (success && response != null && response.player != null)
                {
                    player = new PortalPlayer
                    {
                        id = response.player.id,
                        name = !string.IsNullOrEmpty(response.player.username) ? response.player.username : response.player.name,
                        avatar = response.player.avatar
                    };

                    Debug.Log("[MagicGamesPortal] Session initialized for player: " + player.name);
                }
                else
                {
                    Debug.LogWarning("[MagicGamesPortal] Session init failed.");
                }
            });
        }

        public void ReportProgress(int score)
        {
            if (bridge != null) bridge.SendScoreUpdate(score);
        }

        public void SubmitFinalScore(int score)
        {
            if (bridge == null) return;

            bridge.SendMatchComplete(score);

            // Authoritative, signed score over REST.
            if (HasSession && ApiManager.Instance != null)
            {
                ApiManager.Instance.SubmitScore(bridge.CurrentSessionId, score, (success, response) =>
                {
                    if (success)
                    {
                        Debug.Log("[MagicGamesPortal] Score submitted. Personal best: " + response.is_personal_best);
                    }
                    else
                    {
                        Debug.LogWarning("[MagicGamesPortal] Score submission failed.");
                    }
                });
            }
        }

        public void RequestRestart()
        {
            // Ask for a fresh session now so it's ready by the time the player replays.
            if (bridge != null) bridge.RequestRestart();
        }

        public void RequestLeaderboard(Action<PortalLeaderboardEntry[]> onResult)
        {
            // MagicGames has no leaderboard endpoint in the current contract.
            if (onResult != null) onResult(null);
        }
    }
}
