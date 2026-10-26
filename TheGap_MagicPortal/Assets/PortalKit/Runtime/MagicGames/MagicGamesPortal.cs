using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kilimanjaro.Portals
{
    /// <summary>
    /// Adapter for the MagicGames portal (postMessage bridge + REST API).
    ///
    /// Restart flow: each run needs its own session. RequestRestart() (called by
    /// PortalManager.GameOver) asks the portal for one straight away, so it is
    /// usually already there when the player taps Play Again. WaitForNextRun()
    /// fires as soon as that new session has arrived.
    /// </summary>
    public class MagicGamesPortal : IGamePortal
    {
        private readonly MagicGamesBridge bridge;
        private readonly MagicGamesApi api;
        private PortalPlayer player;

        private bool restartPending;
        private bool nextSessionReady;
        private readonly List<Action> nextRunWaiters = new List<Action>();

        public MagicGamesPortal(MagicGamesBridge bridge, MagicGamesApi api)
        {
            this.bridge = bridge;
            this.api = api;

            if (bridge != null)
            {
                bridge.RestartGameReceived += OnNewSession;
            }
        }

        public string Name { get { return "MagicGames"; } }
        public PortalPlayer Player { get { return player; } }

#pragma warning disable 67 // MagicGames has no pause/resume messages
        public event Action Paused;
        public event Action Resumed;
#pragma warning restore 67

        private bool HasSession { get { return bridge != null && bridge.IsPortalSession; } }

        public void Init(Action<bool> onDone)
        {
            // No session (direct link, Editor without a test session): nothing to
            // check with the backend, so the game may start.
            if (!HasSession || api == null)
            {
                if (onDone != null) onDone(true);
                return;
            }

            InitSession(onDone);
        }

        public void ReportProgress(int score)
        {
            if (bridge != null) bridge.SendScoreUpdate(score);
        }

        public void SubmitFinalScore(int score)
        {
            if (bridge == null) return;

            bridge.SendMatchComplete(score);

            if (HasSession && api != null)
            {
                api.SubmitScore(bridge.CurrentSessionId, score, (success, response) =>
                {
                    if (success && response != null)
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
            if (bridge == null || restartPending) return;

            restartPending = true;
            nextSessionReady = false;
            bridge.RequestRestart();
        }

        public void WaitForNextRun(Action onReady)
        {
            // No session means no portal will answer - start straight away.
            if (!HasSession)
            {
                if (onReady != null) onReady();
                return;
            }

            if (nextSessionReady)
            {
                nextSessionReady = false;
                if (onReady != null) onReady();
                return;
            }

            if (onReady != null) nextRunWaiters.Add(onReady);

            // Nobody asked yet (e.g. game didn't call GameOver) - ask now.
            if (!restartPending) RequestRestart();
        }

        public void RequestLeaderboard(Action<PortalLeaderboardEntry[]> onResult)
        {
            // No leaderboard endpoint in the current MagicGames contract.
            if (onResult != null) onResult(null);
        }

        private void OnNewSession(string sessionId)
        {
            restartPending = false;

            // Refresh session/player info in the background; don't hold up the restart.
            if (HasSession && api != null)
            {
                InitSession(null);
            }

            if (nextRunWaiters.Count == 0)
            {
                nextSessionReady = true;
                return;
            }

            nextSessionReady = false;

            Action[] waiters = nextRunWaiters.ToArray();
            nextRunWaiters.Clear();

            foreach (Action waiter in waiters) waiter();
        }

        private void InitSession(Action<bool> onDone)
        {
            api.InitializeGame(bridge.CurrentSessionId, (success, response) =>
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
                else if (!success)
                {
                    Debug.LogWarning("[MagicGamesPortal] Session init failed.");
                }

                if (onDone != null) onDone(success);
            });
        }
    }
}
