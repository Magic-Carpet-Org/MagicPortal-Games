using UnityEngine;
using System;
using System.Runtime.InteropServices;

namespace Kilimanjaro.Portals
{
    /// <summary>
    /// Low-level postMessage link to the MagicGames portal (Switchback
    /// "Iframe postMessage Bridge": SCORE_UPDATE, MATCH_COMPLETE,
    /// REQUEST_RESTART / RESTART_GAME / RESTART_ACK).
    ///
    /// Created automatically by PortalManager. The .jslib delivers RESTART_GAME
    /// to the GameObject named exactly ObjectName.
    /// </summary>
    public class MagicGamesBridge : MonoBehaviour
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void PKMG_SendMessage(string json);
        [DllImport("__Internal")] private static extern void PKMG_RegisterListener();
#endif

        public const string ObjectName = "PortalKit_MagicGames";

        /// <summary>Fired after a new session id arrives from the portal (RESTART_ACK already sent).</summary>
        public event Action<string> RestartGameReceived;

        private string currentSessionId;

        public string CurrentSessionId { get { return currentSessionId; } }

        /// <summary>True if the URL carried a sessionId (a real MagicGames session).</summary>
        public bool IsPortalSession { get { return !string.IsNullOrEmpty(currentSessionId); } }

        private void Awake()
        {
            gameObject.name = ObjectName;

            if (transform.parent != null)
            {
                transform.SetParent(null, false);
            }

            DontDestroyOnLoad(gameObject);

            currentSessionId = ParseSessionIdFromUrl();

#if UNITY_EDITOR
            // Optional real session for testing against the backend from Play mode.
            currentSessionId = PortalSettings.Load().editorTestSessionId;
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
            PKMG_RegisterListener();
#endif
        }

        public void SendScoreUpdate(int score) { Send("SCORE_UPDATE", score); }

        public void SendMatchComplete(int score) { Send("MATCH_COMPLETE", score); }

        public void RequestRestart()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            PKMG_SendMessage("{\"type\":\"REQUEST_RESTART\"}");
#else
            // No portal to answer, so reply ourselves. Reuse the Editor test session
            // (a random id would just 404 on the backend), or stay session-less.
            string editorSession = PortalSettings.Load().editorTestSessionId;
            Debug.Log("[MagicGamesBridge] REQUEST_RESTART (simulated outside WebGL build, session '" + editorSession + "')");
            OnRestartGameFromJs(editorSession);
#endif
        }

        // Called by PortalKit_MagicGames.jslib via SendMessage - keep the name.
        public void OnRestartGameFromJs(string sessionId)
        {
            currentSessionId = sessionId;

#if UNITY_WEBGL && !UNITY_EDITOR
            PKMG_SendMessage("{\"type\":\"RESTART_ACK\"}");
#else
            Debug.Log("[MagicGamesBridge] RESTART_ACK (no-op outside WebGL build)");
#endif

            if (RestartGameReceived != null) RestartGameReceived(sessionId);
        }

        private void Send(string type, int score)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            PKMG_SendMessage("{\"type\":\"" + type + "\",\"score\":" + score + "}");
#else
            Debug.Log(string.Format("[MagicGamesBridge] {0} score={1} (no-op outside WebGL build)", type, score));
#endif
        }

        private static string ParseSessionIdFromUrl()
        {
            string url = null;

#if UNITY_WEBGL && !UNITY_EDITOR
            url = Application.absoluteURL;
#endif

            if (string.IsNullOrEmpty(url) || url.IndexOf('?') < 0) return null;

            string query = url.Substring(url.IndexOf('?') + 1);

            int hash = query.IndexOf('#');
            if (hash >= 0) query = query.Substring(0, hash);

            foreach (string pair in query.Split('&'))
            {
                string[] kv = pair.Split('=');

                if (kv.Length == 2 && kv[0] == "sessionId")
                {
                    return Uri.UnescapeDataString(kv[1]);
                }
            }

            return null;
        }
    }
}
